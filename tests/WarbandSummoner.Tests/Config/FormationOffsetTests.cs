using WarbandSummoner.Core.Config;
using Xunit;

namespace WarbandSummoner.Tests.Config
{
    public class FormationOffsetTests
    {
        [Theory]
        [InlineData("1.5, -2", 1.5f, -2f)]
        [InlineData("-3,-3.5", -3f, -3.5f)]
        [InlineData("  0 ,  -5.5  ", 0f, -5.5f)]
        [InlineData("2,3", 2f, 3f)]
        public void ParsesRightThenForward(string text, float right, float forward)
        {
            Assert.True(FormationOffset.TryParse(text, out var offset));
            Assert.Equal(new FormationOffset(right, forward), offset);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("1.5")]
        [InlineData("1.5, -2, 0")]
        [InlineData("1,5; -2")]
        [InlineData("left, back")]
        [InlineData("NaN, 1")]
        [InlineData("1, Infinity")]
        public void RejectsAnythingElse(string? text)
        {
            Assert.False(FormationOffset.TryParse(text, out var offset));
            Assert.Equal(default, offset);
        }

        [Fact]
        public void ToStringRoundTripsWithInvariantCulture()
        {
            var original = new FormationOffset(-1.5f, -2f);

            Assert.Equal("-1.5, -2", original.ToString());
            Assert.True(FormationOffset.TryParse(original.ToString(), out var parsed));
            Assert.Equal(original, parsed);
        }

        [Fact]
        public void DefaultMeleeArcMatchesDesign()
        {
            Assert.Equal(new FormationOffset(-1.5f, -2f), FormationOffset.DefaultMelee(0));
            Assert.Equal(new FormationOffset(1.5f, -2f), FormationOffset.DefaultMelee(1));
            Assert.Equal(new FormationOffset(-3f, -3.5f), FormationOffset.DefaultMelee(2));
            Assert.Equal(new FormationOffset(3f, -3.5f), FormationOffset.DefaultMelee(3));
        }

        [Fact]
        public void ExtraMeleeSlotsKeepFanningOutBehind()
        {
            var fourth = FormationOffset.DefaultMelee(3);
            var fifth = FormationOffset.DefaultMelee(4);
            var sixth = FormationOffset.DefaultMelee(5);

            Assert.True(fifth.Forward < fourth.Forward);
            Assert.True(fifth.Right < 0 && sixth.Right > 0);
            Assert.Equal(fifth.Forward, sixth.Forward);
        }

        [Fact]
        public void DefaultRangedSitsFurthestBackAndCentred()
        {
            var ranged = FormationOffset.DefaultRanged(0);

            Assert.Equal(0f, ranged.Right);
            for (int i = 0; i < 4; i++) Assert.True(ranged.Forward < FormationOffset.DefaultMelee(i).Forward);

            Assert.Equal(new FormationOffset(-2f, -5.5f), FormationOffset.DefaultRanged(1));
            Assert.Equal(new FormationOffset(2f, -5.5f), FormationOffset.DefaultRanged(2));
        }
    }
}
