using System.Linq;
using WarbandSummoner.Core;
using WarbandSummoner.Core.Config;
using Xunit;

namespace WarbandSummoner.Tests.Config
{
    public class TierTableLoaderTests
    {
        private const string OneGoodMelee =
            "\"melee\": [ { \"id\": \"wolf\", \"basePrefab\": \"Wolf\", \"trophyPrefab\": \"TrophyWolf\" } ]";

        private const string OneGoodRanged =
            "\"ranged\": [ { \"id\": \"archer\", \"basePrefab\": \"Skeleton\", \"trophyPrefab\": \"TrophySkeleton\" } ]";

        [Fact]
        public void MissingFileYieldsDefaultsAndAsksForThemToBeWritten()
        {
            var result = TierTableLoader.Load(null);

            Assert.True(result.FileWasMissing);
            Assert.False(result.HasErrors);
            Assert.Empty(result.Warnings);
            Assert.Equal(LadderSource.Default, result.MeleeSource);
            Assert.Equal(LadderSource.Default, result.RangedSource);
            Assert.Equal(DefaultLadders.CreateMelee().Count, result.Melee.Count);
            Assert.Equal(DefaultLadders.CreateRanged().Count, result.Ranged.Count);
        }

        [Fact]
        public void WrittenDefaultsLoadBackCleanlyFromTheFile()
        {
            var json = TierFileJson.Serialize(DefaultLadders.CreateDocument());

            var result = TierTableLoader.Load(json);

            Assert.False(result.FileWasMissing);
            Assert.False(result.HasErrors);
            Assert.Empty(result.Warnings);
            Assert.Equal(LadderSource.File, result.MeleeSource);
            Assert.Equal(LadderSource.File, result.RangedSource);
            Assert.Equal("jotun", result.Melee[12].Id);
            Assert.Equal("charred_archer", result.Ranged[3].Id);
        }

        [Fact]
        public void UnparseableFileFallsBackToBothDefaultsWithOneError()
        {
            var result = TierTableLoader.Load("{ this is not json");

            Assert.False(result.FileWasMissing);
            var error = Assert.Single(result.Errors);
            Assert.Contains("could not be parsed", error);
            Assert.Equal(LadderSource.Default, result.MeleeSource);
            Assert.Equal(LadderSource.Default, result.RangedSource);
            Assert.Equal(13, result.Melee.Count);
        }

        [Fact]
        public void ABrokenMeleeLadderDoesNotThrowAwayAGoodRangedOne()
        {
            var json = "{ \"version\": 1, \"melee\": [ " +
                       "{ \"id\": \"wolf\", \"basePrefab\": \"Wolf\", \"trophyPrefab\": \"TrophyWolf\" }, " +
                       "{ \"id\": \"WOLF\", \"basePrefab\": \"Fenring\", \"trophyPrefab\": \"TrophyFenring\" } ], " +
                       OneGoodRanged + " }";

            var result = TierTableLoader.Load(json);

            Assert.Equal(LadderSource.Default, result.MeleeSource);
            Assert.Equal(LadderSource.File, result.RangedSource);
            Assert.Equal("archer", Assert.Single(result.Ranged).Id);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains("melee ladder: tier 'WOLF': duplicate id", result.Errors[0]);
            Assert.Contains("using the built-in default melee ladder", result.Errors[1]);
        }

        [Fact]
        public void EveryProblemInALadderIsReportedWithItsTierId()
        {
            var json = "{ \"version\": 1, " + OneGoodMelee + ", \"ranged\": [ " +
                       "{ \"id\": \"a\", \"basePrefab\": \"\", \"trophyPrefab\": \"T\" }, " +
                       "{ \"id\": \"b\", \"basePrefab\": \"Skeleton\" }, " +
                       "{ \"id\": \"c\", \"basePrefab\": \"Skeleton\", \"trophyPrefab\": \"T\", \"trophyCount\": 0 } ] }";

            var result = TierTableLoader.Load(json);

            Assert.Equal(LadderSource.File, result.MeleeSource);
            Assert.Equal(LadderSource.Default, result.RangedSource);
            Assert.Contains(result.Errors, e => e.Contains("ranged ladder: tier 'a': basePrefab is empty"));
            Assert.Contains(result.Errors, e => e.Contains("ranged ladder: tier 'b': needs a trophyPrefab"));
            Assert.Contains(result.Errors, e => e.Contains("ranged ladder: tier 'c': trophyCount must be at least 1"));
            Assert.Contains(result.Errors, e => e.Contains("ranged ladder has 3 problem(s)"));
        }

        [Fact]
        public void EmptyLadderIsAnErrorNotZeroTiers()
        {
            var result = TierTableLoader.Load("{ \"version\": 1, \"melee\": [], " + OneGoodRanged + " }");

            Assert.Equal(LadderSource.Default, result.MeleeSource);
            Assert.True(result.Melee.Count > 0);
            Assert.Contains(result.Errors, e => e.Contains("melee ladder: ladder has no tiers"));
        }

        [Fact]
        public void MissingLadderKeyIsAnError()
        {
            var result = TierTableLoader.Load("{ \"version\": 1, " + OneGoodMelee + " }");

            Assert.Equal(LadderSource.File, result.MeleeSource);
            Assert.Equal(LadderSource.Default, result.RangedSource);
            var error = Assert.Single(result.Errors);
            Assert.Contains("ranged ladder is missing", error);
        }

        [Fact]
        public void NullEntryIsReportedByPosition()
        {
            var result = TierTableLoader.Load("{ \"version\": 1, \"melee\": [ null ], " + OneGoodRanged + " }");

            Assert.Equal(LadderSource.Default, result.MeleeSource);
            Assert.Contains(result.Errors, e => e.Contains("melee ladder: tier #0: entry is null"));
        }

        [Theory]
        [InlineData("\"version\": 0, ")]
        [InlineData("\"version\": 99, ")]
        [InlineData("")]
        public void OtherVersionsLoadWithAWarning(string versionField)
        {
            var result = TierTableLoader.Load("{ " + versionField + OneGoodMelee + ", " + OneGoodRanged + " }");

            Assert.False(result.HasErrors);
            var warning = Assert.Single(result.Warnings);
            Assert.Contains("version", warning);
            Assert.Equal(LadderSource.File, result.MeleeSource);
            Assert.Equal(LadderSource.File, result.RangedSource);
        }

        [Fact]
        public void LoadedLaddersBuildWorkingSlotGroups()
        {
            var result = TierTableLoader.Load(TierFileJson.Serialize(DefaultLadders.CreateDocument()));

            var melee = SlotGroup.NewMelee(result.Melee, 4, 2);
            var ranged = SlotGroup.NewRanged(result.Ranged, 1, 2);

            Assert.Equal(new SlotState(0, 1), melee[0]);
            Assert.All(ranged.Slots, s => Assert.False(s.IsOwned));
            Assert.Equal("greyling", result.Melee[melee[0].TierIndex].Id);
            Assert.Equal(13, result.Melee.Select(t => t.Id).Distinct().Count());
        }
    }
}
