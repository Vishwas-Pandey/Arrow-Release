using System;
using System.Collections.Generic;
using ReleaseTheArrow.Core;
using ReleaseTheArrow.LevelSystem;
using ReleaseTheArrow.Utils;

namespace ReleaseTheArrow.Generation
{
    /// Builds a guaranteed-solvable puzzle for a given level id.
    ///
    /// Construction works backwards from an empty board: the arrow placed first here is the
    /// LAST one a player will tap, and the arrow placed last here is the FIRST one a player
    /// must tap. Each new arrow is given a direction whose path is clear of every
    /// already-placed (= "removed later") arrow, which is exactly the condition for it to be
    /// safely tappable at its point in play. This guarantees solvability by construction —
    /// PuzzleSolver is still run afterwards as an independent safety-net check, per spec.
    public static class LevelGenerator
    {
        private const int MaxConstructionAttempts = 60;
        private const int CandidateSampleSize = 28;
        private const int MinArrowCount = 6;
        /// However crowded the difficulty curve wants a level to be, never fill every last cell —
        /// leaves the construction algorithm enough breathing room to always find a valid spot.
        private const float MaxFillFraction = 0.88f;

        public static LevelLayout Generate(int levelId)
        {
            int size = DifficultyCurve.BoardSizeForLevel(levelId);
            int width = size, height = size;
            int maxByFill = (int)(width * height * MaxFillFraction);

            for (int attempt = 0; attempt < MaxConstructionAttempts; attempt++)
            {
                int seed = unchecked((int)((uint)levelId * 2654435761u) + attempt);
                var rng = new DeterministicRandom(seed);
                var profile = DifficultyCurve.GetProfile(levelId, rng);

                // A small capped board at very high constrainedness can't always fit the curve's
                // full arrow-count target — back the target off a little more on each retry so
                // generation always converges on *something* constructible instead of ever
                // failing outright, however tight the board/constrainedness combination gets.
                float backoff = (float)Math.Pow(0.97, attempt);
                int arrowCount = Math.Max(MinArrowCount, Math.Min((int)(profile.arrowCount * backoff), maxByFill));

                if (TryConstruct(width, height, arrowCount, profile.constrainedness, rng, out var arrows))
                {
                    var layout = new LevelLayout
                    {
                        levelId = levelId,
                        width = width,
                        height = height,
                        arrows = arrows,
                        difficultyScore = RoundToInt(profile.constrainedness * 1000f),
                        seed = levelId,
                        generationAttempt = attempt
                    };

                    // Safety net: construction guarantees solvability, but never trust that
                    // blindly — an unsolvable level must never ship.
                    if (PuzzleSolver.IsSolvable(layout)) return layout;
                }
            }

            throw new InvalidOperationException(
                $"LevelGenerator failed to produce a solvable layout for level {levelId} after {MaxConstructionAttempts} attempts.");
        }

        private static bool TryConstruct(int width, int height, int arrowCount, float constrainedness,
            DeterministicRandom rng, out List<ArrowSpec> arrows)
        {
            arrows = new List<ArrowSpec>(arrowCount);
            var occupied = new bool[width, height];

            var emptyCells = new List<(int col, int row)>(width * height);
            for (int c = 0; c < width; c++)
                for (int r = 0; r < height; r++)
                    emptyCells.Add((c, r));
            rng.Shuffle(emptyCells);

            int nextId = 0;
            for (int k = 0; k < arrowCount; k++)
            {
                if (!PlaceOne(emptyCells, occupied, width, height, constrainedness, rng, out int chosenIndex, out ArrowDirection dir))
                    return false;

                var cell = emptyCells[chosenIndex];
                occupied[cell.col, cell.row] = true;
                arrows.Add(new ArrowSpec(nextId++, cell.col, cell.row, dir));
                emptyCells.RemoveAt(chosenIndex);
            }
            return true;
        }

        /// Picks one empty cell + a valid direction for it, biased by constrainedness:
        /// low constrainedness prefers cells with many open directions (independent, easy to spot),
        /// high constrainedness prefers cells with only one viable direction (tight dependency chains).
        private static bool PlaceOne(List<(int col, int row)> emptyCells, bool[,] occupied, int width, int height,
            float constrainedness, DeterministicRandom rng, out int chosenIndex, out ArrowDirection chosenDir)
        {
            int sampleSize = Math.Min(CandidateSampleSize, emptyCells.Count);
            if (TryPickFromRange(emptyCells, 0, sampleSize, occupied, width, height, constrainedness, rng, out chosenIndex, out chosenDir))
                return true;

            // Rare fallback: the random sample had nothing placeable — scan every remaining cell.
            return TryPickFromRange(emptyCells, 0, emptyCells.Count, occupied, width, height, constrainedness, rng, out chosenIndex, out chosenDir);
        }

        private static bool TryPickFromRange(List<(int col, int row)> emptyCells, int start, int end, bool[,] occupied,
            int width, int height, float constrainedness, DeterministicRandom rng, out int chosenIndex, out ArrowDirection chosenDir)
        {
            chosenIndex = -1;
            chosenDir = ArrowDirection.Up;
            int bestScore = int.MinValue;
            var bestDirs = new List<ArrowDirection>(4);
            bool preferFew = constrainedness >= 0.5f;

            for (int i = start; i < end; i++)
            {
                var (col, row) = emptyCells[i];
                var validDirs = ValidDirections(col, row, occupied, width, height);
                if (validDirs.Count == 0) continue;

                // Fewer valid directions = tighter/harder. Score so the preferred extreme wins,
                // with a tiny random nudge so ties (common at low constrainedness) don't always
                // pick the first candidate.
                int score = preferFew ? -validDirs.Count : validDirs.Count;
                score = score * 8 + rng.NextInt(0, 8);

                if (score > bestScore)
                {
                    bestScore = score;
                    chosenIndex = i;
                    bestDirs.Clear();
                    bestDirs.AddRange(validDirs);
                }
            }

            if (chosenIndex == -1) return false;
            chosenDir = bestDirs[rng.NextInt(0, bestDirs.Count)];
            return true;
        }

        private static List<ArrowDirection> ValidDirections(int col, int row, bool[,] occupied, int width, int height)
        {
            var result = new List<ArrowDirection>(4);
            foreach (var dir in ArrowDirectionExtensions.All)
            {
                dir.ToStep(out int dCol, out int dRow);
                int c = col + dCol, r = row + dRow;
                bool clear = true;
                while (c >= 0 && c < width && r >= 0 && r < height)
                {
                    if (occupied[c, r]) { clear = false; break; }
                    c += dCol; r += dRow;
                }
                if (clear) result.Add(dir);
            }
            return result;
        }

        private static int RoundToInt(float v) => (int)(v + 0.5f);
    }
}
