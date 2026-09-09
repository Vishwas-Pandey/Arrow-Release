namespace ReleaseTheArrow.Utils
{
    /// Self-contained PCG32-style PRNG. System.Random's internal algorithm is not guaranteed
    /// identical across .NET/Mono/IL2CPP runtimes, which would break "same level id always
    /// produces the same puzzle" across editor vs. device. This class is deterministic by
    /// construction on every platform since it only uses fixed-width integer arithmetic.
    public class DeterministicRandom
    {
        private ulong _state;
        private const ulong Multiplier = 6364136223846793005UL;
        private const ulong Increment = 1442695040888963407UL;

        public DeterministicRandom(int seed)
        {
            _state = 0;
            NextUInt();
            _state += (ulong)seed;
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong old = _state;
            _state = old * Multiplier + Increment;
            uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        /// Inclusive of min, exclusive of max.
        public int NextInt(int minInclusive, int maxExclusive)
        {
            uint range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % range);
        }

        public float NextFloat01() => NextUInt() / (float)uint.MaxValue;

        /// Fisher-Yates shuffle, in place, deterministic given this RNG's state.
        public void Shuffle<T>(System.Collections.Generic.IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = NextInt(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
