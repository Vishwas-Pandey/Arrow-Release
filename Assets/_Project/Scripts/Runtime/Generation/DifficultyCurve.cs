namespace ReleaseTheArrow.Generation
{
    /// Maps a level id (1..1500) to a board size. Every board is fully packed (every cell holds
    /// an arrow — see LevelGenerator), so difficulty now comes entirely from board size (more
    /// arrows, more rings to peel through) rather than from a tunable density/constrainedness
    /// curve on a fixed-size board.
    public static class DifficultyCurve
    {
        public const int MaxLevel = 1500;

        /// Level 1 is a 5x5 board; every 5 levels the board grows by one row/column (6x6 at
        /// level 6, 7x7 at level 11, ...) up to MaxBoardSize, reached at level 376 and held for
        /// the rest of the game.
        public const int MaxBoardSize = 80;

        public static int BoardSizeForLevel(int levelId)
        {
            levelId = Clamp(levelId, 1, MaxLevel);
            int step = (levelId - 1) / 5;
            return Clamp(5 + step, 5, MaxBoardSize);
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
    }
}
