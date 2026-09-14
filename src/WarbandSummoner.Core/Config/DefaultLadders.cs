using System.Collections.Generic;

namespace WarbandSummoner.Core.Config
{
    /// <summary>
    /// The shipped tier ladders (DESIGN §2.3, prefab names from PREFABS.md).
    /// Written to <c>tiers.json</c> on first run and used in place of any
    /// ladder that fails to load. Order is the tier index that gets
    /// persisted, so append rather than insert when adding tiers.
    ///
    /// Costs, container sizes and stamina are starting points for the
    /// Phase 12 balance pass, not balance decisions: fallbackCount is 2
    /// everywhere as DESIGN §2.5 prescribes, stamina climbs 4 per melee tier
    /// from 10, and the ranged tiers cost what their melee counterparts do.
    /// </summary>
    public static class DefaultLadders
    {
        private const int Version = TierFileDocument.CurrentVersion;

        /// <summary>The whole default file, ready to serialise.</summary>
        public static TierFileDocument CreateDocument() => new TierFileDocument
        {
            Version = Version,
            Notes = "WarbandSummoner tier ladders. A tier's position in its list is the index saved on your character " +
                    "and your minions, so append new tiers rather than reordering. Prefab names are listed in the mod's " +
                    "docs/PREFABS.md; unknown keys are errors. Read at startup only. Delete this file to regenerate the defaults.",
            Melee = CreateMeleeEntries(),
            Ranged = CreateRangedEntries(),
        };

        public static TierTable CreateMelee() => ToTable(CreateMeleeEntries());

        public static TierTable CreateRanged() => ToTable(CreateRangedEntries());

        private static TierTable ToTable(List<TierEntry> entries)
        {
            var tiers = new List<TierDefinition>(entries.Count);
            foreach (var entry in entries) tiers.Add(entry.ToDefinition());
            return new TierTable(tiers);
        }

        private static List<TierEntry> CreateMeleeEntries() => new List<TierEntry>
        {
            Melee(0, "greyling", "Greyling", "Greyling", trophy: "", fallback: "Resin", container: 4,
                notes: "Material-only: greylings drop no trophy. Cheap on purpose to fill the empty Meadows stretch."),
            Melee(1, "greydwarf", "Greydwarf", "Greydwarf", "TrophyGreydwarf", "GreydwarfEye", container: 4),
            Melee(2, "skeleton", "Skeleton", "Skeleton", "TrophySkeleton", "BoneFragments", container: 4,
                notes: "Base prefab rolls a random bow or melee weapon; equipmentLoadout must name the melee weapon " +
                       "once the catalogue dump's item-pool section has verified it (Phase 4)."),
            Melee(3, "greydwarf_brute", "Greydwarf Brute", "Greydwarf_Elite", "TrophyGreydwarfBrute", "GreydwarfEye", container: 8),
            Melee(4, "draugr", "Draugr", "Draugr", "TrophyDraugr", "Entrails", container: 8),
            Melee(5, "draugr_elite", "Draugr Elite", "Draugr_Elite", "TrophyDraugrElite", "Entrails", container: 8),
            Melee(6, "wolf", "Wolf", "Wolf", "TrophyWolf", "WolfFang", container: 8,
                notes: "Already Tameable with Procreation; the clone strips Procreation."),
            Melee(7, "fenring", "Fenring", "Fenring", "TrophyFenring", "WolfFang", container: 12,
                notes: "Night spawn."),
            Melee(8, "fuling", "Fuling", "Goblin", "TrophyGoblin", "BlackMetalScrap", container: 12),
            Melee(9, "fuling_berserker", "Fuling Berserker", "GoblinBrute", "TrophyGoblinBrute", "BlackMetalScrap", container: 16),
            Melee(10, "seeker", "Seeker", "Seeker", "TrophySeeker", "Carapace", container: 16),
            Melee(11, "charred", "Charred Warrior", "Charred_Melee", "TrophyCharredMelee", "CharredBone", container: 16),
            Melee(12, "jotun", "Jotun Warrior", "JotunWarrior", "TrophyJotunWarrior", "Leatherstraps", container: 24,
                notes: "Model size unverified; swap basePrefab for Skeleton_DeepNorth, Bjorn or Elaking if it turns out giant-sized."),
        };

        private static List<TierEntry> CreateRangedEntries() => new List<TierEntry>
        {
            Ranged("skeleton_archer", "Skeleton Archer", "Skeleton", "TrophySkeleton", "BoneFragments", container: 4, stamina: 18,
                notes: "Same prefab as melee tier 2; equipmentLoadout must name the bow once verified (Phase 4)."),
            Ranged("draugr_archer", "Draugr Archer", "Draugr_Ranged", "TrophyDraugr", "Entrails", container: 8, stamina: 26),
            Ranged("fuling_archer", "Fuling Archer", "GoblinArcher", "TrophyGoblin", "BlackMetalScrap", container: 12, stamina: 42),
            Ranged("charred_archer", "Charred Archer", "Charred_Archer", "TrophyCharredArcher", "CharredBone", container: 16, stamina: 54),
        };

        private static TierEntry Melee(int index, string id, string displayName, string basePrefab, string trophy, string fallback,
            int container, string? notes = null) =>
            Entry(id, displayName, basePrefab, trophy, fallback, container, stamina: 10 + 4 * index, notes);

        private static TierEntry Ranged(string id, string displayName, string basePrefab, string trophy, string fallback,
            int container, float stamina, string? notes = null) =>
            Entry(id, displayName, basePrefab, trophy, fallback, container, stamina, notes);

        // Every field is set here, empty strings and empty collections
        // included, so the generated file shows the whole schema on every
        // tier. Leaving one null would omit nothing (nulls are serialised)
        // but would write "null" where a user expects "" or [].
        private static TierEntry Entry(string id, string displayName, string basePrefab, string trophy, string fallback,
            int container, float stamina, string? notes) => new TierEntry
        {
            Id = id,
            DisplayName = displayName,
            BasePrefab = basePrefab,
            TrophyPrefab = trophy,
            TrophyCount = 1,
            FallbackMaterial = fallback,
            FallbackCount = 2,
            ContainerSlots = container,
            SummonStaminaCost = stamina,
            EquipmentLoadout = new List<string>(),
            DamageModifierOverrides = new Dictionary<string, string>(),
            Notes = notes ?? string.Empty,
        };
    }
}
