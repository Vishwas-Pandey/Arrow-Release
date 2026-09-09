using System.Collections;
using NUnit.Framework;
using ReleaseTheArrow.Audio;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.Gameplay;
using ReleaseTheArrow.Generation;
using UnityEngine;
using UnityEngine.TestTools;

namespace ReleaseTheArrow.Tests
{
    /// End-to-end smoke tests: boots the real GameFlowController (the same object the actual
    /// game scene contains) and exercises it through GameManager, confirming the whole wiring —
    /// not just individual classes in isolation — works at runtime.
    public class GameBootPlayModeTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.Destroy(_root);

            // Clean up the DontDestroyOnLoad singletons the controller creates, so each test boots fresh.
            if (GameManager.Instance != null) Object.Destroy(GameManager.Instance.gameObject);
            if (AudioManager.Instance != null) Object.Destroy(AudioManager.Instance.gameObject);
        }

        private IEnumerator Boot()
        {
            _root = new GameObject("GameFlowController");
            _root.AddComponent<GameFlowController>();
            yield return null; // Awake
            yield return null; // Start
        }

        [UnityTest]
        public IEnumerator Boot_ReachesMainMenuWithNoErrors()
        {
            LogAssert.ignoreFailingMessages = false;
            yield return Boot();

            Assert.AreEqual(AppState.MainMenu, GameManager.Instance.State);
            Assert.IsNotNull(AudioManager.Instance);
        }

        [UnityTest]
        public IEnumerator StartLevel_EntersPlayingWithFullBoard()
        {
            yield return Boot();

            GameManager.Instance.StartLevel(3);
            yield return null;

            Assert.AreEqual(AppState.Playing, GameManager.Instance.State);
            Assert.AreEqual(3, GameManager.Instance.CurrentSession.Layout.levelId);
            Assert.Greater(GameManager.Instance.CurrentSession.Board.RemainingCount, 0);
        }

        [UnityTest]
        public IEnumerator TappingReleasableArrow_ReducesBoardCount_ThroughFullStack()
        {
            yield return Boot();
            GameManager.Instance.StartLevel(3);
            yield return null;

            var session = GameManager.Instance.CurrentSession;
            PuzzleSolver.TrySolve(session.Layout, out var order);
            int before = session.Board.RemainingCount;

            var result = GameManager.Instance.TapArrow(order[0]);

            Assert.AreEqual(TapResult.Released, result);
            Assert.AreEqual(before - 1, session.Board.RemainingCount);
        }

        [UnityTest]
        public IEnumerator CompletingLevel_UnlocksNextLevel()
        {
            yield return Boot();
            GameManager.Instance.StartLevel(2);
            yield return null;

            var session = GameManager.Instance.CurrentSession;
            PuzzleSolver.TrySolve(session.Layout, out var order);
            foreach (int id in order) GameManager.Instance.TapArrow(id);
            yield return null;

            Assert.AreEqual(AppState.LevelComplete, GameManager.Instance.State);
            Assert.GreaterOrEqual(GameManager.Instance.SaveData.highestUnlockedLevel, 3);
        }

        [UnityTest]
        public IEnumerator CompletingLevel_DoesNotResurrectInProgressFlag()
        {
            yield return Boot();
            GameManager.Instance.StartLevel(4);
            yield return null;

            var session = GameManager.Instance.CurrentSession;
            PuzzleSolver.TrySolve(session.Layout, out var order);
            foreach (int id in order) GameManager.Instance.TapArrow(id);
            yield return null;

            Assert.IsFalse(GameManager.Instance.SaveData.hasInProgress,
                "A tap that completes the level must not re-mark the (now finished) level as in-progress.");
        }
    }
}
