using WarbandSummoner.Core;
using Xunit;
using static WarbandSummoner.Tests.TestLadders;

namespace WarbandSummoner.Tests
{
    public class UpgradeResolverTests
    {
        private static UpgradeResult Resolve(SlotGroup group, IInventoryView inventory, SpendPriority priority = SpendPriority.TrophyFirst) =>
            UpgradeResolver.Resolve(group, inventory, priority);

        private static Purchase Buy(SlotGroup group, IInventoryView inventory, SpendPriority priority = SpendPriority.TrophyFirst)
        {
            var result = Resolve(group, inventory, priority);
            Assert.True(result.IsPurchase, result.ToString());
            return result.Purchase!;
        }

        private static void AssertNoOp(SlotGroup group, IInventoryView inventory, NoOpReason reason, SpendPriority priority = SpendPriority.TrophyFirst)
        {
            var result = Resolve(group, inventory, priority);
            Assert.False(result.IsPurchase, result.ToString());
            Assert.Equal(reason, result.Reason);
        }

        // --- DESIGN §2.4 example sentences ---------------------------------

        [Fact]
        public void SkeletonTrophyWithThreeGreylingsFillsTheEmptySlot()
        {
            // "A player with three greyling slots who finds a skeleton trophy ends up
            //  with three greylings and one skeleton, in a slot that never held a greyling."
            var group = MeleeWith(Owned(Greyling, 1), Owned(Greyling, 1), Owned(Greyling, 1), SlotState.Unowned);

            var purchase = Buy(group, new DictionaryInventory("TrophySkeleton", 1));

            Assert.Equal(3, purchase.SlotIndex);
            Assert.Equal(Owned(Skeleton, 1), purchase.After);
            Assert.True(purchase.IsUnlock);
        }

        [Fact]
        public void NextSkeletonTrophyExpandsRatherThanRanksUp()
        {
            // "A player holding three greylings and one 1-star skeleton will always see
            //  their next skeleton trophy expand rather than rank up."
            var group = MeleeWith(Owned(Greyling, 1), Owned(Greyling, 1), Owned(Greyling, 1), Owned(Skeleton, 1));

            var purchase = Buy(group, new DictionaryInventory("TrophySkeleton", 1));

            Assert.Equal(0, purchase.SlotIndex);
            Assert.Equal(Owned(Skeleton, 1), purchase.After);
            Assert.False(purchase.IsRankUp);
        }

        [Fact]
        public void RankResetsOnTierChange()
        {
            // "A tier-2 rank-2 slot upgraded to tier 3 becomes tier 3 rank 1."
            var group = MeleeWith(Owned(Skeleton, 2));

            var purchase = Buy(group, new DictionaryInventory("TrophyGreydwarfBrute", 1));

            Assert.Equal(Owned(Brute, 1), purchase.After);
        }

        [Fact]
        public void MaxingATierCostsOnePurchasePerSlotPerRank()
        {
            // "4 slots × 2 ranks = 8 trophies. Slot 0 is free at tier 0 rank 1, so tier 0 costs 7."
            Assert.Equal(7, PurchasesUntilNoOp(NewMelee(), new DictionaryInventory("Resin", 1000)));
            Assert.Equal(8, PurchasesUntilNoOp(NewMelee(), new DictionaryInventory("TrophySkeleton", 1000)));
            // "The ranged slot costs 2 per tier."
            Assert.Equal(2, PurchasesUntilNoOp(NewRanged(), new DictionaryInventory("TrophyDraugr", 1000)));
        }

        // --- Tier selection --------------------------------------------------

        [Fact]
        public void HighestAffordableTierWins()
        {
            var group = NewMelee();
            var inventory = new DictionaryInventory("TrophyGreydwarf", 1).Add("TrophySkeleton", 1).Add("Resin", 10);

            var purchase = Buy(group, inventory);

            Assert.Equal(Skeleton, purchase.After.TierIndex);
            Assert.Equal("TrophySkeleton", purchase.Cost.ItemPrefab);
        }

