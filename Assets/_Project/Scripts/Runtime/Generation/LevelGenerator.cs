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
    /// The direction assignment is a randomized peel, not a fixed geometric rule. An early
    /// version pointed every cell straight at whichever single edge (top/bottom/left/right) it
    /// was structurally closest to — provably safe, but visually it meant "the whole left side
    /// points left, the whole right side points right", so the board always cleared as a few
    /// big uniform regions rather than feeling tangled. This version instead simulates the
    /// clearing process itself, injecting real randomness into both the order and the
    /// direction, while keeping every step provably safe:
    ///
    /// At any point during construction, a cell has a clear straight-line path off the board in
    /// direction Left/Right iff it is currently the leftmost/rightmost occupied cell in its row
    /// (nothing else remains between it and that edge); Up/Down work the same way for its
    /// column. So the set of "currently safe (cell, direction) pairs" is exactly: each
    /// non-empty row's current leftmost and rightmost occupied cell, plus each non-empty
    /// column's current bottommost and topmost. That set never exceeds roughly 2*rows + 2*cols
    /// cells, however big the board is. The generator repeatedly picks one uniformly at random,
    /// picks one of *its* currently-safe directions at random (a cell can have more than one),
    /// assigns that direction, and removes it — which only ever shrinks rows/columns further,
    /// so the invariant holds until every cell is placed. No backtracking, no failed attempts,
    /// and the frontier's small fixed size (not the whole board) keeps this fast even at an
    /// 80x80 fully-packed board (6400 cells): each removal is an O(1) linked-list update, so
    /// generation stays roughly O(size^2) overall. PuzzleSolver.IsSolvable is still run as a
    /// final safety net (with a bare fallback pattern behind it), consistent with never shipping
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
                arrows = BuildRandomizedPeelLayout(size, rng, fillFraction),
                seed = levelId,
                generationAttempt = 0
            };
            layout.difficultyScore = layout.ArrowCount;

            if (PuzzleSolver.IsSolvable(layout)) return layout;

            // Should be structurally unreachable — every direction is only ever assigned once
            // it's already proven safe — but a level that hasn't actually been verified must
            // never ship. Falls back to the simplest possible safe pattern: peel from the
            // outside in, one ring at a time.
            layout.arrows = BuildRingPeelFallbackLayout(size);
            layout.difficultyScore = layout.ArrowCount;
            if (!PuzzleSolver.IsSolvable(layout))
                throw new InvalidOperationException(
                    $"LevelGenerator: even the unvaried fallback pattern failed to validate for level {levelId} (should be impossible).");
            return layout;
        }

        private static List<ArrowSpec> BuildRandomizedPeelLayout(int size, DeterministicRandom rng, float fillFraction)
        {
            bool[,] occupied = ChooseOccupiedCells(size, rng, fillFraction);

            // Doubly linked lists of occupied cells, ordered by column within each row and by
            // row within each column — -1 is the "no such neighbor" sentinel throughout.
            var rowPrev = new int[size, size];
            var rowNext = new int[size, size];
            var colPrev = new int[size, size];
            var colNext = new int[size, size];
            var rowLeftmost = new int[size];
            var rowRightmost = new int[size];
            var colBottommost = new int[size];
            var colTopmost = new int[size];
            for (int i = 0; i < size; i++)
            {
                rowLeftmost[i] = rowRightmost[i] = -1;
                colBottommost[i] = colTopmost[i] = -1;
            }

            for (int row = 0; row < size; row++)
            {
                int prev = -1;
                for (int col = 0; col < size; col++)
                {
                    if (!occupied[col, row]) continue;
                    rowPrev[col, row] = prev;
                    if (prev == -1) rowLeftmost[row] = col; else rowNext[prev, row] = col;
                    prev = col;
                }
                if (prev != -1) rowNext[prev, row] = -1;
                rowRightmost[row] = prev;
            }

            for (int col = 0; col < size; col++)
            {
                int prev = -1;
                for (int row = 0; row < size; row++)
                {
                    if (!occupied[col, row]) continue;
                    colPrev[col, row] = prev;
                    if (prev == -1) colBottommost[col] = row; else colNext[col, prev] = row;
                    prev = row;
                }
                if (prev != -1) colNext[col, prev] = -1;
                colTopmost[col] = prev;
            }

            // Frontier of currently-safe cells, as a swap-remove list for O(1) random pick/removal.
            var frontierList = new List<int>();
            var frontierIndex = new Dictionary<int, int>();
            void AddToFrontier(int cellId)
            {
                if (frontierIndex.ContainsKey(cellId)) return;
                frontierIndex[cellId] = frontierList.Count;
                frontierList.Add(cellId);
            }
            void RemoveFromFrontier(int cellId)
            {
                int idx = frontierIndex[cellId];
                int lastIdx = frontierList.Count - 1;
                int lastCell = frontierList[lastIdx];
                frontierList[idx] = lastCell;
                frontierIndex[lastCell] = idx;
                frontierList.RemoveAt(lastIdx);
                frontierIndex.Remove(cellId);
            }
            int CellId(int col, int row) => row * size + col;

            for (int row = 0; row < size; row++)
            {
                if (rowLeftmost[row] != -1) AddToFrontier(CellId(rowLeftmost[row], row));
                if (rowRightmost[row] != -1) AddToFrontier(CellId(rowRightmost[row], row));
            }
            for (int col = 0; col < size; col++)
            {
                if (colBottommost[col] != -1) AddToFrontier(CellId(col, colBottommost[col]));
                if (colTopmost[col] != -1) AddToFrontier(CellId(col, colTopmost[col]));
            }

            var validDirs = new List<ArrowDirection>(4);
            var placed = new List<ArrowSpec>();
            int nextId = 0;

            while (frontierList.Count > 0)
            {
                int cellId = frontierList[rng.NextInt(0, frontierList.Count)];
                int col = cellId % size;
                int row = cellId / size;

                validDirs.Clear();
                if (rowLeftmost[row] == col) validDirs.Add(ArrowDirection.Left);
                if (rowRightmost[row] == col) validDirs.Add(ArrowDirection.Right);
                if (colBottommost[col] == row) validDirs.Add(ArrowDirection.Down);
                if (colTopmost[col] == row) validDirs.Add(ArrowDirection.Up);

                var chosen = validDirs[rng.NextInt(0, validDirs.Count)];
                placed.Add(new ArrowSpec(nextId++, col, row, chosen));

                int rp = rowPrev[col, row], rn = rowNext[col, row];
                if (rp == -1) rowLeftmost[row] = rn; else rowNext[rp, row] = rn;
                if (rn == -1) rowRightmost[row] = rp; else rowPrev[rn, row] = rp;

                int cp = colPrev[col, row], cn = colNext[col, row];
                if (cp == -1) colBottommost[col] = cn; else colNext[col, cp] = cn;
                if (cn == -1) colTopmost[col] = cp; else colPrev[col, cn] = cp;

                RemoveFromFrontier(cellId);
                if (rowLeftmost[row] != -1) AddToFrontier(CellId(rowLeftmost[row], row));
                if (rowRightmost[row] != -1) AddToFrontier(CellId(rowRightmost[row], row));
                if (colBottommost[col] != -1) AddToFrontier(CellId(col, colBottommost[col]));
                if (colTopmost[col] != -1) AddToFrontier(CellId(col, colTopmost[col]));
            }

            placed.Sort((a, b) => a.row != b.row ? a.row.CompareTo(b.row) : a.col.CompareTo(b.col));
            for (int i = 0; i < placed.Count; i++)
            {
                var p = placed[i];
                p.id = i;
                placed[i] = p;
            }
            return placed;
        }

        private static bool[,] ChooseOccupiedCells(int size, DeterministicRandom rng, float fillFraction)
        {
            var occupied = new bool[size, size];
            var allCells = new List<(int col, int row)>(size * size);
            for (int col = 0; col < size; col++)
                for (int row = 0; row < size; row++)
                    allCells.Add((col, row));

            int targetCount = fillFraction >= 1f
                ? allCells.Count
                : Math.Clamp((int)Math.Round(allCells.Count * fillFraction), 1, allCells.Count);

            if (targetCount < allCells.Count)
            {
                rng.Shuffle(allCells);
                allCells.RemoveRange(targetCount, allCells.Count - targetCount);
            }

            foreach (var (col, row) in allCells) occupied[col, row] = true;
            return occupied;
        }

        /// Simplest possible always-safe pattern, used only if the randomized peel somehow
        /// failed validation: peel the board from the outside in, one ring at a time, every
        /// cell pointing straight off the board through whichever side of its ring it sits on.
        private static List<ArrowSpec> BuildRingPeelFallbackLayout(int size)
        {
            var arrows = new List<ArrowSpec>(size * size);
            int nextId = 0;
            int maxRing = (size - 1) / 2;
            for (int r = 0; r <= maxRing; r++)
            {
                int left = r, right = size - 1 - r, bottom = r, top = size - 1 - r;
                if (left == right && bottom == top)
                {
                    arrows.Add(new ArrowSpec(nextId++, left, bottom, ArrowDirection.Up));
                    continue;
                }
                for (int col = left; col <= right; col++)
                {
                    for (int row = bottom; row <= top; row++)
                    {
                        bool onBoundary = col == left || col == right || row == bottom || row == top;
                        if (!onBoundary) continue;
                        ArrowDirection dir = row == top ? ArrowDirection.Up
                            : row == bottom ? ArrowDirection.Down
                            : col == left ? ArrowDirection.Left
                            : ArrowDirection.Right;
                        arrows.Add(new ArrowSpec(nextId++, col, row, dir));
                    }
                }
            }
            return arrows;
        }
    }
}
