using System.Linq;
using NUnit.Framework;
using ReleaseTheArrow.Gameplay;
using ReleaseTheArrow.Generation;

namespace ReleaseTheArrow.Tests
{
    public class LevelSessionTests
    {
        private static LevelSession NewSession(int levelId = 1) => new LevelSession(LevelGenerator.Generate(levelId));

        [Test]
        public void NewSession_StartsWithThreeLivesAndZeroContinues()
        {
            var session = NewSession();
            Assert.AreEqual(3, session.Lives);
            Assert.AreEqual(0, session.ContinuesUsed);
            Assert.AreEqual(LevelSessionState.Playing, session.State);
        }

        [Test]
        public void TappingReleasableArrow_DoesNotCostALife()
        {
            var session = NewSession();
            var solvable = PuzzleSolver.TrySolve(session.Layout, out var order);
            Assert.IsTrue(solvable);

            session.Tap(order[0]);
            Assert.AreEqual(3, session.Lives, "A successful release must never consume a life.");
        }

        [Test]
        public void TappingBlockedArrow_CostsExactlyOneLife()
        {
            var session = NewSession();
            PuzzleSolver.TrySolve(session.Layout, out var order);
            // The last arrow in a valid solve order is blocked at the very start (everything
            // else is still on the board), unless the level has only one arrow.
            int blockedId = order[order.Count - 1];
            if (session.Board.IsReleasable(blockedId)) Assert.Ignore("Level too trivial to have a blocked first arrow.");

            var result = session.Tap(blockedId);
            Assert.AreEqual(TapResult.Blocked, result);
            Assert.AreEqual(2, session.Lives);
        }

        [Test]
        public void ThreeBlockedTaps_TriggerGameOverAwaitingFirstContinue()
        {
            var session = NewSession();
            PuzzleSolver.TrySolve(session.Layout, out var order);
            int blockedId = order[order.Count - 1];
            if (session.Board.IsReleasable(blockedId)) Assert.Ignore("Level too trivial to have a blocked first arrow.");

            session.Tap(blockedId);
            session.Tap(blockedId);
            session.Tap(blockedId);

            Assert.AreEqual(0, session.Lives);
            Assert.AreEqual(LevelSessionState.GameOverAwaitingContinue, session.State);
            Assert.AreEqual(1, session.AdsRequiredForNextContinue(), "First continue must require exactly 1 ad.");
        }

        [Test]
        public void ContinueTable_RequiresProgressivelyMoreAds_ThenForcesRestart()
        {
            var session = NewSession();
            PuzzleSolver.TrySolve(session.Layout, out var order);
            int blockedId = order[order.Count - 1];
            if (session.Board.IsReleasable(blockedId)) Assert.Ignore("Level too trivial to have a blocked first arrow.");

            void KnockOut()
            {
                for (int i = 0; i < 3 && session.State == LevelSessionState.Playing; i++)
                    session.Tap(blockedId);
            }

            KnockOut();
            Assert.AreEqual(1, session.AdsRequiredForNextContinue());
            Assert.IsTrue(session.GrantContinue());
            Assert.AreEqual(1, session.Lives);
            Assert.AreEqual(LevelSessionState.Playing, session.State);

            KnockOut();
            Assert.AreEqual(2, session.AdsRequiredForNextContinue());
            Assert.IsTrue(session.GrantContinue());

            KnockOut();
            Assert.AreEqual(3, session.AdsRequiredForNextContinue());
            Assert.IsTrue(session.GrantContinue());

            KnockOut();
            Assert.AreEqual(LevelSessionState.GameOverFinal, session.State,
                "After three continues are used, a fourth knockout must force a restart with no further continue offered.");
            Assert.AreEqual(0, session.AdsRequiredForNextContinue());
            Assert.IsFalse(session.GrantContinue());
        }

        [Test]
        public void Restore_PreservesExactRemovedAndRemainingArrows()
        {
            var original = NewSession(50);
            PuzzleSolver.TrySolve(original.Layout, out var order);

            int removeCount = order.Count / 2;
            for (int i = 0; i < removeCount; i++) original.Tap(order[i]);

            var restored = LevelSession.Restore(original.Layout, original.RemovedArrowIdsInOrder, original.Lives, original.ContinuesUsed);

            Assert.AreEqual(original.Board.RemainingCount, restored.Board.RemainingCount);
            Assert.AreEqual(original.RemovedArrowIdsInOrder.Count, restored.RemovedArrowIdsInOrder.Count);
            CollectionAssert.AreEqual(original.RemovedArrowIdsInOrder, restored.RemovedArrowIdsInOrder);

            foreach (var arrow in original.Layout.arrows)
            {
                Assert.AreEqual(original.Board.IsActive(arrow.id), restored.Board.IsActive(arrow.id),
                    $"Arrow {arrow.id} active/removed state must match exactly after restore.");
            }
        }

        [Test]
        public void Restore_DoesNotRegenerateOrShuffleTheLayout()
        {
            var original = NewSession(123);
            var restored = LevelSession.Restore(original.Layout, System.Array.Empty<int>(), 3, 0);

            Assert.AreEqual(original.Layout.width, restored.Layout.width);
            Assert.AreEqual(original.Layout.height, restored.Layout.height);
            CollectionAssert.AreEqual(
                original.Layout.arrows.Select(a => (a.id, a.col, a.row, a.direction)),
                restored.Layout.arrows.Select(a => (a.id, a.col, a.row, a.direction)));
        }
    }
}
