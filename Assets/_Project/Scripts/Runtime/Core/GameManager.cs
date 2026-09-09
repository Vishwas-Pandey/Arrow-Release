using System;
using System.Collections.Generic;
using ReleaseTheArrow.Gameplay;
using ReleaseTheArrow.Generation;
using ReleaseTheArrow.Save;
using UnityEngine;

namespace ReleaseTheArrow.Core
{
    /// Top-level orchestrator: owns the current save data, the active LevelSession, and the
    /// app's coarse state machine. Deliberately thin — gameplay rules live in LevelSession,
    /// puzzle rules live in BoardState/PuzzleSolver, this class only wires them to persistence
    /// and drives scene-level state transitions.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public SaveData SaveData { get; private set; }
        public LevelSession CurrentSession { get; private set; }
        public AppState State { get; private set; } = AppState.Boot;

        public event Action<AppState> StateChanged;
        public event Action LivesChanged;
        public event Action<int> ArrowReleased;
        public event Action<int> ArrowBlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveData = SaveSystem.Load();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveSystem.Save(SaveData);
        }

        private void OnApplicationQuit()
        {
            SaveSystem.Save(SaveData);
        }

        public void StartLevel(int levelId)
        {
            levelId = Mathf.Clamp(levelId, 1, DifficultyCurve.MaxLevel);
            var layout = LevelGenerator.Generate(levelId);
            AttachSession(new LevelSession(layout));
            PersistInProgress();
            SetState(AppState.Playing);
        }

        /// True if there's a saved in-progress attempt worth resuming (app was killed mid-level).
        public bool HasResumableLevel => SaveData.hasInProgress;

        public void ResumeInProgressLevel()
        {
            if (!SaveData.hasInProgress)
            {
                StartLevel(SaveData.highestUnlockedLevel);
                return;
            }

            var layout = LevelGenerator.Generate(SaveData.inProgress.levelId);
            var session = LevelSession.Restore(
                layout,
                SaveData.inProgress.removedArrowIds,
                SaveData.inProgress.lives,
                SaveData.inProgress.continuesUsed);
            AttachSession(session);

            State = session.State switch
            {
                LevelSessionState.GameOverAwaitingContinue => AppState.GameOver,
                LevelSessionState.GameOverFinal => AppState.GameOver,
                LevelSessionState.Complete => AppState.LevelComplete,
                _ => AppState.Playing
            };
            StateChanged?.Invoke(State);
        }

        public void RestartLevel()
        {
            if (CurrentSession == null) return;
            int levelId = CurrentSession.Layout.levelId;
            var layout = LevelGenerator.Generate(levelId); // same seed -> identical layout, fresh attempt
            AttachSession(new LevelSession(layout));
            PersistInProgress();
            SetState(AppState.Playing);
        }

        public TapResult TapArrow(int arrowId)
        {
            if (CurrentSession == null) return TapResult.Ignored;
            var result = CurrentSession.Tap(arrowId);
            PersistInProgress();
            return result;
        }

        /// Call only after every rewarded ad required for this continue has completed successfully.
        public bool GrantContinue()
        {
            if (CurrentSession == null || !CurrentSession.GrantContinue()) return false;
            PersistInProgress();
            SetState(AppState.Playing);
            return true;
        }

        public void GoToNextLevel()
        {
            int next = CurrentSession.Layout.levelId + 1;
            if (next > DifficultyCurve.MaxLevel)
            {
                SetState(AppState.FinalCompletion);
                return;
            }
            StartLevel(next);
        }

        public void SetSettings(bool music, bool sound, bool vibration)
        {
            SaveData.musicOn = music;
            SaveData.soundOn = sound;
            SaveData.vibrationOn = vibration;
            SaveSystem.Save(SaveData);
        }

        public void SetState(AppState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }

        private void AttachSession(LevelSession session)
        {
            CurrentSession = session;
            session.ArrowReleased += id => ArrowReleased?.Invoke(id);
            session.ArrowBlocked += id => ArrowBlocked?.Invoke(id);
            session.LivesChanged += () => LivesChanged?.Invoke();
            session.LevelCompleted += OnLevelCompleted;
            session.GameOverReached += OnGameOverReached;
        }

        private void PersistInProgress()
        {
            if (CurrentSession == null) return;
            SaveData.hasInProgress = true;
            SaveData.inProgress.levelId = CurrentSession.Layout.levelId;
            SaveData.inProgress.removedArrowIds = new List<int>(CurrentSession.RemovedArrowIdsInOrder);
            SaveData.inProgress.lives = CurrentSession.Lives;
            SaveData.inProgress.continuesUsed = CurrentSession.ContinuesUsed;
            SaveSystem.Save(SaveData);
        }

        private void OnLevelCompleted()
        {
            int completedId = CurrentSession.Layout.levelId;
            if (completedId >= SaveData.highestUnlockedLevel && completedId < DifficultyCurve.MaxLevel)
            {
                SaveData.highestUnlockedLevel = completedId + 1;
            }
            SaveSystem.ClearInProgress(SaveData);
            SetState(AppState.LevelComplete);
        }

        private void OnGameOverReached()
        {
            PersistInProgress();
            SetState(AppState.GameOver);
        }
    }
}
