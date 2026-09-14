using System;

namespace WarbandSummoner.Core
{
    /// <summary>
    /// What one minion slot holds: nothing, or a (tier index, rank) pair
    /// within its group's ladder. This is the entire per-slot progression
    /// state (DESIGN §2.1). <c>default</c> is <see cref="Unowned"/>.
    /// </summary>
    public readonly struct SlotState : IEquatable<SlotState>
    {
        public static readonly SlotState Unowned = default;

        private readonly int _tierIndex;
        private readonly int _rank;

        public SlotState(int tierIndex, int rank)
        {
            if (tierIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(tierIndex), tierIndex, "Tier index cannot be negative.");
            if (rank < Ranks.Min)
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Rank is 1-based; use Unowned for an empty slot.");
            _tierIndex = tierIndex;
            _rank = rank;
        }

        public bool IsOwned => _rank >= Ranks.Min;

        /// <summary>Index into the group's <see cref="TierTable"/>, or -1 when unowned.</summary>
        public int TierIndex => IsOwned ? _tierIndex : -1;

        /// <summary>1-based rank, or 0 when unowned.</summary>
        public int Rank => IsOwned ? _rank : 0;

        public bool Equals(SlotState other) => TierIndex == other.TierIndex && Rank == other.Rank;

        public override bool Equals(object? obj) => obj is SlotState other && Equals(other);

        public override int GetHashCode() => unchecked(TierIndex * 397 ^ Rank);

        public static bool operator ==(SlotState a, SlotState b) => a.Equals(b);

        public static bool operator !=(SlotState a, SlotState b) => !a.Equals(b);

        public override string ToString() => IsOwned ? $"T{TierIndex} R{Rank}" : "unowned";
    }
}