        [Fact]
        public void TiersCanBeSkipped()
        {
            var group = NewMelee();

            var purchase = Buy(group, new DictionaryInventory("TrophyGreydwarfBrute", 1));

            Assert.Equal(1, purchase.SlotIndex);
            Assert.Equal(Owned(Brute, 1), purchase.After);
        }

        [Fact]
        public void WhenTheHighestAffordableTierOffersNothingLowerTiersCannotHelpEither()
        {
            // DESIGN §2.4 says to fall through to T-1 when T offers nothing. Given
            // rule 2 (any slot below T benefits from T), that is provably a no-op:
            // T offering nothing means every slot already sits at or above T, so
            // nothing lower can improve any of them. The resolver still walks down
            // (cheap, and safe if the rules change) but the result is a no-op.
            var group = MeleeWith(Owned(Brute, 2), Owned(Brute, 2));
            var inventory = new DictionaryInventory("TrophyGreydwarfBrute", 5).Add("TrophySkeleton", 5).Add("Resin", 50);

            AssertNoOp(group, inventory, NoOpReason.NoSlotWouldBenefit);
        }

        [Fact]
        public void UnaffordableTiersAreSkippedOnTheWayDown()
        {
            // Brute and skeleton are unaffordable; greydwarf is, and ranks slot 1 up.
            var group = MeleeWith(Owned(Brute, 2), Owned(Greydwarf, 1));
            var inventory = new DictionaryInventory("TrophyGreydwarf", 1).Add("BoneFragments", 1).Add("GreydwarfEye", 3);

            var purchase = Buy(group, inventory);

            Assert.Equal(1, purchase.SlotIndex);
            Assert.Equal(Owned(Greydwarf, 2), purchase.After);
            Assert.True(purchase.IsRankUp);
        }

        [Fact]
        public void LowerTierSlotIsReplacedEvenWhenItsOwnTrophyIsAlsoAffordable()
        {
            // Slot 1 is a rank-1 greydwarf; the bag holds a brute trophy and a
            // greydwarf trophy. Highest affordable tier wins: the greydwarf is
            // replaced by a brute rather than ranked up.
            var group = MeleeWith(Owned(Brute, 2), Owned(Greydwarf, 1));
            var inventory = new DictionaryInventory("TrophyGreydwarfBrute", 1).Add("TrophyGreydwarf", 1);

            var purchase = Buy(group, inventory);

            Assert.Equal(1, purchase.SlotIndex);
            Assert.Equal(Owned(Brute, 1), purchase.After);
        }

        // --- Target-slot priority --------------------------------------------

        [Fact]
        public void UnownedSlotBeatsEveryOwnedSlot()
        {
            var group = MeleeWith(Owned(Greyling, 1), SlotState.Unowned, Owned(Greyling, 1));

            var purchase = Buy(group, new DictionaryInventory("TrophySkeleton", 1));

            Assert.Equal(1, purchase.SlotIndex);
        }

        [Fact]
        public void LowestTierThenLowestIndexIsReplaced()
        {
            var group = MeleeWith(Owned(Greydwarf, 2), Owned(Greyling, 2), Owned(Greyling, 1), Owned(Skeleton, 1));

            var purchase = Buy(group, new DictionaryInventory("TrophyGreydwarfBrute", 1));

            // Both greylings are the lowest tier; slot 1 comes first regardless of rank.
            Assert.Equal(1, purchase.SlotIndex);
            Assert.Equal(Owned(Brute, 1), purchase.After);
        }

        [Fact]
        public void RankUpGoesToTheLowestRankThenLowestIndex()
        {
            var group = MeleeWith(Owned(Skeleton, 2), Owned(Skeleton, 1), Owned(Skeleton, 1), Owned(Skeleton, 2));

            var purchase = Buy(group, new DictionaryInventory("TrophySkeleton", 1));

            Assert.Equal(1, purchase.SlotIndex);
            Assert.Equal(Owned(Skeleton, 2), purchase.After);
            Assert.True(purchase.IsRankUp);
        }

