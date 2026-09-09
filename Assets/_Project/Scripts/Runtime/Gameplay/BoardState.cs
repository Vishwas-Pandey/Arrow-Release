using System.Collections.Generic;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.LevelSystem;

namespace ReleaseTheArrow.Gameplay
{
    /// Pure grid/occupancy model for one puzzle board. No Unity dependencies —
    /// used identically by the puzzle solver (generation-time validation) and
    /// by BoardController at runtime, so the blocking rule is defined exactly once.
    public class BoardState
    {
        public readonly int Width;
        public readonly int Height;

        private readonly Dictionary<int, ArrowSpec> _activeById = new Dictionary<int, ArrowSpec>();
        private readonly int[,] _grid; // arrow id occupying a cell, or -1 if empty/removed

        public int RemainingCount => _activeById.Count;

        public BoardState(int width, int height, IEnumerable<ArrowSpec> arrows)
        {
            Width = width;
            Height = height;
            _grid = new int[width, height];
            for (int c = 0; c < width; c++)
                for (int r = 0; r < height; r++)
                    _grid[c, r] = -1;

            foreach (var arrow in arrows)
            {
                _activeById[arrow.id] = arrow;
                _grid[arrow.col, arrow.row] = arrow.id;
            }
        }

        public static BoardState FromLayout(LevelLayout layout) => new BoardState(layout.width, layout.height, layout.arrows);

        public bool IsActive(int id) => _activeById.ContainsKey(id);

        public bool TryGetArrow(int id, out ArrowSpec arrow) => _activeById.TryGetValue(id, out arrow);

        public IEnumerable<ArrowSpec> ActiveArrows => _activeById.Values;

        /// True if nothing occupies the cells between this arrow and the board edge, in its own direction.
        public bool IsReleasable(int id)
        {
            if (!_activeById.TryGetValue(id, out var arrow)) return false;
            return IsPathClear(arrow);
        }

        private bool IsPathClear(ArrowSpec arrow)
        {
            arrow.direction.ToStep(out int dCol, out int dRow);
            int c = arrow.col + dCol;
            int r = arrow.row + dRow;
            while (c >= 0 && c < Width && r >= 0 && r < Height)
            {
                if (_grid[c, r] != -1) return false;
                c += dCol;
                r += dRow;
            }
            return true;
        }

        /// Removes the arrow if (and only if) it is currently releasable. Returns false without changing state otherwise.
        public bool TryRelease(int id)
        {
            if (!IsReleasable(id)) return false;
            var arrow = _activeById[id];
            _grid[arrow.col, arrow.row] = -1;
            _activeById.Remove(id);
            return true;
        }

        /// All currently active arrow ids whose path is clear right now. O(N * (W+H)).
        public List<int> GetAllReleasableIds()
        {
            var result = new List<int>();
            foreach (var kvp in _activeById)
            {
                if (IsPathClear(kvp.Value)) result.Add(kvp.Key);
            }
            return result;
        }

        /// Cheap incremental check: only arrows sharing the removed cell's row or column can have
        /// changed status, so gameplay only needs to re-test those instead of the whole board.
        public List<int> GetNewlyReleasableInRowOrColumn(int col, int row)
        {
            var result = new List<int>();
            for (int c = 0; c < Width; c++)
            {
                int id = _grid[c, row];
                if (id != -1 && IsPathClear(_activeById[id])) result.Add(id);
            }
            for (int r = 0; r < Height; r++)
            {
                if (r == row) continue; // already covered by the row scan above at (col,row)
                int id = _grid[col, r];
                if (id != -1 && IsPathClear(_activeById[id])) result.Add(id);
            }
            return result;
        }
    }
}
