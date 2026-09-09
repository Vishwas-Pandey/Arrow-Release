using System;
using System.Diagnostics;
using ReleaseTheArrow.Generation;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace ReleaseTheArrow.EditorTools
{
    /// Release-gate QA tool (Section 20/50/51): generates and solver-validates every one of the
    /// 1500 levels. Run before any submission build — a single failure here means a level that
    /// must never ship.
    public static class LevelValidationTool
    {
        [MenuItem("Release The Arrow/Validate All 1500 Levels")]
        public static void ValidateAllLevels()
        {
            int failures = 0;
            int minArrows = int.MaxValue, maxArrows = int.MinValue;
            var stopwatch = Stopwatch.StartNew();

            for (int levelId = 1; levelId <= DifficultyCurve.MaxLevel; levelId++)
            {
                try
                {
                    var layout = LevelGenerator.Generate(levelId);
                    if (!PuzzleSolver.IsSolvable(layout))
                    {
                        Debug.LogError($"[LevelValidation] Level {levelId} generated but is NOT solvable.");
                        failures++;
                        continue;
                    }
                    minArrows = Math.Min(minArrows, layout.ArrowCount);
                    maxArrows = Math.Max(maxArrows, layout.ArrowCount);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LevelValidation] Level {levelId} threw during generation: {e}");
                    failures++;
                }
            }

            stopwatch.Stop();

            if (failures == 0)
            {
                Debug.Log($"[LevelValidation] PASSED — all {DifficultyCurve.MaxLevel} levels generate and solve correctly " +
                          $"in {stopwatch.ElapsedMilliseconds}ms. Arrow count range: {minArrows}-{maxArrows}.");
            }
            else
            {
                Debug.LogError($"[LevelValidation] FAILED — {failures} of {DifficultyCurve.MaxLevel} levels are broken. Do not ship.");
            }
        }
    }
}
