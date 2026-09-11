using System.Collections.Generic;
using NUnit.Framework;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.Gameplay;
using ReleaseTheArrow.Generation;
using ReleaseTheArrow.LevelSystem;

namespace ReleaseTheArrow.Tests
{
    public class PuzzleGenerationTests
    {
        [Test]
        public void BoardState_RightArrow_BlockedThenClearedAfterNeighborRemoved()
        {
            // Two cells in a row: (0,0) points Right, blocked by (1,0). Remove (1,0), then (0,0) is free.
            var layout = new LevelLayout
            {
                width = 3,
                height = 1,
                arrows = new List<ArrowSpec>
                {
                    new ArrowSpec(0, 0, 0, ArrowDirection.Right),
                    new ArrowSpec(1, 1, 0, ArrowDirection.Up) // occupies (1,0), direction irrelevant to blocking
                }
            };
            var board = BoardState.FromLayout(layout);

            Assert.IsFalse(board.IsReleasable(0), "Arrow 0 should be blocked by arrow 1 in its path.");
            Assert.IsTrue(board.TryRelease(1));
            Assert.IsTrue(board.IsReleasable(0), "Arrow 0 should be free once the blocker is removed.");
        }

        [Test]
        public void BoardState_TryRelease_FailsWhenBlocked_AndDoesNotMutateState()
        {
            var layout = new LevelLayout
            {
                width = 2,
                height = 1,
                arrows = new List<ArrowSpec>
                {
                    new ArrowSpec(0, 0, 0, ArrowDirection.Right),
                    new ArrowSpec(1, 1, 0, ArrowDirection.Left)
                }
            };
            var board = BoardState.FromLayout(layout);

            Assert.IsFalse(board.TryRelease(0));
            Assert.AreEqual(2, board.RemainingCount, "A failed release must not remove the arrow.");
        }

        [Test]
        public void PuzzleSolver_DetectsGenuineDeadlock()
        {
            // Two arrows facing each other with nothing beyond them: each blocks the other, forever.
            var layout = new LevelLayout
            {
                width = 2,
                height = 1,
                arrows = new List<ArrowSpec>
                {
                    new ArrowSpec(0, 0, 0, ArrowDirection.Right),
                    new ArrowSpec(1, 1, 0, ArrowDirection.Left)
                }
            };
            Assert.IsFalse(PuzzleSolver.IsSolvable(layout));
        }

        [Test]
        public void PuzzleSolver_SolvesSimpleChain()
        {
            // (0,0) Right blocked by (1,0); (1,0) Right is clear to the edge at width=2.
            var layout = new LevelLayout
            {
                width = 2,
                height = 1,
                arrows = new List<ArrowSpec>
                {
                    new ArrowSpec(0, 0, 0, ArrowDirection.Right),
                    new ArrowSpec(1, 1, 0, ArrowDirection.Right)
                }
            };
            Assert.IsTrue(PuzzleSolver.TrySolve(layout, out var order));
            Assert.AreEqual(2, order.Count);
            Assert.AreEqual(1, order[0], "Arrow 1 (unblocked) must be removed before arrow 0.");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(20)]
        [TestCase(21)]
        [TestCase(100)]
        [TestCase(300)]
        [TestCase(600)]
        [TestCase(1000)]
        [TestCase(1300)]
        [TestCase(1450)]
        [TestCase(1500)]
        public void LevelGenerator_ProducesSolvableLayout(int levelId)
        {
            var layout = LevelGenerator.Generate(levelId);
            Assert.Greater(layout.ArrowCount, 0);
            Assert.IsTrue(PuzzleSolver.IsSolvable(layout), $"Level {levelId} must be solvable.");
        }

        [Test]
        public void LevelGenerator_IsDeterministic_SameLevelIdProducesIdenticalLayout()
        {
            var a = LevelGenerator.Generate(777);
            var b = LevelGenerator.Generate(777);

            Assert.AreEqual(a.width, b.width);
            Assert.AreEqual(a.height, b.height);
            Assert.AreEqual(a.arrows.Count, b.arrows.Count);
            for (int i = 0; i < a.arrows.Count; i++)
            {
                Assert.AreEqual(a.arrows[i].col, b.arrows[i].col);
                Assert.AreEqual(a.arrows[i].row, b.arrows[i].row);
                Assert.AreEqual(a.arrows[i].direction, b.arrows[i].direction);
            }
        }

        [Test]
        public void LevelGenerator_DifficultyGenerallyIncreasesWithLevel()
        {
            var early = LevelGenerator.Generate(5);
            var late = LevelGenerator.Generate(1490);
            Assert.Greater(late.ArrowCount, early.ArrowCount);
        }

        [Test]
        public void LevelGenerator_NoTwoArrowsShareACell()
        {
            var layout = LevelGenerator.Generate(950);
            var seen = new HashSet<(int, int)>();
            foreach (var arrow in layout.arrows)
            {
                Assert.IsTrue(seen.Add((arrow.col, arrow.row)), "Two arrows must not occupy the same cell.");
            }
        }

        [Test]
        public void LevelGenerator_Level1IsSmallAndSparse_NotFullyPacked()
        {
            var layout = LevelGenerator.Generate(1);
            Assert.AreEqual(5, layout.width);
            Assert.AreEqual(5, layout.height);
            Assert.Less(layout.ArrowCount, layout.width * layout.height,
                "Level 1 should start with empty cells, not a fully packed board.");
            Assert.GreaterOrEqual(layout.ArrowCount, 8, "Level 1 should still be roughly the 10-15 arrow design target.");
        }

        [Test]
        public void LevelGenerator_FillFractionRisesWithinAFixedBoardSize()
        {
            // Levels 1-5 all share a 5x5 board (DifficultyCurve steps size every 5 levels) —
            // arrow count should still climb level to level as the fill fraction ramps.
            int previousCount = 0;
            for (int levelId = 1; levelId <= 5; levelId++)
            {
                var layout = LevelGenerator.Generate(levelId);
                Assert.AreEqual(5, layout.width, $"Level {levelId} should still be on the 5x5 board.");
                Assert.GreaterOrEqual(layout.ArrowCount, previousCount,
                    $"Arrow count should not decrease from level {levelId - 1} to {levelId}.");
                previousCount = layout.ArrowCount;
            }
        }

        [Test]
        public void LevelGenerator_FullyPackedOnceMaxBoardSizeIsReached()
        {
            var atCap = LevelGenerator.Generate(DifficultyCurve.RampEndLevel);
            Assert.AreEqual(DifficultyCurve.MaxBoardSize, atCap.width);
            Assert.AreEqual(atCap.width * atCap.height, atCap.ArrowCount,
                "Once the board reaches its max size, it should be completely packed with no empty cells.");

            var finalLevel = LevelGenerator.Generate(DifficultyCurve.MaxLevel);
            Assert.AreEqual(DifficultyCurve.MaxBoardSize, finalLevel.width);
            Assert.AreEqual(finalLevel.width * finalLevel.height, finalLevel.ArrowCount);
        }
    }
}
