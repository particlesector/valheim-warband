using System;
using System.Collections;
using System.Collections.Generic;

namespace WarbandSummoner.Core
{
    /// <summary>
    /// An ordered, validated tier ladder. Position in the list is the tier
    /// index that gets persisted on slots and minions, so the order is part
    /// of the contract: reordering a ladder changes what every saved index
    /// means. Two ladders exist (melee, ranged); indices are not comparable
    /// across them.
    /// </summary>
    public sealed class TierTable : IReadOnlyList<TierDefinition>
    {
        private readonly TierDefinition[] _tiers;
        private readonly Dictionary<string, int> _indexById;

        /// <summary>
        /// Builds a table from <paramref name="tiers"/>, throwing
        /// <see cref="TierTableException"/> listing every problem if any fail
        /// <see cref="Validate"/>.
        /// </summary>
        public TierTable(IEnumerable<TierDefinition> tiers)
        {
            if (tiers == null) throw new ArgumentNullException(nameof(tiers));
            _tiers = new List<TierDefinition>(tiers).ToArray();

            var problems = Validate(_tiers);
            if (problems.Count > 0) throw new TierTableException(problems);

            _indexById = new Dictionary<string, int>(_tiers.Length, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _tiers.Length; i++) _indexById[_tiers[i].Id] = i;
        }

        /// <summary>
        /// Every reason <paramref name="tiers"/> is not a usable ladder, each
        /// prefixed with the offending tier's id (or its position when the id
        /// itself is the problem). Empty when the ladder is valid.
        /// </summary>
        public static IReadOnlyList<string> Validate(IReadOnlyList<TierDefinition> tiers)
        {
            if (tiers == null) throw new ArgumentNullException(nameof(tiers));

            var problems = new List<string>();
            if (tiers.Count == 0)
            {
                problems.Add("ladder has no tiers");
                return problems;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < tiers.Count; i++)
            {
                var tier = tiers[i];
                if (tier == null)
                {
                    problems.Add($"tier #{i}: entry is null");
                    continue;
                }

                string label = tier.Id.Length > 0 ? $"tier '{tier.Id}'" : $"tier #{i}";

                if (tier.Id.Length == 0)
                    problems.Add($"{label}: id is empty");
                else if (!seen.Add(tier.Id))
                    problems.Add($"{label}: duplicate id");

                if (tier.BasePrefab.Length == 0)
                    problems.Add($"{label}: basePrefab is empty");

                if (!tier.HasTrophy && !tier.HasFallback)
                    problems.Add($"{label}: needs a trophyPrefab, a fallbackMaterial, or both");

                if (tier.HasTrophy && tier.TrophyCount < 1)
                    problems.Add($"{label}: trophyCount must be at least 1 (was {tier.TrophyCount})");

                if (tier.HasFallback && tier.FallbackCount < 1)
                    problems.Add($"{label}: fallbackCount must be at least 1 (was {tier.FallbackCount})");

                if (tier.ContainerSlots < 0)
                    problems.Add($"{label}: containerSlots cannot be negative (was {tier.ContainerSlots})");

                if (tier.SummonStaminaCost < 0f)
                    problems.Add($"{label}: summonStaminaCost cannot be negative (was {tier.SummonStaminaCost})");

                for (int j = 0; j < tier.EquipmentLoadout.Count; j++)
                {
                    if (string.IsNullOrEmpty(tier.EquipmentLoadout[j]))
                        problems.Add($"{label}: equipmentLoadout[{j}] is empty");
                }

                foreach (var kv in tier.DamageModifierOverrides)
                {
                    if (string.IsNullOrEmpty(kv.Key) || string.IsNullOrEmpty(kv.Value))
                        problems.Add($"{label}: damageModifierOverrides has an empty key or value");
                }
            }

            return problems;
        }

        public int Count => _tiers.Length;

        public TierDefinition this[int index] => _tiers[index];

        public bool IsValidIndex(int index) => index >= 0 && index < _tiers.Length;

        /// <summary>Index of the tier with this id (case-insensitive), or -1.</summary>
        public int IndexOf(string id) =>
            id != null && _indexById.TryGetValue(id, out int i) ? i : -1;

        /// <summary>The tier with this id (case-insensitive), or null.</summary>
        public TierDefinition? Find(string id)
        {
            int i = IndexOf(id);
            return i >= 0 ? _tiers[i] : null;
        }

        public IEnumerator<TierDefinition> GetEnumerator() => ((IEnumerable<TierDefinition>)_tiers).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Thrown when a ladder fails validation. <see cref="Problems"/> lists every issue.</summary>
    public sealed class TierTableException : Exception
    {
        public IReadOnlyList<string> Problems { get; }

        public TierTableException(IReadOnlyList<string> problems)
            : base("Tier table is invalid: " + string.Join("; ", problems))
        {
            Problems = problems;
        }
    }
}
