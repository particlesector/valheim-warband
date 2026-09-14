using System.Linq;
using WarbandSummoner.Core;
using WarbandSummoner.Core.Config;
using Xunit;

namespace WarbandSummoner.Tests.Config
{
    public class DefaultLaddersTests
    {
        [Fact]
        public void MeleeLadderMatchesDesign()
        {
            var melee = DefaultLadders.CreateMelee();

            Assert.Equal(13, melee.Count);
            Assert.Equal(new[]
            {
                "greyling", "greydwarf", "skeleton", "greydwarf_brute", "draugr", "draugr_elite", "wolf",
                "fenring", "fuling", "fuling_berserker", "seeker", "charred", "jotun",
            }, melee.Select(t => t.Id).ToArray());
        }

        [Fact]
        public void RangedLadderMatchesDesign()
        {
            var ranged = DefaultLadders.CreateRanged();

            Assert.Equal(new[] { "skeleton_archer", "draugr_archer", "fuling_archer", "charred_archer" },
                ranged.Select(t => t.Id).ToArray());
            Assert.All(ranged, t => Assert.True(t.HasTrophy));
        }

        [Fact]
        public void GreylingIsTheOnlyMaterialOnlyTier()
        {
            var melee = DefaultLadders.CreateMelee();

            var materialOnly = Assert.Single(melee, t => t.IsMaterialOnly);
            Assert.Equal("greyling", materialOnly.Id);
            Assert.Equal("Resin", materialOnly.FallbackMaterial);
        }

        [Fact]
        public void EveryTierHasAFallbackAndAPositiveSummonCost()
        {
            foreach (var tier in DefaultLadders.CreateMelee().Concat(DefaultLadders.CreateRanged()))
            {
                Assert.True(tier.HasFallback, tier.Id);
                Assert.Equal(2, tier.FallbackCount);
                Assert.True(tier.SummonStaminaCost > 0, tier.Id);
                Assert.True(tier.ContainerSlots > 0, tier.Id);
            }
        }

        [Fact]
        public void SummonCostRisesWithTier()
        {
            foreach (var ladder in new[] { DefaultLadders.CreateMelee(), DefaultLadders.CreateRanged() })
            {
                for (int i = 1; i < ladder.Count; i++)
                    Assert.True(ladder[i].SummonStaminaCost > ladder[i - 1].SummonStaminaCost, ladder[i].Id);
            }
        }

        [Fact]
        public void DocumentCarriesTheCurrentVersion()
        {
            var document = DefaultLadders.CreateDocument();

            Assert.Equal(TierFileDocument.CurrentVersion, document.Version);
            Assert.Equal(13, document.Melee!.Count);
            Assert.Equal(4, document.Ranged!.Count);
        }
    }
}
