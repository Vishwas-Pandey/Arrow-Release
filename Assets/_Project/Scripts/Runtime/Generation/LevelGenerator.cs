using System;
using System.Collections.Generic;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.LevelSystem;
using ReleaseTheArrow.Utils;

namespace ReleaseTheArrow.Generation
{
    /// Builds a guaranteed-solvable puzzle for a given level id, at whatever board size and
    /// fill fraction DifficultyCurve assigns to that level.
    ///
    /// The construction is a closed-form "peel the board from the outside in, one square ring
    /// at a time" rule: every cell on a ring's boundary points straight off the board through
    /// whichever single side it sits on (top/bottom/left/right). Every cell on that straight-line
    /// path belongs to a strictly outer ring (or is simply empty), so the direction is safe *by
    /// definition*, not by search — and this stays true no matter which subset of cells actually
    /// gets an arrow. An empty cell is always already "clear", so leaving cells out to hit a
    /// target fill fraction can never make an otherwise-safe direction unsafe. That's what lets
    /// this generator serve both ends of the game: a sparse level 1 board and a completely packed
    /// 80x80 finale use the exact same per-cell rule, just with a different subset of cells kept.
    ///
    /// This replaces an earlier greedy "randomly place arrows, prefer the most constrained free
    /// cell" generator, which could tune density directly but fell over at scale: pushing it
    /// toward high fill on anything bigger than a small board took it from milliseconds to tens
    /// of seconds (verified directly — an 80x80 attempt at ~98% fill took 96 seconds and still
    /// only reached 22% before giving up), because "most constrained" and "still has any valid
    /// cell left" are directly in tension once free space gets scarce. The ring-peel construction
    /// is O(size^2) with no backtracking regardless of target fill, so an 80x80 board — packed or
    /// not — still generates in well under a millisecond. PuzzleSolver.IsSolvable is still run as
    /// a final safety net (with a bare fallback pattern behind it), consistent with never shipping
    /// a level that hasn't actually been verified.
    public static class LevelGenerator
    {
        public static LevelLayout Generate(int levelId)
        {
            int size = DifficultyCurve.BoardSizeForLevel(levelId);
            float fillFraction = DifficultyCurve.FillFractionForLevel(levelId);
            var rng = new DeterministicRandom(unchecked((int)((uint)levelId * 2654435761u)));

            var layout = new LevelLayout
            {
                levelId = levelId,
                width = size,
                height = size,
                arrows = BuildLayout(size, rng, fillFraction),
                seed = levelId,
                generationAttempt = 0
            };
            layout.difficultyScore = layout.ArrowCount;

            if (PuzzleSolver.IsSolvable(layout)) return layout;

            // Should be structurally unreachable — every cell's direction is proven safe by
            // construction regardless of which subset of cells is kept — but a level that hasn't
            // actually been verified must never ship.
            layout.arrows = BuildLayout(size, rng: null, fillFraction: 1f);
            layout.difficultyScore = layout.ArrowCount;
            if (!PuzzleSolver.IsSolvable(layout))
                throw new InvalidOperationException(
                    $"LevelGenerator: even the unvaried fallback pattern failed to validate for level {levelId} (should be impossible).");
            return layout;
        }

        /// Peels the board from the outside in and keeps roughly `fillFraction` of its cells.
        /// `rng` is optional; when present it also picks which of a handful of provably-equivalent
        /// symmetric relabelings is used (purely cosmetic) and which cells survive the fill-fraction
        /// trim (never a factor in whether the result is solvable — see class remarks).
        private static List<ArrowSpec> BuildLayout(int size, DeterministicRandom rng, float fillFraction)
        {
            var cells = new List<(int col, int row, ArrowDirection dir)>(size * size);

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
                    cells.Add((left, bottom, upDir));
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

                        cells.Add((col, row, dir));
                    }
                }
            }

            int targetCount = fillFraction >= 1f
                ? cells.Count
                : Math.Clamp((int)Math.Round(cells.Count * fillFraction), 1, cells.Count);

            if (targetCount < cells.Count && rng != null)
            {
                rng.Shuffle(cells);
                cells.RemoveRange(targetCount, cells.Count - targetCount);
                // Sort back into reading order — the shuffle only needed to pick *which* cells
                // survive, not the order they're stored/rendered in.
                cells.Sort((a, b) => a.row != b.row ? a.row.CompareTo(b.row) : a.col.CompareTo(b.col));
            }

            var arrows = new List<ArrowSpec>(cells.Count);
            for (int i = 0; i < cells.Count; i++)
                arrows.Add(new ArrowSpec(i, cells[i].col, cells[i].row, cells[i].dir));
            return arrows;
        }
    }
}
