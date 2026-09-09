using System;
using System.Collections.Generic;
using ReleaseTheArrow.LevelSystem;

namespace ReleaseTheArrow.Gameplay
{
    public enum TapResult { Released, Blocked, Ignored }

    public enum LevelSessionState { Playing, GameOverAwaitingContinue, GameOverFinal, Complete }

    /// The rules engine for one level attempt: lives, the progressive ad-continue table, and
    /// exact puzzle-state preservation across continues. Pure C# (no Unity/MonoBehaviour
    /// dependency) so it's directly unit-testable and reusable between a fresh session and one
    /// restored from a save file after the app was killed mid-level.
    public class LevelSession
    {
        public const int StartingLives = 3;
        public const int MaxContinues = 3;

        public LevelLayout Layout { get; }
        public BoardState Board { get; }
        public int Lives { get; private set; }
        public int ContinuesUsed { get; private set; }
        public LevelSessionState State { get; private set; }

        private readonly List<int> _removedIdsInOrder = new List<int>();
        public IReadOnlyList<int> RemovedArrowIdsInOrder => _removedIdsInOrder;

        public event Action<int> ArrowReleased;
        public event Action<int> ArrowBlocked;
        public event Action LivesChanged;
        public event Action LevelCompleted;
        public event Action GameOverReached;

        public LevelSession(LevelLayout layout)
        {
            Layout = layout;
            Board = BoardState.FromLayout(layout);
            Lives = StartingLives;
            ContinuesUsed = 0;
            State = LevelSessionState.Playing;
        }

        /// Rebuilds a session exactly where a previous attempt left off: same deterministic
        /// layout, replaying the stored removal order onto a fresh BoardState. No shuffling,
        /// no regenerating, no restoring already-removed arrows — the board ends up in the
        /// identical state it was in when the game was last saved.
        public static LevelSession Restore(LevelLayout layout, IReadOnlyList<int> removedArrowIdsInOrder, int lives, int continuesUsed)
        {
            var session = new LevelSession(layout);
            foreach (int id in removedArrowIdsInOrder)
            {
                if (!session.Board.TryRelease(id))
                    throw new InvalidOperationException(
                        $"Corrupt save: arrow {id} was not releasable during replay for level {layout.levelId}.");
                session._removedIdsInOrder.Add(id);
            }

            session.Lives = lives;
            session.ContinuesUsed = continuesUsed;

            if (session.Board.RemainingCount == 0) session.State = LevelSessionState.Complete;
            else if (lives <= 0) session.State = continuesUsed < MaxContinues
                ? LevelSessionState.GameOverAwaitingContinue
                : LevelSessionState.GameOverFinal;
            else session.State = LevelSessionState.Playing;

            return session;
        }

        public TapResult Tap(int arrowId)
        {
            if (State != LevelSessionState.Playing) return TapResult.Ignored;
            if (!Board.IsActive(arrowId)) return TapResult.Ignored;

            if (Board.TryRelease(arrowId))
            {
                _removedIdsInOrder.Add(arrowId);
                ArrowReleased?.Invoke(arrowId);

                if (Board.RemainingCount == 0)
                {
                    State = LevelSessionState.Complete;
                    LevelCompleted?.Invoke();
                }
                return TapResult.Released;
            }

            Lives--;
            LivesChanged?.Invoke();
            ArrowBlocked?.Invoke(arrowId);

            if (Lives <= 0)
            {
                State = ContinuesUsed < MaxContinues
                    ? LevelSessionState.GameOverAwaitingContinue
                    : LevelSessionState.GameOverFinal;
                GameOverReached?.Invoke();
            }
            return TapResult.Blocked;
        }

        /// How many rewarded ads must complete before the next continue grants its 1 life.
        /// 0 means no continue is available (the fourth knockout: must restart).
        public int AdsRequiredForNextContinue()
        {
            if (State != LevelSessionState.GameOverAwaitingContinue) return 0;
            return ContinuesUsed + 1;
        }

        /// Call only after every required rewarded ad for this continue has successfully
        /// completed. Restores exactly 1 life and resumes the same puzzle in place.
        public bool GrantContinue()
        {
            if (State != LevelSessionState.GameOverAwaitingContinue) return false;
            if (ContinuesUsed >= MaxContinues) return false;

            ContinuesUsed++;
            Lives = 1;
            State = LevelSessionState.Playing;
            LivesChanged?.Invoke();
            return true;
        }
    }
}
