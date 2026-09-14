using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using WarbandSummoner.Core;
using WarbandSummoner.Core.Config;

namespace WarbandSummoner.Config
{
    /// <summary>
    /// Every scalar tunable from DESIGN §6, bound to the BepInEx
    /// <c>.cfg</c>. Values are read live through the entries, so edits via
    /// ConfigurationManager apply immediately — except the slot counts,
    /// which size the formation offset list and are read at startup. The
    /// tier ladders live in tiers.json (see <see cref="TierConfigFile"/>).
    /// </summary>
    internal sealed class WarbandConfig
    {
        // General
        public ConfigEntry<bool> Enabled { get; }

        // Keybinds
        public ConfigEntry<KeyboardShortcut> SummonKey { get; }
        public ConfigEntry<KeyboardShortcut> HealKey { get; }
        public ConfigEntry<KeyboardShortcut> RecallKey { get; }
        public ConfigEntry<KeyboardShortcut> AttackTargetKey { get; }
        public ConfigEntry<KeyboardShortcut> UpgradeKey { get; }
        public ConfigEntry<KeyboardShortcut> UpgradeRangedKey { get; }

        // Recall
        public ConfigEntry<float> RecallHoldDuration { get; }
        public ConfigEntry<float> RecallTapCooldown { get; }
        public ConfigEntry<float> RecallHoldCooldown { get; }
        public ConfigEntry<float> RecallPlacementDistance { get; }

        // Heal
        public ConfigEntry<float> HealRadius { get; }
        public ConfigEntry<float> HealPercentOfMaxHealth { get; }
        public ConfigEntry<float> HealStaminaCost { get; }

        // Follow / formation
        public ConfigEntry<float> FollowDistance { get; }
        private readonly OffsetEntry[] _meleeOffsets;
        private readonly OffsetEntry[] _rangedOffsets;

        // Slots
        public ConfigEntry<int> MeleeSlotCount { get; }
        public ConfigEntry<int> RangedSlotCount { get; }
        public ConfigEntry<int> MaxRank { get; }

        // Progression
        public ConfigEntry<float> TrophyDropRateMultiplier { get; }
        public ConfigEntry<Core.SpendPriority> SpendPriority { get; }

        // Debug
        public ConfigEntry<bool> DumpPrefabCatalogue { get; }

        public WarbandConfig(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true,
                "Master switch. When false the plugin loads but applies no patches.");

            SummonKey = config.Bind("Keybinds", "Summon", new KeyboardShortcut(KeyCode.Z),
                "Summon a minion into the lowest owned slot that has no living minion. Costs the tier's stamina.");
            HealKey = config.Bind("Keybinds", "Heal", new KeyboardShortcut(KeyCode.H),
                "Heal every owned living minion within HealRadius. Costs HealStaminaCost.");
            RecallKey = config.Bind("Keybinds", "Recall", new KeyboardShortcut(KeyCode.B),
                "Tap: teleport the highest-tier living minion to just ahead of you. Hold for RecallHoldDuration: teleport all of them. " +
                "Both resolve on release. Free, separate cooldowns. Also the get-unstuck button.");
            AttackTargetKey = config.Bind("Keybinds", "AttackTarget", new KeyboardShortcut(KeyCode.N),
                "Order every living minion to attack whatever creature is under the crosshair.");
            UpgradeKey = config.Bind("Keybinds", "Upgrade", new KeyboardShortcut(KeyCode.U),
                "Spend a trophy (or its fallback material) on the MELEE slots. The target slot is chosen automatically; " +
                "the message says what was bought and spent.");
            UpgradeRangedKey = config.Bind("Keybinds", "UpgradeRanged", new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift),
                "Same as Upgrade, but for the RANGED slot and its ladder. Needed because skeleton and draugr trophies buy tiers in both ladders.");

            RecallHoldDuration = config.Bind("Recall", "HoldDuration", 1.5f,
                new ConfigDescription("Seconds the recall key must be held before release recalls every minion instead of one.",
                    new AcceptableValueRange<float>(0.2f, 5f)));
            RecallTapCooldown = config.Bind("Recall", "TapCooldown", 8f,
                new ConfigDescription("Seconds between single-minion recalls.", new AcceptableValueRange<float>(0f, 120f)));
            RecallHoldCooldown = config.Bind("Recall", "HoldCooldown", 30f,
                new ConfigDescription("Seconds between recall-all uses.", new AcceptableValueRange<float>(0f, 600f)));
            RecallPlacementDistance = config.Bind("Recall", "PlacementDistance", 3f,
                new ConfigDescription("Metres ahead of you, along your facing, that recalled minions appear. Never inside your own collider.",
                    new AcceptableValueRange<float>(1.5f, 10f)));

            HealRadius = config.Bind("Heal", "Radius", 10f,
                new ConfigDescription("Metres from you within which minions are healed.", new AcceptableValueRange<float>(1f, 50f)));
            HealPercentOfMaxHealth = config.Bind("Heal", "PercentOfMaxHealth", 25f,
                new ConfigDescription("Health restored to each minion, as a percentage of its maximum. A percentage so one setting " +
                    "is meaningful for a greyling and a jotun alike.", new AcceptableValueRange<float>(1f, 100f)));
            HealStaminaCost = config.Bind("Heal", "StaminaCost", 5f,
                new ConfigDescription("Stamina the heal costs. Keep this well below the lowest tier's summonStaminaCost in tiers.json " +
                    "(default 10): healing is meant to be the cheap repeatable action, resummoning the expensive failure case.",
                    new AcceptableValueRange<float>(0f, 100f)));

