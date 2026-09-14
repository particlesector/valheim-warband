using System.Collections.Generic;
using HarmonyLib;
using WarbandSummoner.Core;
using WarbandSummoner.Core.Config;

namespace WarbandSummoner.Config
{
    /// <summary>
    /// The half of tiers.json validation that needs the game: do the prefab
    /// names actually exist? Items are checked once ObjectDB is populated
    /// (main menu), creatures once ZNetScene exists (world load). A
    /// misspelled name is by far the likeliest edit mistake, and nothing in
    /// the pure loader can catch it. Warnings only — the ladder still loads,
    /// and the summon path (Phase 4) refuses the tier at use time.
    /// </summary>
    [HarmonyPatch]
    internal static class TierPrefabCheck
    {
        private static bool _itemsChecked;
        private static bool _creaturesChecked;

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
        [HarmonyPostfix]
        private static void ObjectDb_CopyOtherDB_Postfix(ObjectDB __instance) => CheckItems(__instance);

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        [HarmonyPostfix]
        private static void ObjectDb_Awake_Postfix(ObjectDB __instance) => CheckItems(__instance);

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        [HarmonyPostfix]
        private static void ZNetScene_Awake_Postfix(ZNetScene __instance) => CheckCreatures(__instance);

        private static void CheckItems(ObjectDB db)
        {
            if (_itemsChecked || db.m_items.Count == 0) return;
            _itemsChecked = true;

            int problems = 0;
            foreach (var (ladder, tier) in AllTiers())
            {
                if (tier.HasTrophy && db.GetItemPrefab(tier.TrophyPrefab) == null)
                    problems += Report(ladder, tier, $"trophyPrefab '{tier.TrophyPrefab}' is not an item prefab");
                if (tier.HasFallback && db.GetItemPrefab(tier.FallbackMaterial) == null)
                    problems += Report(ladder, tier, $"fallbackMaterial '{tier.FallbackMaterial}' is not an item prefab");
            }
            Summarise("item", problems);
        }

        private static void CheckCreatures(ZNetScene scene)
        {
            if (_creaturesChecked) return;
            _creaturesChecked = true;

            int problems = 0;
            foreach (var (ladder, tier) in AllTiers())
            {
                var prefab = scene.GetPrefab(tier.BasePrefab);
                if (prefab == null)
                {
                    problems += Report(ladder, tier, $"basePrefab '{tier.BasePrefab}' is not a registered prefab");
                    continue;
                }
                if (prefab.GetComponent<Humanoid>() == null || prefab.GetComponent<MonsterAI>() == null)
                    problems += Report(ladder, tier, $"basePrefab '{tier.BasePrefab}' is not a Humanoid with MonsterAI; it cannot be a minion");
            }
            Summarise("creature", problems);
        }

        private static IEnumerable<(string ladder, TierDefinition tier)> AllTiers()
        {
            var tiers = Plugin.Tiers;
            if (tiers == null) yield break;
            foreach (var tier in tiers.Melee) yield return (TierTableLoader.MeleeLadderName, tier);
            foreach (var tier in tiers.Ranged) yield return (TierTableLoader.RangedLadderName, tier);
        }

        private static int Report(string ladder, TierDefinition tier, string problem)
        {
            Plugin.Log.LogWarning($"[Tiers] {ladder} ladder, tier '{tier.Id}': {problem}. Check {TierConfigFile.FileName} against PREFABS.md.");
            return 1;
        }

        private static void Summarise(string what, int problems)
        {
            if (problems == 0) Plugin.Log.LogInfo($"[Tiers] All {what} prefab names in the ladders resolved.");
            else Plugin.Log.LogWarning($"[Tiers] {problems} {what} prefab name(s) in the ladders did not resolve.");
        }
    }
}
