using System;
using System.Collections.Generic;

namespace WarbandSummoner.Core
{
    /// <summary>
    /// The upgrade action from DESIGN §2.4, as a pure function. Given one
    /// slot group and what the player carries, decides which slot changes
    /// and what it costs — or that nothing should happen. Never touches the
    /// inventory; the caller consumes <see cref="Purchase.Cost"/> and then
    /// calls <see cref="SlotGroup.Apply"/>.
    /// </summary>
    public static class UpgradeResolver
    {
        /// <summary>
        /// Walks the ladder from the top. The first tier that is affordable
        /// <em>and</em> has a slot that would benefit wins; an affordable tier
        /// that offers nothing falls through to the next one down.
        /// </summary>
        public static UpgradeResult Resolve(SlotGroup group, IInventoryView inventory, SpendPriority spendPriority)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));

            if (group.SlotCount == 0) return UpgradeResult.NoOp(NoOpReason.GroupHasNoSlots);

            bool anyAffordable = false;
            for (int tierIndex = group.Tiers.Count - 1; tierIndex >= 0; tierIndex--)
            {
                var tier = group.Tiers[tierIndex];
                var cost = AffordableCost(tier, inventory, spendPriority);
                if (cost == null) continue;
                anyAffordable = true;

                var target = FindTarget(group.Slots, tierIndex, group.MaxRank);
                if (target == null) continue;

                var (slotIndex, after) = target.Value;
                return UpgradeResult.Buy(new Purchase(group.Kind, slotIndex, group.Slots[slotIndex], after, tier, cost.Value));
            }

            return UpgradeResult.NoOp(anyAffordable ? NoOpReason.NoSlotWouldBenefit : NoOpReason.NothingAffordable);
        }

        /// <summary>
        /// The price the player would pay for one purchase of this tier, or
        /// null if they can afford neither the trophy nor the fallback.
        /// When both are affordable, <paramref name="spendPriority"/> picks.
        /// </summary>
        public static ItemCost? AffordableCost(TierDefinition tier, IInventoryView inventory, SpendPriority spendPriority)
        {
            if (tier == null) throw new ArgumentNullException(nameof(tier));
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));

            ItemCost? trophy = tier.HasTrophy && inventory.CountOf(tier.TrophyPrefab) >= tier.TrophyCount
                ? new ItemCost(tier.TrophyPrefab, tier.TrophyCount, CostKind.Trophy)
                : (ItemCost?)null;

            ItemCost? material = tier.HasFallback && inventory.CountOf(tier.FallbackMaterial) >= tier.FallbackCount
                ? new ItemCost(tier.FallbackMaterial, tier.FallbackCount, CostKind.Material)
                : (ItemCost?)null;

            return spendPriority == SpendPriority.MaterialFirst
                ? material ?? trophy
                : trophy ?? material;
        }

        /// <summary>
        /// Target-slot priority for a given tier (DESIGN §2.4), in strict order:
        /// <list type="number">
        /// <item>any unowned slot, lowest index first → (tier, rank 1);</item>
        /// <item>any owned slot below the tier, lowest tier first then lowest index → (tier, rank 1);</item>
        /// <item>any slot at exactly this tier below max rank, lowest rank first then lowest index → rank + 1.</item>
        /// </list>
        /// Returns null when the tier offers nothing to any slot. Breadth
        /// always beats depth; rank resets on every tier change.
        /// </summary>
        public static (int slotIndex, SlotState after)? FindTarget(IReadOnlyList<SlotState> slots, int tierIndex, int maxRank)
        {
            if (slots == null) throw new ArgumentNullException(nameof(slots));

            int bestIndex = -1;
            int bestTier = int.MaxValue;
            int bestRank = int.MaxValue;

            // Rules 1 and 2 share a scan: an unowned slot beats every owned one,
            // and among owned slots below the tier the lowest tier wins. Slot
            // order breaks ties because the scan is ascending.
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (!slot.IsOwned) return (i, new SlotState(tierIndex, Ranks.Min));
                if (slot.TierIndex < tierIndex && slot.TierIndex < bestTier)
                {
                    bestTier = slot.TierIndex;
                    bestIndex = i;
                }
            }
            if (bestIndex >= 0) return (bestIndex, new SlotState(tierIndex, Ranks.Min));

            // Rule 3: rank up the lowest-ranked slot already on this tier.
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.TierIndex == tierIndex && slot.Rank < maxRank && slot.Rank < bestRank)
                {
                    bestRank = slot.Rank;
                    bestIndex = i;
                }
            }
            if (bestIndex >= 0) return (bestIndex, new SlotState(tierIndex, bestRank + 1));

            return null;
        }
    }
}
