using System;
using WarbandSummoner.Core;
using Xunit;
using static WarbandSummoner.Tests.TestLadders;

namespace WarbandSummoner.Tests
{
    public class RanksTests
    {
        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        [InlineData(3, 4)]
        public void RankMapsToOneLevelHigher(int rank, int expectedLevel)
        {
            // DESIGN §2.2: level 1 is star-less, so rank 1 must show one star.
            Assert.Equal(expectedLevel, Ranks.ToCharacterLevel(rank));
        }

        [Fact]
        public void RankZeroHasNoLevel()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Ranks.ToCharacterLevel(0));
        }
    }

    public class SlotStateTests
    {
        [Fact]
        public void DefaultIsUnowned()
        {
            SlotState state = default;

            Assert.False(state.IsOwned);
            Assert.Equal(SlotState.Unowned, state);
            Assert.Equal(-1, state.TierIndex);
            Assert.Equal(0, state.Rank);
        }

        [Fact]
        public void OwnedStateRoundTrips()
        {
            var state = new SlotState(3, 2);

            Assert.True(state.IsOwned);
            Assert.Equal(3, state.TierIndex);
            Assert.Equal(2, state.Rank);
            Assert.Equal(new SlotState(3, 2), state);
            Assert.NotEqual(new SlotState(3, 1), state);
            Assert.NotEqual(SlotState.Unowned, state);
        }

        [Fact]
        public void RankZeroCannotBeOwned()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SlotState(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SlotState(-1, 1));
        }
    }

    public class SlotGroupTests
    {
        [Fact]
        public void FreshMeleeGroupOwnsSlotZeroForFree()
        {
            var group = NewMelee();

            Assert.Equal(SlotGroupKind.Melee, group.Kind);
            Assert.Equal(4, group.SlotCount);
            Assert.Equal(Owned(Greyling, 1), group[0]);
            Assert.Equal(SlotState.Unowned, group[1]);
            Assert.Equal(SlotState.Unowned, group[2]);
            Assert.Equal(SlotState.Unowned, group[3]);
        }

        [Fact]
        public void FreshRangedGroupOwnsNothing()
        {
            var group = NewRanged();

            Assert.Equal(SlotGroupKind.Ranged, group.Kind);
            Assert.Equal(1, group.SlotCount);
            Assert.Equal(SlotState.Unowned, group[0]);
        }

        [Fact]
        public void ZeroSlotsDisablesTheGroup()
        {
            Assert.Equal(0, NewRanged(slotCount: 0).SlotCount);
            Assert.Equal(0, NewMelee(slotCount: 0).SlotCount);
        }

        [Fact]
        public void SetSlotRejectsStateOutsideTheLadderOrAboveMaxRank()
        {
            var group = NewMelee(maxRank: 2);

            Assert.Throws<ArgumentOutOfRangeException>(() => group.SetSlot(0, Owned(99, 1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => group.SetSlot(0, Owned(Skeleton, 3)));
            Assert.Throws<ArgumentOutOfRangeException>(() => group.SetSlot(4, Owned(Skeleton, 1)));

            group.SetSlot(3, Owned(Skeleton, 2));
            Assert.Equal(Owned(Skeleton, 2), group[3]);
        }

        [Fact]
        public void ApplyRejectsPurchaseForTheWrongGroupOrStaleState()
        {
            var melee = NewMelee();
            var ranged = NewRanged();
            var purchase = UpgradeResolver.Resolve(ranged, new DictionaryInventory("TrophySkeleton", 1), SpendPriority.TrophyFirst).Purchase!;

            Assert.Throws<InvalidOperationException>(() => melee.Apply(purchase));

            ranged.Apply(purchase);
            Assert.Throws<InvalidOperationException>(() => ranged.Apply(purchase));
        }

        [Fact]
        public void IsFullyMaxedRequiresTopTierAtMaxRankEverywhere()
        {
            Assert.False(NewMelee().IsFullyMaxed);
            Assert.False(MeleeWith(Owned(Brute, 2), Owned(Brute, 1)).IsFullyMaxed);
            Assert.False(MeleeWith(Owned(Brute, 2), Owned(Skeleton, 2)).IsFullyMaxed);
            Assert.True(MeleeWith(Owned(Brute, 2), Owned(Brute, 2)).IsFullyMaxed);
            Assert.True(NewRanged(slotCount: 0).IsFullyMaxed);
        }
    }
}
