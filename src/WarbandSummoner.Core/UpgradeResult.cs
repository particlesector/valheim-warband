using System;

namespace WarbandSummoner.Core
{
    /// <summary>Which price to take when a tier is affordable both ways (DESIGN §2.5).</summary>
    public enum SpendPriority
    {
        /// <summary>Spend the trophy; it is the scarcer resource. Default.</summary>
        TrophyFirst,

        /// <summary>Spend the fallback material and bank the trophy.</summary>
        MaterialFirst,
    }

    public enum CostKind
    {
        Trophy,
        Material,
    }

    /// <summary>What a purchase removes from the inventory.</summary>
    public readonly struct ItemCost : IEquatable<ItemCost>
    {
        public string ItemPrefab { get; }
        public int Count { get; }
        public CostKind Kind { get; }

        public ItemCost(string itemPrefab, int count, CostKind kind)
        {
            ItemPrefab = itemPrefab ?? throw new ArgumentNullException(nameof(itemPrefab));
            Count = count;
            Kind = kind;
        }

        public bool Equals(ItemCost other) =>
            ItemPrefab == other.ItemPrefab && Count == other.Count && Kind == other.Kind;

        public override bool Equals(object? obj) => obj is ItemCost other && Equals(other);

        public override int GetHashCode() => unchecked((ItemPrefab.GetHashCode() * 397 ^ Count) * 397 ^ (int)Kind);

        public override string ToString() => $"{Count}x {ItemPrefab} ({Kind})";
    }

    /// <summary>Why the resolver did nothing. Nothing was consumed in any of these cases.</summary>
    public enum NoOpReason
    {
        /// <summary>The group has zero slots (config disabled it).</summary>
        GroupHasNoSlots,

        /// <summary>The player cannot afford any tier in the ladder, by trophy or by fallback.</summary>
        NothingAffordable,

        /// <summary>At least one tier is affordable, but no slot would improve from any of them.</summary>
        NoSlotWouldBenefit,
    }

    /// <summary>
    /// A resolved upgrade: which slot changes, from what to what, and what
    /// it costs. Everything the HUD message needs (DESIGN §4.5) is here.
    /// </summary>
    public sealed class Purchase
    {
        public SlotGroupKind Group { get; }
        public int SlotIndex { get; }
        public SlotState Before { get; }
        public SlotState After { get; }
        public TierDefinition Tier { get; }
        public ItemCost Cost { get; }

        /// <summary>True when the slot was unowned and is now owned.</summary>
        public bool IsUnlock => !Before.IsOwned;

        /// <summary>True when the slot stays on the same tier and only its rank rises.</summary>
        public bool IsRankUp => Before.IsOwned && Before.TierIndex == After.TierIndex;

        public Purchase(SlotGroupKind group, int slotIndex, SlotState before, SlotState after, TierDefinition tier, ItemCost cost)
        {
            if (!after.IsOwned) throw new ArgumentException("A purchase always results in an owned slot.", nameof(after));
            Group = group;
            SlotIndex = slotIndex;
            Before = before;
            After = after;
            Tier = tier ?? throw new ArgumentNullException(nameof(tier));
            Cost = cost;
        }

        public override string ToString() =>
            $"{Group} slot {SlotIndex}: {Before} -> {After} ({Tier.DisplayName}) for {Cost}";
    }

    /// <summary>Outcome of <see cref="UpgradeResolver.Resolve"/>: a purchase, or a reason there is none.</summary>
    public sealed class UpgradeResult
    {
        public Purchase? Purchase { get; }
        public NoOpReason Reason { get; }

        public bool IsPurchase => Purchase != null;

        private UpgradeResult(Purchase? purchase, NoOpReason reason)
        {
            Purchase = purchase;
            Reason = reason;
        }

        public static UpgradeResult Buy(Purchase purchase) =>
            new UpgradeResult(purchase ?? throw new ArgumentNullException(nameof(purchase)), default);

        public static UpgradeResult NoOp(NoOpReason reason) => new UpgradeResult(null, reason);

        public override string ToString() => IsPurchase ? Purchase!.ToString() : $"no-op: {Reason}";
    }
}