        [Fact]
        public void RankUpNeverExceedsMaxRank()
        {
            var capped = MeleeWith(Owned(Skeleton, 2), Owned(Skeleton, 2));
            AssertNoOp(capped, new DictionaryInventory("TrophySkeleton", 10), NoOpReason.NoSlotWouldBenefit);

            var raised = new SlotGroup(SlotGroupKind.Melee, Melee(), 1, maxRank: 3);
            raised.SetSlot(0, Owned(Skeleton, 2));
            Assert.Equal(Owned(Skeleton, 3), Buy(raised, new DictionaryInventory("TrophySkeleton", 1)).After);
        }

        [Fact]
        public void FullyMaxedGroupIsANoOp()
        {
            var group = MeleeWith(Owned(Brute, 2), Owned(Brute, 2), Owned(Brute, 2), Owned(Brute, 2));
            var everything = new DictionaryInventory("Resin", 99).Add("TrophyGreydwarf", 99)
                .Add("TrophySkeleton", 99).Add("TrophyGreydwarfBrute", 99).Add("GreydwarfEye", 99);

            Assert.True(group.IsFullyMaxed);
            AssertNoOp(group, everything, NoOpReason.NoSlotWouldBenefit);
        }

        // --- Costs and spend priority ----------------------------------------

        [Fact]
        public void NothingAffordableIsANoOpThatConsumesNothing()
        {
            var group = NewMelee();
            var inventory = new DictionaryInventory("Resin", 1).Add("TrophyWolf", 5);

            AssertNoOp(group, inventory, NoOpReason.NothingAffordable);
            Assert.Equal(6, inventory.TotalItems);
            Assert.Equal(Owned(Greyling, 1), group[0]);
        }

        [Fact]
        public void EmptyGroupIsANoOp()
        {
            AssertNoOp(NewRanged(slotCount: 0), new DictionaryInventory("TrophySkeleton", 5), NoOpReason.GroupHasNoSlots);
        }

        [Fact]
        public void MaterialOnlyTierIsBoughtWithMaterial()
        {
            var group = NewMelee();

            var purchase = Buy(group, new DictionaryInventory("Resin", 2));

            Assert.Equal(1, purchase.SlotIndex);
            Assert.Equal(Owned(Greyling, 1), purchase.After);
            Assert.Equal(new ItemCost("Resin", 2, CostKind.Material), purchase.Cost);
        }

        [Fact]
        public void FallbackIsUsedOnlyWhenTheTrophyIsUnaffordable()
        {
            var group = NewMelee();
            var both = new DictionaryInventory("TrophySkeleton", 1).Add("BoneFragments", 10);
            var materialOnly = new DictionaryInventory("BoneFragments", 10);

            Assert.Equal(new ItemCost("TrophySkeleton", 1, CostKind.Trophy), Buy(group, both).Cost);
            Assert.Equal(new ItemCost("BoneFragments", 2, CostKind.Material), Buy(group, materialOnly).Cost);
        }

        [Fact]
        public void InvertedSpendPriorityPrefersMaterial()
        {
            var group = NewMelee();
            var both = new DictionaryInventory("TrophySkeleton", 1).Add("BoneFragments", 10);
            var trophyOnly = new DictionaryInventory("TrophySkeleton", 1);

            Assert.Equal(new ItemCost("BoneFragments", 2, CostKind.Material), Buy(group, both, SpendPriority.MaterialFirst).Cost);
            Assert.Equal(new ItemCost("TrophySkeleton", 1, CostKind.Trophy), Buy(group, trophyOnly, SpendPriority.MaterialFirst).Cost);
        }

