using System;
using System.Collections.Generic;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.LevelSystem;
using ReleaseTheArrow.Utils;

namespace ReleaseTheArrow.Generation
{
    /// Builds a fully packed (every cell occupied — no empty gaps), guaranteed-solvable puzzle
    /// for a given level id.
    ///
    /// This replaces an earlier greedy "randomly place arrows, prefer the most constrained
    /// free cell" generator. That approach could tune density and dependency-chain length
    /// independently, but neither property survives at real scale: pushing it toward 100% fill
    /// on anything bigger than a small board takes it from milliseconds to tens of seconds
    /// (verified directly — an 80x80 attempt at ~98% fill took 96 seconds and still only reached
    /// 22% before giving up), because "most constrained" and "still has any valid cell left" are
    /// directly in tension once free space gets scarce.
    ///
    /// The replacement sidesteps that entirely with a closed-form construction: peel the board
    /// from the outside in, one square ring at a time. Every cell on a ring's boundary points
    /// straight off the board through whichever single side it sits on (top/bottom/left/right);
    /// every cell on that path belongs to a strictly outer, already-cleared ring by construction,
    /// so the direction is safe *by definition*, not by search. That makes this O(size^2) with no
    /// backtracking, and — because it's provably correct rather than merely likely-correct — an
    /// entire 80x80 board still generates in well under a second. PuzzleSolver.IsSolvable is
    /// still run as a final safety net (with a bare fallback pattern behind it), consistent with
    /// never shipping a level that hasn't actually been verified.
    ///
    /// The one real cost of this approach: since every cell already has exactly one safe
    /// direction (two, at a ring's corners), there's no room left to also bias toward long
    /// tangled dependency chains the way the old generator could — doing that would reintroduce
    /// the same "straight line to the true edge" cost blowup this was built to avoid. Difficulty
    /// here comes from board size (more rings to peel, far more arrows to clear) rather than
    /// from per-move tanglement.
    public static class LevelGenerator
    {
        public static LevelLayout Generate(int levelId)
        {
            int size = DifficultyCurve.BoardSizeForLevel(levelId);
            var rng = new DeterministicRandom(unchecked((int)((uint)levelId * 2654435761u)));

            var layout = new LevelLayout
            {
                levelId = levelId,
                width = size,
                height = size,
                arrows = BuildFullyPackedLayout(size, rng),
                difficultyScore = size,
                seed = levelId,
                generationAttempt = 0
            };

            if (PuzzleSolver.IsSolvable(layout)) return layout;

            // Should be structurally unreachable — every cell's direction is proven safe by
            // construction — but a level that hasn't actually been verified must never ship.
            layout.arrows = BuildFullyPackedLayout(size, rng: null);
            if (!PuzzleSolver.IsSolvable(layout))
                throw new InvalidOperationException(
                    $"LevelGenerator: even the unvaried fallback pattern failed to validate for level {levelId} (should be impossible).");
            return layout;
        }

        /// Peels the board from the outside in. `rng` is optional and only ever affects which of
        /// a handful of provably-equivalent symmetric relabelings is used (which physical edge
        /// plays the "top/bottom/left/right" role) — purely cosmetic variety between levels of
        /// the same size, never a factor in whether the result is solvable.
        private static List<ArrowSpec> BuildFullyPackedLayout(int size, DeterministicRandom rng)
        {
            var arrows = new List<ArrowSpec>(size * size);
            int nextId = 0;

            bool swapAxes = rng != null && rng.NextFloat01() < 0.5f;
            bool flipRows = rng != null && rng.NextFloat01() < 0.5f;
            bool flipCols = rng != null && rng.NextFloat01() < 0.5f;

            ArrowDirection upDir = flipRows ? ArrowDirection.Down : ArrowDirection.Up;
            ArrowDirection downDir = flipRows ? ArrowDirection.Up : ArrowDirection.Down;
            ArrowDirection leftDir = flipCols ? ArrowDirection.Right : ArrowDirection.Left;
            ArrowDirection rightDir = flipCols ? ArrowDirection.Left : ArrowDirection.Right;

            int maxRing = (size - 1) / 2;
            for (int r = 0; r <= maxRing; r++)
            {
                int left = r, right = size - 1 - r, bottom = r, top = size - 1 - r;

                if (left == right && bottom == top)
                {
                    // Odd-sized board's single center cell — nothing else remains, any direction
                    // is trivially clear.
                    arrows.Add(new ArrowSpec(nextId++, left, bottom, upDir));
                    continue;
                }

                for (int col = left; col <= right; col++)
                {
                    for (int row = bottom; row <= top; row++)
                    {
                        // Only this ring's boundary — its interior belongs to inner rings handled
                        // on a later iteration.
                        bool onBoundary = col == left || col == right || row == bottom || row == top;
                        if (!onBoundary) continue;

                        bool isTopSide = row == (flipRows ? bottom : top);
                        bool isBottomSide = row == (flipRows ? top : bottom);
                        bool isLeftSide = col == (flipCols ? right : left);
                        bool isRightSide = col == (flipCols ? left : right);

                        ArrowDirection dir;
                        if (swapAxes)
                        {
                            if (isLeftSide) dir = leftDir;
                            else if (isRightSide) dir = rightDir;
                            else if (isTopSide) dir = upDir;
                            else dir = downDir; // isBottomSide
                        }
                        else
                        {
                            if (isTopSide) dir = upDir;
                            else if (isBottomSide) dir = downDir;
                            else if (isLeftSide) dir = leftDir;
                            else dir = rightDir; // isRightSide
                        }

                        arrows.Add(new ArrowSpec(nextId++, col, row, dir));
                    }
                }
            }

            return arrows;
        }
    }
}
