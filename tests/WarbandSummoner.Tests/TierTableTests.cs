using System.Collections.Generic;
using WarbandSummoner.Core;
using Xunit;

namespace WarbandSummoner.Tests
{
    public class TierTableTests
    {
        [Fact]
        public void IndicesFollowListOrder()
        {
            var table = TestLadders.Melee();

            Assert.Equal(4, table.Count);
            Assert.Equal("greyling", table[0].Id);
            Assert.Equal("greydwarf_brute", table[3].Id);
            Assert.Equal(TestLadders.Skeleton, table.IndexOf("skeleton"));
            Assert.Equal(TestLadders.Skeleton, table.IndexOf("SKELETON"));
            Assert.Equal(-1, table.IndexOf("troll"));
            Assert.True(table.TryGet("greydwarf", out var greydwarf));
            Assert.Equal("Greydwarf", greydwarf.BasePrefab);
        }

        [Fact]
        public void MaterialOnlyTierIsValid()
        {
            var tier = new TierDefinition("greyling", "Greyling", fallbackMaterial: "Resin");

            Assert.True(tier.IsMaterialOnly);
            Assert.False(tier.HasTrophy);
            Assert.Empty(TierTable.Validate(new[] { tier }));
        }

        [Fact]
        public void TrophyOnlyTierIsValid()
        {
            var tier = new TierDefinition("wolf", "Wolf", trophyPrefab: "TrophyWolf");

            Assert.False(tier.HasFallback);
            Assert.Empty(TierTable.Validate(new[] { tier }));
        }

        [Fact]
        public void TierWithNoPriceIsAProblem()
        {
            var problems = TierTable.Validate(new[] { new TierDefinition("free", "Greyling") });

            var problem = Assert.Single(problems);
            Assert.Contains("'free'", problem);
            Assert.Contains("trophyPrefab", problem);
        }

        [Fact]
        public void EmptyLadderIsAProblem()
        {
            var problems = TierTable.Validate(new TierDefinition[0]);

            Assert.Single(problems);
            Assert.Throws<TierTableException>(() => new TierTable(new TierDefinition[0]));
        }

        [Fact]
        public void DuplicateIdsAreCaughtCaseInsensitively()
        {
            var problems = TierTable.Validate(new[]
            {
                new TierDefinition("wolf", "Wolf", trophyPrefab: "TrophyWolf"),
                new TierDefinition("Wolf", "Fenring", trophyPrefab: "TrophyFenring"),
            });

            var problem = Assert.Single(problems);
            Assert.Contains("duplicate id", problem);
        }

        [Fact]
        public void EveryProblemIsReportedWithTheTierId()
        {
            var problems = TierTable.Validate(new[]
            {
                new TierDefinition("", "", trophyPrefab: "TrophyWolf", trophyCount: 0),
                new TierDefinition("bad", "Wolf", fallbackMaterial: "WolfFang", fallbackCount: 0,
                    containerSlots: -1, summonStaminaCost: -5, equipmentLoadout: new[] { "" },
                    damageModifierOverrides: new Dictionary<string, string> { { "Fire", "" } }),
            });

            Assert.Contains(problems, p => p.StartsWith("tier #0: id is empty"));
            Assert.Contains(problems, p => p.StartsWith("tier #0: basePrefab is empty"));
            Assert.Contains(problems, p => p.StartsWith("tier #0: trophyCount"));
            Assert.Contains(problems, p => p.StartsWith("tier 'bad': fallbackCount"));
            Assert.Contains(problems, p => p.StartsWith("tier 'bad': containerSlots"));
            Assert.Contains(problems, p => p.StartsWith("tier 'bad': summonStaminaCost"));
            Assert.Contains(problems, p => p.StartsWith("tier 'bad': equipmentLoadout[0]"));
            Assert.Contains(problems, p => p.StartsWith("tier 'bad': damageModifierOverrides"));
            Assert.Equal(8, problems.Count);
        }

        [Fact]
        public void ConstructorThrowsWithAllProblems()
        {
            var ex = Assert.Throws<TierTableException>(() => new TierTable(new[]
            {
                new TierDefinition("a", ""),
                new TierDefinition("a", "Wolf", trophyPrefab: "TrophyWolf"),
            }));

            Assert.Equal(3, ex.Problems.Count);
            Assert.Contains("duplicate id", ex.Message);
        }

        [Fact]
        public void CountsThatDoNotApplyAreIgnored()
        {
            // A zero trophyCount on a material-only tier is meaningless, not an error.
            var problems = TierTable.Validate(new[]
            {
                new TierDefinition("greyling", "Greyling", fallbackMaterial: "Resin", trophyCount: 0),
            });

            Assert.Empty(problems);
        }

        [Fact]
        public void DisplayNameDefaultsToId()
        {
            Assert.Equal("wolf", new TierDefinition("wolf", "Wolf", trophyPrefab: "TrophyWolf").DisplayName);
            Assert.Equal("Wolf", new TierDefinition("wolf", "Wolf", "Wolf", trophyPrefab: "TrophyWolf").DisplayName);
        }
    }
}
