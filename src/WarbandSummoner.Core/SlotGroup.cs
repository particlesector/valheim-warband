using System;
using System.Collections.Generic;

namespace WarbandSummoner.Core
{
    /// <summary>Which ladder a slot group buys from. Persisted as a tag on slot state.</summary>
    public enum SlotGroupKind
    {
        Melee,
        Ranged,
    }

    /// <summary>
    /// A ladder plus the slots that buy from it (DESIGN §2.1). The melee
    /// group and the ranged group are two instances of this; they never
    /// share a purchase because <see cref="UpgradeResolver"/> only ever
    /// sees one of them.
    /// </summary>
    public sealed class SlotGroup
    {
        private readonly SlotState[] _slots;

        public SlotGroupKind Kind { get; }
        public TierTable Tiers { get; }

        /// <summary>Highest rank a slot may reach. DESIGN §2.2 ships with 2.</summary>
        public int MaxRank { get; }

        public IReadOnlyList<SlotState> Slots => _slots;
        public int SlotCount => _slots.Length;

        /// <summary>A group with every slot unowned. Zero slots disables the group.</summary>
        public SlotGroup(SlotGroupKind kind, TierTable tiers, int slotCount, int maxRank)
        {
            if (tiers == null) throw new ArgumentNullException(nameof(tiers));
            if (slotCount < 0)
                throw new ArgumentOutOfRangeException(nameof(slotCount), slotCount, "Slot count cannot be negative.");
            if (maxRank < Ranks.Min)
                throw new ArgumentOutOfRangeException(nameof(maxRank), maxRank, "Max rank must be at least 1.");

            Kind = kind;
            Tiers = tiers;
            MaxRank = maxRank;
            _slots = new SlotState[slotCount];
        }

        /// <summary>
        /// A fresh character's melee group: slot 0 is owned at tier 0, rank 1,
        /// for free, so a new character can summon immediately.
        /// </summary>
        public static SlotGroup NewMelee(TierTable tiers, int slotCount, int maxRank)
        {
            var group = new SlotGroup(SlotGroupKind.Melee, tiers, slotCount, maxRank);
            if (slotCount > 0) group.SetSlot(0, new SlotState(0, Ranks.Min));
            return group;
        }

        /// <summary>A fresh character's ranged group: nothing is free; the first purchase unlocks it.</summary>
        public static SlotGroup NewRanged(TierTable tiers, int slotCount, int maxRank) =>
            new SlotGroup(SlotGroupKind.Ranged, tiers, slotCount, maxRank);

        public SlotState this[int slotIndex] => _slots[slotIndex];

        /// <summary>
        /// Overwrites a slot. Rejects tier indices outside the ladder and
        /// ranks above <see cref="MaxRank"/>, so persisted state that no
        /// longer fits the config fails loudly at load rather than at summon.
        /// </summary>
        public void SetSlot(int slotIndex, SlotState state)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length)
                throw new ArgumentOutOfRangeException(nameof(slotIndex), slotIndex, $"Group has {_slots.Length} slots.");
            if (state.IsOwned)
            {
                if (!Tiers.IsValidIndex(state.TierIndex))
                    throw new ArgumentOutOfRangeException(nameof(state), state.TierIndex, $"Ladder has {Tiers.Count} tiers.");
                if (state.Rank > MaxRank)
                    throw new ArgumentOutOfRangeException(nameof(state), state.Rank, $"Max rank is {MaxRank}.");
            }
            _slots[slotIndex] = state;
        }

        /// <summary>Applies a purchase produced by <see cref="UpgradeResolver"/> against this group.</summary>
        public void Apply(Purchase purchase)
        {
            if (purchase == null) throw new ArgumentNullException(nameof(purchase));
            if (purchase.Group != Kind)
                throw new InvalidOperationException($"Purchase targets the {purchase.Group} group, not {Kind}.");
            if (_slots[purchase.SlotIndex] != purchase.Before)
                throw new InvalidOperationException(
                    $"Slot {purchase.SlotIndex} is {_slots[purchase.SlotIndex]}, but the purchase expected {purchase.Before}.");
            SetSlot(purchase.SlotIndex, purchase.After);
        }

        /// <summary>True when every slot sits at the top tier at max rank — no purchase can ever help.</summary>
        public bool IsFullyMaxed
        {
            get
            {
                int top = Tiers.Count - 1;
                foreach (var slot in _slots)
                {
                    if (!slot.IsOwned || slot.TierIndex < top || slot.Rank < MaxRank) return false;
                }
                return true;
            }
        }
    }
}
