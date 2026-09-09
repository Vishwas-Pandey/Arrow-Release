using System;

namespace ReleaseTheArrow.Core
{
    /// Immutable description of a single arrow's placement on a level's grid.
    [Serializable]
    public struct ArrowSpec
    {
        public int id;
        public int col;
        public int row;
        public ArrowDirection direction;

        public ArrowSpec(int id, int col, int row, ArrowDirection direction)
        {
            this.id = id;
            this.col = col;
            this.row = row;
            this.direction = direction;
        }
    }
}
