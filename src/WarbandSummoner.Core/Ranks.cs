using System;

namespace WarbandSummoner.Core
{
    /// <summary>
    /// Rank rules (DESIGN §2.2). Rank is 1-based and never zero for an owned
    /// slot; the game's star level is one higher because a summoned creature
    /// is never plain. This is the only place that off-by-one is allowed.
    /// </summary>
    public static class Ranks
    {
        public const int Min = 1;

        /// <summary>
        /// The value to pass to <c>Character.SetLevel</c>: level 1 is star-less,
        /// so rank 1 → level 2 (one star), rank 2 → level 3 (two stars).
        /// </summary>
        public static int ToCharacterLevel(int rank)
        {
            if (rank < Min)
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Rank is 1-based.");
            return rank + 1;
        }
    }
}
