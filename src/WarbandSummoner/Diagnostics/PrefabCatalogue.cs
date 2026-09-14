using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace WarbandSummoner.Diagnostics
{
    /// <summary>
    /// Config-gated dump of every creature and trophy prefab the game knows
    /// about, for filling in the tier table. Trophies come from ObjectDB
    /// (available at the main menu); creatures come from ZNetScene (only
    /// exists once a world is loaded).
    /// </summary>
    [HarmonyPatch]
    internal static class PrefabCatalogue
    {
        private static string OutputPath => Path.Combine(Paths.ConfigPath, "WarbandSummoner.catalogue.txt");

        private static bool _trophiesWritten;
        private static bool _creaturesWritten;

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
        [HarmonyPostfix]
        private static void ObjectDb_CopyOtherDB_Postfix(ObjectDB __instance) => DumpTrophies(__instance);

        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        [HarmonyPostfix]
        private static void ObjectDb_Awake_Postfix(ObjectDB __instance) => DumpTrophies(__instance);

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        [HarmonyPostfix]
        private static void ZNetScene_Awake_Postfix(ZNetScene __instance) => DumpCreatures(__instance);

        private static void DumpTrophies(ObjectDB db)
        {
            if (!Plugin.DumpPrefabCatalogue.Value || _trophiesWritten || db.m_items.Count == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine("# Trophies (prefab name | shared name token)");
            foreach (var go in db.m_items.OrderBy(g => g.name))
            {
                var drop = go.GetComponent<ItemDrop>();
                if (drop == null || drop.m_itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Trophy) continue;
                sb.AppendLine($"{go.name} | {drop.m_itemData.m_shared.m_name}");
            }
            sb.AppendLine();

            File.WriteAllText(OutputPath, sb.ToString());
            _trophiesWritten = true;
            Plugin.Log.LogInfo($"Trophy catalogue written to {OutputPath}");
        }

        private static void DumpCreatures(ZNetScene scene)
        {
            if (!Plugin.DumpPrefabCatalogue.Value || _creaturesWritten) return;

            var sb = new StringBuilder();
            sb.AppendLine("# Creatures (prefab | faction | components | drops)");
            foreach (var go in scene.m_prefabs.OrderBy(g => g.name))
            {
                var character = go.GetComponent<Character>();
                if (character == null) continue;

                var comps = new List<string>();
                if (go.GetComponent<Humanoid>() != null) comps.Add("Humanoid");
                if (go.GetComponent<MonsterAI>() != null) comps.Add("MonsterAI");
                if (go.GetComponent<AnimalAI>() != null) comps.Add("AnimalAI");
                if (go.GetComponent<Tameable>() != null) comps.Add("Tameable");
                if (go.GetComponent<TimedDestruction>() != null) comps.Add("TimedDestruction");
                if (go.GetComponent<Procreation>() != null) comps.Add("Procreation");

                var drops = "";
                var cd = go.GetComponent<CharacterDrop>();
                if (cd != null)
                {
                    drops = string.Join(", ", cd.m_drops
                        .Where(d => d.m_prefab != null)
                        .Select(d => $"{d.m_prefab.name}@{d.m_chance:0.###}"));
                }

                sb.AppendLine($"{go.name} | {character.m_faction} | {string.Join("+", comps)} | {drops}");
            }

            File.AppendAllText(OutputPath, sb.ToString());
            _creaturesWritten = true;
            Plugin.Log.LogInfo($"Creature catalogue appended to {OutputPath}");
        }
    }
}