            FollowDistance = config.Bind("Follow", "Distance", 4.5f,
                new ConfigDescription("Metres from their formation point at which minions stop following. Vanilla's single-companion " +
                    "value is 3; higher keeps four bodies from crowding you in doorways.", new AcceptableValueRange<float>(1f, 15f)));

            MeleeSlotCount = config.Bind("Slots", "MeleeSlotCount", 4,
                new ConfigDescription("Number of melee minion slots. Slot 0 is always owned at tier 0. Read at startup.",
                    new AcceptableValueRange<int>(1, 8)));
            RangedSlotCount = config.Bind("Slots", "RangedSlotCount", 1,
                new ConfigDescription("Number of ranged minion slots. 0 disables the ranged group entirely. Read at startup.",
                    new AcceptableValueRange<int>(0, 4)));
            MaxRank = config.Bind("Slots", "MaxRank", 2,
                new ConfigDescription("Highest rank a slot can reach; rank N shows N stars. Values above 2 are exposed but unsupported: " +
                    "vanilla never spawns more than two stars and star rendering beyond that is unverified.",
                    new AcceptableValueRange<int>(1, 5)));

            _meleeOffsets = BindOffsets(config, "Melee", MeleeSlotCount.Value, FormationOffset.DefaultMelee);
            _rangedOffsets = BindOffsets(config, "Ranged", RangedSlotCount.Value, FormationOffset.DefaultRanged);

            TrophyDropRateMultiplier = config.Bind("Progression", "TrophyDropRateMultiplier", 1f,
                new ConfigDescription("Multiplies the drop chance of EVERY trophy in the game, not only the ones this mod's tiers use. " +
                    "1 is vanilla.", new AcceptableValueRange<float>(0f, 10f)));
            SpendPriority = config.Bind("Progression", "SpendPriority", Core.SpendPriority.TrophyFirst,
                "When a tier is affordable both by trophy and by fallback material, which to spend. TrophyFirst banks the " +
                "material (trophies are scarcer); MaterialFirst banks the trophy.");

            DumpPrefabCatalogue = config.Bind("Debug", "DumpPrefabCatalogue", false,
                "Write every creature (with its item pools) and trophy prefab name to BepInEx/config/WarbandSummoner.catalogue.txt. " +
                "Trophies are written at the main menu; creatures once a world is loaded. Useful when editing tiers.json.");
        }

        /// <summary>Formation point for a melee slot. Slots beyond the bound count get the default arc.</summary>
        public FormationOffset MeleeOffset(int slotIndex) => Offset(_meleeOffsets, slotIndex, FormationOffset.DefaultMelee);

        /// <summary>Formation point for a ranged slot. Slots beyond the bound count get the default line.</summary>
        public FormationOffset RangedOffset(int slotIndex) => Offset(_rangedOffsets, slotIndex, FormationOffset.DefaultRanged);

        private static FormationOffset Offset(OffsetEntry[] entries, int slotIndex, System.Func<int, FormationOffset> defaults)
        {
            if (slotIndex < 0) throw new System.ArgumentOutOfRangeException(nameof(slotIndex), slotIndex, "Slot index cannot be negative.");
            return slotIndex < entries.Length ? entries[slotIndex].Value : defaults(slotIndex);
        }

        private static OffsetEntry[] BindOffsets(ConfigFile config, string group, int count, System.Func<int, FormationOffset> defaults)
        {
            var entries = new List<OffsetEntry>(count);
            for (int i = 0; i < count; i++)
            {
                var fallback = defaults(i);
                var entry = config.Bind("Formation", $"{group}Slot{i}Offset", fallback.ToString(),
                    $"Where {group.ToLowerInvariant()} slot {i} stands relative to you, as \"right, forward\" in metres " +
                    "(negative right = your left, negative forward = behind you). Malformed values fall back to the default.");
                entries.Add(new OffsetEntry(entry, fallback));
            }
            return entries.ToArray();
        }

        /// <summary>
        /// A parsed formation entry. Re-parses only when the raw string
        /// changes, and complains once per bad value rather than every frame.
        /// </summary>
        private sealed class OffsetEntry
        {
            private readonly ConfigEntry<string> _entry;
            private readonly FormationOffset _fallback;
            private string? _parsedRaw;
            private FormationOffset _parsed;

            public OffsetEntry(ConfigEntry<string> entry, FormationOffset fallback)
            {
                _entry = entry;
                _fallback = fallback;
            }

            public FormationOffset Value
            {
                get
                {
                    string raw = _entry.Value;
                    if (raw == _parsedRaw) return _parsed;

                    if (!FormationOffset.TryParse(raw, out _parsed))
                    {
                        Plugin.Log.LogWarning(
                            $"[Formation] {_entry.Definition.Key} = \"{raw}\" is not \"right, forward\"; using the default {_fallback}.");
                        _parsed = _fallback;
                    }
                    _parsedRaw = raw;
                    return _parsed;
                }
            }
        }
    }
}
