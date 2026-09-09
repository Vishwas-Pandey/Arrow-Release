using System.Collections.Generic;
using ReleaseTheArrow.Gameplay;
using ReleaseTheArrow.LevelSystem;

namespace ReleaseTheArrow.Generation
{
    /// Validates that a level layout can be fully cleared.
    ///
    /// Key property of this puzzle type: removing an arrow can only ever remove blockers,
    /// never add one. So the set of currently-releasable arrows can only grow as arrows are
    /// removed, regardless of *which* releasable arrow is removed first. That means a single
    /// greedy pass (repeatedly remove any one currently-releasable arrow) is a complete solver:
    /// if the board can be cleared at all, this loop clears it. No backtracking is needed.
    public static class PuzzleSolver
    {
        public static bool IsSolvable(LevelLayout layout) => TrySolve(layout, out _);

        /// Attempts to fully clear the layout. On success, solutionOrder holds one valid
        /// tap order (arrow ids) a player could use to clear the board.
        public static bool TrySolve(LevelLayout layout, out List<int> solutionOrder)
        {
            solutionOrder = new List<int>(layout.ArrowCount);
            var board = BoardState.FromLayout(layout);

            while (board.RemainingCount > 0)
            {
                var releasable = board.GetAllReleasableIds();
                if (releasable.Count == 0) return false; // deadlock — unsolvable

                foreach (var id in releasable)
                {
                    board.TryRelease(id);
                    solutionOrder.Add(id);
                }
            }
            return true;
        }
    }
}
