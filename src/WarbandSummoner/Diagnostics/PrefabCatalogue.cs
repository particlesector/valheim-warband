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

        // Each section is captured once, independently, and the whole file is
        // rewritten from whatever sections exist. The two patch points fire in
        // different scenes (ObjectDB at the main menu, ZNetScene on world load),
        // so nothing here may depend on which one runs first.
        private static string? _trophySection;
        private static string? _creatureSection;

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
            if (!Plugin.DumpPrefabCatalogue.Value || _trophySection != null || db.m_items.Count == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine("# Trophies (prefab name | shared name token)");
            foreach (var go in db.m_items.OrderBy(g => g.name))
            {
                var drop = go.GetComponent<ItemDrop>();
                if (drop == null || drop.m_itemData.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Trophy) continue;
                sb.AppendLine($"{go.name} | {drop.m_itemData.m_shared.m_name}");
            }

            _trophySection = sb.ToString();
            WriteFile("trophy");
        }

        private static void DumpCreatures(ZNetScene scene)
        {
            if (!Plugin.DumpPrefabCatalogue.Value || _creatureSection != null) return;

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

            _creatureSection = sb.ToString();
            WriteFile("creature");
        }

        private static void WriteFile(string justCaptured)
        {
            var sb = new StringBuilder();
            if (_trophySection != null) sb.Append(_trophySection).AppendLine();
            if (_creatureSection != null) sb.Append(_creatureSection);
            File.WriteAllText(OutputPath, sb.ToString());
            Plugin.Log.LogInfo($"Prefab catalogue ({justCaptured} section captured) written to {OutputPath}");
        }
    }
}
