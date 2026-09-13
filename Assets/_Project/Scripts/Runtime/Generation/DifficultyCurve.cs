namespace ReleaseTheArrow.Generation
{
    /// Maps a level id (1..1500) to a board size and a fill fraction (what portion of the
    /// board's cells actually hold an arrow).
    ///
    /// The progression alternates two independent knobs, both driven purely by level id so they
    /// always move in lockstep with no jarring resets:
    ///   - BoardSizeForLevel steps up by one row/column every 5 levels (5x5 at level 1, 6x6 at
    ///     level 6, ... up to MaxBoardSize).
    ///   - FillFractionForLevel climbs smoothly and continuously from a sparse start toward
    ///     fully packed, reaching 1.0 exactly when the board size hits MaxBoardSize.
    ///
    /// The result is exactly the intended "start small, add more arrows, then grow the board,
    /// then add more arrows again" staircase: within a 5-level band the size holds still while
    /// the fill fraction (and so the arrow count) ticks up level by level; at the band boundary
    /// the size steps up while the fill curve continues uninterrupted. Once the board reaches
    /// MaxBoardSize the fill fraction has also reached 1.0 (completely packed, no empty cells)
    /// and both stay pinned there for the remainder of the game — the final stage is the
    /// hardest possible combination: the biggest board, completely full.
    public static class DifficultyCurve
    {
        public const int MaxLevel = 1500;

        public const int MinBoardSize = 5;
        public const int MaxBoardSize = 80;

        /// Level at which the board first reaches MaxBoardSize (5 levels per size step, sizes
        /// 5..80 is 76 steps of 5 levels = level 376), and also the level at which the fill
        /// fraction finishes ramping to 1.0.
        public const int RampEndLevel = 1 + (MaxBoardSize - MinBoardSize) * 5;

        /// Fraction of the board filled with arrows at level 1 — 0.4 on a 5x5 board is 10
        /// arrows, matching the original "10-15 arrows at level 1" design target.
        public const float StartFillFraction = 0.40f;

        public static int BoardSizeForLevel(int levelId)
        {
            levelId = Clamp(levelId, 1, MaxLevel);
            int step = (levelId - 1) / 5;
            return Clamp(MinBoardSize + step, MinBoardSize, MaxBoardSize);
        }

        /// Every level this many levels apart (after the opening band) eases the fill fraction
        /// back slightly before the ramp resumes — a deliberate "breather" so the climb reads as
        /// organic pressure-and-release rather than a perfectly rigid staircase. It never drops
        /// the fraction below what the player already faced BreatherPeriod levels earlier, so the
        /// long-range trend is still strictly toward "harder", just not a dead-straight line.
        private const int BreatherPeriod = 12;
        private const float BreatherStrength = 0.10f;

        public static float FillFractionForLevel(int levelId)
        {
            levelId = Clamp(levelId, 1, MaxLevel);
            if (levelId >= RampEndLevel) return 1f;

            float baseFraction = RawRampFraction(levelId);
            if (levelId <= 5 || levelId % BreatherPeriod != 0) return baseFraction;

            float floor = RawRampFraction(Clamp(levelId - BreatherPeriod, 1, MaxLevel));
            float breathed = baseFraction - BreatherStrength;
            return breathed > floor ? breathed : floor;
        }

        private static float RawRampFraction(int levelId)
        {
            if (levelId >= RampEndLevel) return 1f;
            float t = (levelId - 1) / (float)(RampEndLevel - 1);
            return StartFillFraction + (1f - StartFillFraction) * t;
        }

        /// How strongly LevelGenerator should cluster its removal order around recently-placed
        /// cells rather than picking uniformly across the whole frontier. 0 at the very start
        /// (fully random, easiest to read) ramping up to 0.5 by the time the board reaches
        /// MaxBoardSize, then holding there — clustering produces deeper, more localized
        /// dependency chains that take more foresight to untangle than density alone provides.
        public static float LocalityBiasForLevel(int levelId)
        {
            levelId = Clamp(levelId, 1, MaxLevel);
            float t = (levelId - 1) / (float)(RampEndLevel - 1);
            if (t < 0f) t = 0f; else if (t > 1f) t = 1f;
            return t * 0.5f;
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
    }
}
