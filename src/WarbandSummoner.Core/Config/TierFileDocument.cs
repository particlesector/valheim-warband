using System.Collections.Generic;
using Newtonsoft.Json;

namespace WarbandSummoner.Core.Config
{
    /// <summary>
    /// The on-disk shape of <c>WarbandSummoner.tiers.json</c>: a version
    /// stamp and the two ladders. Plain mutable DTOs so the JSON layer can
    /// fill them in; <see cref="TierTableLoader"/> turns them into validated
    /// <see cref="TierTable"/>s.
    /// </summary>
    public sealed class TierFileDocument
    {
        /// <summary>Format version written by <see cref="DefaultLadders"/>. Bump when a field changes meaning.</summary>
        public const int CurrentVersion = 1;

        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>Free text for humans, ignored by the loader. The default file uses it to explain itself.</summary>
        [JsonProperty("notes")]
        public string? Notes { get; set; }

        [JsonProperty("melee")]
        public List<TierEntry>? Melee { get; set; }

        [JsonProperty("ranged")]
        public List<TierEntry>? Ranged { get; set; }
    }

    /// <summary>
    /// One tier as written in the file. Field names match the DESIGN §2.3
    /// table so the document and the design read the same. Everything but
    /// <c>id</c> and <c>basePrefab</c> is optional.
    /// </summary>
    public sealed class TierEntry
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("displayName")]
        public string? DisplayName { get; set; }

        [JsonProperty("basePrefab")]
        public string? BasePrefab { get; set; }

        [JsonProperty("trophyPrefab")]
        public string? TrophyPrefab { get; set; }

        [JsonProperty("trophyCount")]
        public int TrophyCount { get; set; } = 1;

        [JsonProperty("fallbackMaterial")]
        public string? FallbackMaterial { get; set; }

        [JsonProperty("fallbackCount")]
        public int FallbackCount { get; set; } = 2;

        [JsonProperty("containerSlots")]
        public int ContainerSlots { get; set; }

        [JsonProperty("summonStaminaCost")]
        public float SummonStaminaCost { get; set; }

        [JsonProperty("equipmentLoadout")]
        public List<string>? EquipmentLoadout { get; set; }

        [JsonProperty("damageModifierOverrides")]
        public Dictionary<string, string>? DamageModifierOverrides { get; set; }

        /// <summary>
        /// Free text for humans — why this fallback count, what to verify.
        /// Ignored by the loader; the sanctioned place for comments in a
        /// format that has none.
        /// </summary>
        [JsonProperty("notes")]
        public string? Notes { get; set; }

        /// <summary>Builds the immutable definition; validation happens later in <see cref="TierTable.Validate"/>.</summary>
        public TierDefinition ToDefinition() => new TierDefinition(
            Id ?? string.Empty,
            BasePrefab ?? string.Empty,
            DisplayName,
            TrophyPrefab,
            TrophyCount,
            FallbackMaterial,
            FallbackCount,
            ContainerSlots,
            SummonStaminaCost,
            EquipmentLoadout,
            DamageModifierOverrides);

        /// <summary>
        /// The inverse of <see cref="ToDefinition"/>, used to write defaults.
        /// Every field is written, empty strings included, so the generated
        /// file shows the whole schema on every tier.
        /// </summary>
        public static TierEntry FromDefinition(TierDefinition tier, string? notes = null)
        {
            var entry = new TierEntry
            {
                Id = tier.Id,
                DisplayName = tier.DisplayName,
                BasePrefab = tier.BasePrefab,
                TrophyPrefab = tier.TrophyPrefab,
                TrophyCount = tier.TrophyCount,
                FallbackMaterial = tier.FallbackMaterial,
                FallbackCount = tier.FallbackCount,
                ContainerSlots = tier.ContainerSlots,
                SummonStaminaCost = tier.SummonStaminaCost,
                EquipmentLoadout = new List<string>(tier.EquipmentLoadout),
                DamageModifierOverrides = new Dictionary<string, string>(),
                Notes = notes,
            };
            foreach (var kv in tier.DamageModifierOverrides) entry.DamageModifierOverrides[kv.Key] = kv.Value;
            return entry;
        }
    }
}