        [Fact]
        public void PerTierCountsAreHonoured()
        {
            // Brute fallback costs 4 eyes in the test ladder; greydwarf costs 2.
            var group = NewMelee();

            Assert.Equal(Greydwarf, Buy(group, new DictionaryInventory("GreydwarfEye", 3)).After.TierIndex);
            Assert.Equal(Brute, Buy(group, new DictionaryInventory("GreydwarfEye", 4)).After.TierIndex);

            var pricey = new TierTable(new[] { new TierDefinition("boss", "Boss", trophyPrefab: "TrophyBoss", trophyCount: 3) });
            var bossGroup = SlotGroup.NewRanged(pricey, 1, 2);
            AssertNoOp(bossGroup, new DictionaryInventory("TrophyBoss", 2), NoOpReason.NothingAffordable);
            Assert.Equal(3, Buy(bossGroup, new DictionaryInventory("TrophyBoss", 3)).Cost.Count);
        }

        [Fact]
        public void SpendPriorityNeverChangesWhichTierIsBought()
        {
            // Brute is only affordable by trophy; skeleton by material. Brute still wins.
            var group = NewMelee();
            var inventory = new DictionaryInventory("TrophyGreydwarfBrute", 1).Add("BoneFragments", 10);

            Assert.Equal(Brute, Buy(group, inventory, SpendPriority.MaterialFirst).After.TierIndex);
        }

        // --- Groups ------------------------------------------------------------

        [Fact]
        public void RangedSlotIsNotFreeAndUnlocksOnFirstPurchase()
        {
            var group = NewRanged();
            Assert.False(group[0].IsOwned);

            var purchase = Buy(group, new DictionaryInventory("TrophySkeleton", 1));
            group.Apply(purchase);

            Assert.True(purchase.IsUnlock);
            Assert.Equal(SlotGroupKind.Ranged, purchase.Group);
            Assert.Equal(Owned(SkeletonArcher, 1), group[0]);
        }

        [Fact]
        public void GroupsNeverShareAPurchase()
        {
            // A skeleton trophy is valid in both ladders. Resolving against one
            // group must never propose or touch a slot in the other.
            var melee = NewMelee();
            var ranged = NewRanged();
            var inventory = new DictionaryInventory("TrophySkeleton", 1);

            var rangedBuy = Buy(ranged, inventory);
            ranged.Apply(rangedBuy);
            Assert.Equal(SlotGroupKind.Ranged, rangedBuy.Group);
            Assert.Equal(Owned(Greyling, 1), melee[0]);
            Assert.Equal(SlotState.Unowned, melee[1]);

            var meleeBuy = Buy(melee, inventory);
            melee.Apply(meleeBuy);
            Assert.Equal(SlotGroupKind.Melee, meleeBuy.Group);
            Assert.Equal(Owned(SkeletonArcher, 1), ranged[0]);
        }

        [Fact]
        public void PurchaseCarriesEverythingTheMessageNeeds()
        {
            var purchase = Buy(MeleeWith(Owned(Greyling, 1), SlotState.Unowned), new DictionaryInventory("TrophySkeleton", 1));

            Assert.Equal(SlotGroupKind.Melee, purchase.Group);
            Assert.Equal(1, purchase.SlotIndex);
            Assert.Equal(SlotState.Unowned, purchase.Before);
            Assert.Equal(Owned(Skeleton, 1), purchase.After);
            Assert.Equal("Skeleton", purchase.Tier.DisplayName);
            Assert.Equal(new ItemCost("TrophySkeleton", 1, CostKind.Trophy), purchase.Cost);
        }

        private static int PurchasesUntilNoOp(SlotGroup group, IInventoryView inventory)
        {
            int purchases = 0;
            while (true)
            {
                var result = Resolve(group, inventory);
                if (!result.IsPurchase) return purchases;
                group.Apply(result.Purchase!);
                Assert.True(++purchases < 100, "resolver never reached a no-op");
            }
        }
    }
}
