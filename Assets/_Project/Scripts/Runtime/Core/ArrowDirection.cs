namespace ReleaseTheArrow.Core
{
    public enum ArrowDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    public static class ArrowDirectionExtensions
    {
        public static readonly ArrowDirection[] All =
        {
            ArrowDirection.Up, ArrowDirection.Down, ArrowDirection.Left, ArrowDirection.Right
        };

        public static void ToStep(this ArrowDirection direction, out int dCol, out int dRow)
        {
            switch (direction)
            {
                case ArrowDirection.Up: dCol = 0; dRow = 1; return;
                case ArrowDirection.Down: dCol = 0; dRow = -1; return;
                case ArrowDirection.Left: dCol = -1; dRow = 0; return;
                default: dCol = 1; dRow = 0; return; // Right
            }
        }
    }
}
