using System;
using System.Collections.Generic;
using ReleaseTheArrow.Core;

namespace ReleaseTheArrow.LevelSystem
{
    /// The full, validated description of one level's puzzle board.
    /// Regenerated deterministically from (levelId, seed, params) — never stored as a giant per-level blob.
    [Serializable]
    public class LevelLayout
    {
        public int levelId;
        public int width;
        public int height;
        public List<ArrowSpec> arrows = new List<ArrowSpec>();
        public int difficultyScore;
        public int seed;
        public int generationAttempt;

        public int ArrowCount => arrows.Count;
    }
}
