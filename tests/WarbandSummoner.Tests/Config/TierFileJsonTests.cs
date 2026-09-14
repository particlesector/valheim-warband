using System.Collections.Generic;
using WarbandSummoner.Core;
using WarbandSummoner.Core.Config;
using Xunit;

namespace WarbandSummoner.Tests.Config
{
    public class TierFileJsonTests
    {
        [Fact]
        public void DefaultsRoundTrip()
        {
            var written = DefaultLadders.CreateDocument();

            var json = TierFileJson.Serialize(written);
            var read = TierFileJson.Deserialize(json);

            Assert.Equal(written.Version, read.Version);
            Assert.Equal(written.Notes, read.Notes);
            AssertLaddersEqual(written.Melee!, read.Melee!);
            AssertLaddersEqual(written.Ranged!, read.Ranged!);
        }

        [Fact]
        public void EveryFieldIsWrittenForEveryTier()
        {
            var json = TierFileJson.Serialize(DefaultLadders.CreateDocument());

            // The greyling has no trophy; its empty trophyPrefab must still appear so the schema is visible.
            Assert.Contains("\"trophyPrefab\": \"\"", json);
            foreach (var field in new[]
            {
                "id", "displayName", "basePrefab", "trophyPrefab", "trophyCount", "fallbackMaterial", "fallbackCount",
                "containerSlots", "summonStaminaCost", "equipmentLoadout", "damageModifierOverrides",
            })
            {
                Assert.Equal(17, CountOccurrences(json, "\"" + field + "\":"));
            }
            // 17 tiers plus the file-level explanation.
            Assert.Equal(18, CountOccurrences(json, "\"notes\":"));
        }

        [Fact]
        public void OptionalFieldsDefaultWhenOmitted()
        {
            var read = TierFileJson.Deserialize(
                "{ \"version\": 1, \"melee\": [ { \"id\": \"wolf\", \"basePrefab\": \"Wolf\", \"trophyPrefab\": \"TrophyWolf\" } ], \"ranged\": [] }");

            var tier = Assert.Single(read.Melee!).ToDefinition();
            Assert.Equal("wolf", tier.Id);
            Assert.Equal("wolf", tier.DisplayName);
            Assert.Equal(1, tier.TrophyCount);
            Assert.False(tier.HasFallback);
            Assert.Empty(tier.EquipmentLoadout);
            Assert.Empty(tier.DamageModifierOverrides);
            Assert.Empty(TierTable.Validate(new[] { tier }));
        }

        [Fact]
        public void LoadoutAndOverridesAreRead()
        {
            var read = TierFileJson.Deserialize(@"{
                ""version"": 1,
                ""melee"": [ {
                    ""id"": ""skeleton"", ""basePrefab"": ""Skeleton"", ""trophyPrefab"": ""TrophySkeleton"",
                    ""equipmentLoadout"": [ ""skeleton_mace"" ],
                    ""damageModifierOverrides"": { ""Fire"": ""Weak"", ""Pierce"": ""Resistant"" }
                } ],
                ""ranged"": []
            }");

            var tier = Assert.Single(read.Melee!).ToDefinition();
            Assert.Equal(new[] { "skeleton_mace" }, tier.EquipmentLoadout);
            Assert.Equal("Weak", tier.DamageModifierOverrides["Fire"]);
            Assert.Equal("Resistant", tier.DamageModifierOverrides["Pierce"]);
        }

        [Fact]
        public void UnknownKeyIsAnErrorNamingTheKey()
        {
            var ex = Assert.Throws<TierFileFormatException>(() => TierFileJson.Deserialize(
                "{ \"version\": 1, \"melee\": [ { \"id\": \"wolf\", \"basePrefab\": \"Wolf\", \"trophyPrefabb\": \"TrophyWolf\" } ] }"));

            Assert.Contains("trophyPrefabb", ex.Message);
        }

        [Fact]
        public void MalformedJsonIsAnErrorWithAPosition()
        {
            var ex = Assert.Throws<TierFileFormatException>(() => TierFileJson.Deserialize("{ \"version\": 1, \"melee\": [ { \"id\": "));

            Assert.Contains("line", ex.Message);
        }

        [Fact]
        public void WrongValueTypeIsAnError()
        {
            var ex = Assert.Throws<TierFileFormatException>(() => TierFileJson.Deserialize(
                "{ \"version\": 1, \"melee\": [ { \"id\": \"wolf\", \"basePrefab\": \"Wolf\", \"trophyCount\": \"two\" } ] }"));

            Assert.Contains("trophyCount", ex.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   \n")]
        [InlineData("null")]
        public void EmptyInputIsAnError(string json)
        {
            Assert.Throws<TierFileFormatException>(() => TierFileJson.Deserialize(json));
        }

        private static void AssertLaddersEqual(List<TierEntry> expected, List<TierEntry> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                var e = expected[i].ToDefinition();
                var a = actual[i].ToDefinition();
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.DisplayName, a.DisplayName);
                Assert.Equal(e.BasePrefab, a.BasePrefab);
                Assert.Equal(e.TrophyPrefab, a.TrophyPrefab);
                Assert.Equal(e.TrophyCount, a.TrophyCount);
                Assert.Equal(e.FallbackMaterial, a.FallbackMaterial);
                Assert.Equal(e.FallbackCount, a.FallbackCount);
                Assert.Equal(e.ContainerSlots, a.ContainerSlots);
                Assert.Equal(e.SummonStaminaCost, a.SummonStaminaCost);
                Assert.Equal(e.EquipmentLoadout, a.EquipmentLoadout);
                Assert.Equal(e.DamageModifierOverrides, a.DamageModifierOverrides);
                Assert.Equal(expected[i].Notes, actual[i].Notes);
            }
        }

        private static int CountOccurrences(string text, string needle)
        {
            int count = 0;
            for (int i = text.IndexOf(needle); i >= 0; i = text.IndexOf(needle, i + needle.Length)) count++;
            return count;
        }
    }
}
