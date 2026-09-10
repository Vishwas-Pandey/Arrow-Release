using ReleaseTheArrow.Utils;

namespace ReleaseTheArrow.Generation
{
    /// Generation parameters for one level, before random jitter/aspect-ratio decisions.
    public struct DifficultyProfile
    {
        public int arrowCount;
        /// 0 = short independent chains / many open first moves (easy). 1 = long tangled
        /// dependency chains / few open first moves (hard). Drives generator cell/direction bias.
        public float constrainedness;
        /// Target occupied-cell fraction of the board. Lower = more open space = easier to read.
        public float density;
    }

    /// Maps a level id (1..1500) to a difficulty profile. Piecewise-linear between hand-tuned
    /// anchor points along the progression described in the design brief, plus small per-level
    /// jitter (seeded, so still deterministic) so consecutive levels don't feel identical.
    public static class DifficultyCurve
    {
        public const int MaxLevel = 1500;

        private struct Anchor
        {
            public int level;
            public float count;
            public float constrainedness;
            public float density;

            public Anchor(int level, float count, float constrainedness, float density)
            {
                this.level = level;
                this.count = count;
                this.constrainedness = constrainedness;
                this.density = density;
            }
        }

        private static readonly Anchor[] Anchors =
        {
            new Anchor(1, 10, 0.05f, 0.70f),
            new Anchor(20, 20, 0.15f, 0.68f),
            new Anchor(100, 32, 0.30f, 0.65f),
            new Anchor(300, 50, 0.45f, 0.62f),
            new Anchor(600, 75, 0.58f, 0.60f),
            new Anchor(1000, 110, 0.70f, 0.58f),
            new Anchor(1300, 150, 0.82f, 0.56f),
            new Anchor(1450, 175, 0.90f, 0.55f),
            new Anchor(1500, 195, 0.97f, 0.55f),
        };

        /// Level 1 is a 5x5 board, growing by one row/column per level (6x6, 7x7, ...) up to
        /// MaxBoardSize, per the requested progression. Growth is capped rather than unbounded —
        /// the board has no horizontal scroll, so anything wider than this stops fitting on
        /// screen at a legible cell size. Difficulty keeps climbing past the cap via density/
        /// constrainedness instead of raw size (see LevelGenerator, which also clamps arrowCount
        /// to whatever actually fits in size*size cells).
        public const int MaxBoardSize = 10;

        public static int BoardSizeForLevel(int levelId)
        {
            levelId = Clamp(levelId, 1, MaxLevel);
            return Clamp(levelId + 4, 5, MaxBoardSize);
        }

        public static DifficultyProfile GetProfile(int levelId, DeterministicRandom rng)
        {
            levelId = Clamp(levelId, 1, MaxLevel);

            int i = 0;
            while (i < Anchors.Length - 2 && levelId > Anchors[i + 1].level) i++;
            Anchor a = Anchors[i];
            Anchor b = Anchors[i + 1];

            float t = b.level == a.level ? 0f : (levelId - a.level) / (float)(b.level - a.level);
            float count = Lerp(a.count, b.count, t);
            float constrainedness = Lerp(a.constrainedness, b.constrainedness, t);
            float density = Lerp(a.density, b.density, t);

            // Deterministic ±12% jitter so two adjacent levels of similar difficulty don't look identical.
            float jitter = 0.88f + rng.NextFloat01() * 0.24f;
            int arrowCount = System.Math.Max(6, RoundToInt(count * jitter));

            return new DifficultyProfile
            {
                arrowCount = arrowCount,
                constrainedness = Clamp01(constrainedness + (rng.NextFloat01() - 0.5f) * 0.1f),
                density = Clamp01(density + (rng.NextFloat01() - 0.5f) * 0.06f)
            };
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);
        private static int RoundToInt(float v) => (int)(v + 0.5f);
    }
}
