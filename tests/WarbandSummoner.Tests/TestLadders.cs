using System.Collections.Generic;
using WarbandSummoner.Core;

namespace WarbandSummoner.Tests
{
    /// <summary>
    /// Cut-down copies of the DESIGN §2.3 ladders — enough tiers to exercise
    /// skipping, material-only, shared trophies and fall-through without
    /// dragging the whole table into every test.
    /// </summary>
    internal static class TestLadders
    {
        public const int Greyling = 0;
        public const int Greydwarf = 1;
        public const int Skeleton = 2;
        public const int Brute = 3;

        public const int SkeletonArcher = 0;
        public const int DraugrArcher = 1;

        public static TierTable Melee() => new TierTable(new[]
        {
            new TierDefinition("greyling", "Greyling", "Greyling",
                fallbackMaterial: "Resin", fallbackCount: 2, summonStaminaCost: 10),
            new TierDefinition("greydwarf", "Greydwarf", "Greydwarf",
                trophyPrefab: "TrophyGreydwarf", fallbackMaterial: "GreydwarfEye", fallbackCount: 2, summonStaminaCost: 15),
            new TierDefinition("skeleton", "Skeleton", "Skeleton",
                trophyPrefab: "TrophySkeleton", fallbackMaterial: "BoneFragments", fallbackCount: 2, summonStaminaCost: 15),
            new TierDefinition("greydwarf_brute", "Greydwarf_Elite", "Greydwarf Brute",
                trophyPrefab: "TrophyGreydwarfBrute", fallbackMaterial: "GreydwarfEye", fallbackCount: 4, summonStaminaCost: 20),
        });

        public static TierTable Ranged() => new TierTable(new[]
        {
            new TierDefinition("skeleton_archer", "Skeleton", "Skeleton Archer",
                trophyPrefab: "TrophySkeleton", fallbackMaterial: "BoneFragments", fallbackCount: 2,
                equipmentLoadout: new[] { "skeleton_bow" }),
            new TierDefinition("draugr_archer", "Draugr_Ranged", "Draugr Archer",
                trophyPrefab: "TrophyDraugr", fallbackMaterial: "Entrails", fallbackCount: 2),
        });

        public static SlotGroup NewMelee(int slotCount = 4, int maxRank = 2) =>
            SlotGroup.NewMelee(Melee(), slotCount, maxRank);

        public static SlotGroup NewRanged(int slotCount = 1, int maxRank = 2) =>
            SlotGroup.NewRanged(Ranged(), slotCount, maxRank);

        /// <summary>Builds a melee group with the given slot states, in order.</summary>
        public static SlotGroup MeleeWith(params SlotState[] slots)
        {
            var group = new SlotGroup(SlotGroupKind.Melee, Melee(), slots.Length, 2);
            for (int i = 0; i < slots.Length; i++) group.SetSlot(i, slots[i]);
            return group;
        }

        public static SlotState Owned(int tier, int rank) => new SlotState(tier, rank);
    }

    internal sealed class DictionaryInventory : IInventoryView
    {
        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();

        public DictionaryInventory() { }

        public DictionaryInventory(string item, int count) => Add(item, count);

        public DictionaryInventory Add(string item, int count)
        {
            _counts.TryGetValue(item, out int have);
            _counts[item] = have + count;
            return this;
        }

        public int CountOf(string itemPrefab) => _counts.TryGetValue(itemPrefab, out int n) ? n : 0;

        public int TotalItems
        {
            get
            {
                int total = 0;
                foreach (var n in _counts.Values) total += n;
                return total;
            }
        }
    }
}
