using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WarbandSummoner.Core
{
    /// <summary>
    /// One rung of a tier ladder (DESIGN §2.3). Immutable data; deliberately
    /// unvalidated so a loader can build every entry from config and then
    /// report all problems at once via <see cref="TierTable.Validate"/>.
    /// </summary>
    public sealed class TierDefinition
    {
        private static readonly IReadOnlyList<string> NoItems = Array.Empty<string>();
        // Shared across every tier without overrides, so it must be truly
        // immutable: a bare Dictionary behind the read-only interface could be
        // downcast and mutated for all of them at once.
        private static readonly IReadOnlyDictionary<string, string> NoOverrides =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        /// <summary>Stable key used in config, persistence and logs.</summary>
        public string Id { get; }

        /// <summary>Shown in HUD messages. Defaults to <see cref="Id"/>.</summary>
        public string DisplayName { get; }

        /// <summary>Creature prefab the minion is cloned from.</summary>
        public string BasePrefab { get; }

        /// <summary>Item prefab of the trophy that buys this tier. Empty = material-only tier.</summary>
        public string TrophyPrefab { get; }

        /// <summary>Trophies consumed per purchase.</summary>
        public int TrophyCount { get; }

        /// <summary>Item prefab accepted in place of the trophy. Empty = no fallback.</summary>
        public string FallbackMaterial { get; }

        /// <summary>Units of <see cref="FallbackMaterial"/> consumed per purchase. Per-tier by design.</summary>
        public int FallbackCount { get; }

        /// <summary>Pack-mule inventory size at this tier.</summary>
        public int ContainerSlots { get; }

        /// <summary>Stamina the player spends to summon at this tier.</summary>
        public float SummonStaminaCost { get; }

        /// <summary>Item prefabs equipped on spawn. Empty means the base prefab's defaults apply.</summary>
        public IReadOnlyList<string> EquipmentLoadout { get; }

        /// <summary>
        /// Damage type name → damage modifier name (both as the game spells
        /// them, e.g. "Fire" → "Resistant"). Kept as strings so the core has
        /// no game dependency; the plugin maps them to enums at spawn.
        /// </summary>
        public IReadOnlyDictionary<string, string> DamageModifierOverrides { get; }

        public bool HasTrophy => TrophyPrefab.Length > 0;
        public bool HasFallback => FallbackMaterial.Length > 0;
        public bool IsMaterialOnly => !HasTrophy && HasFallback;

        public TierDefinition(
            string id,
            string basePrefab,
            string? displayName = null,
            string? trophyPrefab = null,
            int trophyCount = 1,
            string? fallbackMaterial = null,
            int fallbackCount = 2,
            int containerSlots = 0,
            float summonStaminaCost = 0f,
            IReadOnlyList<string>? equipmentLoadout = null,
            IReadOnlyDictionary<string, string>? damageModifierOverrides = null)
        {
            Id = id ?? string.Empty;
            BasePrefab = basePrefab ?? string.Empty;
            DisplayName = string.IsNullOrEmpty(displayName) ? Id : displayName!;
            TrophyPrefab = trophyPrefab ?? string.Empty;
            TrophyCount = trophyCount;
            FallbackMaterial = fallbackMaterial ?? string.Empty;
            FallbackCount = fallbackCount;
            ContainerSlots = containerSlots;
            SummonStaminaCost = summonStaminaCost;
            EquipmentLoadout = equipmentLoadout ?? NoItems;
            DamageModifierOverrides = damageModifierOverrides ?? NoOverrides;
        }

        public override string ToString() => $"{Id} ({BasePrefab})";
    }
}
