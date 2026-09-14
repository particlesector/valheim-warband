using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace WarbandSummoner
{
    [BepInPlugin(Guid, Name, Version)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "particlesector.WarbandSummoner";
        public const string Name = "WarbandSummoner";
        public const string Version = "0.1.0";

        internal static ManualLogSource Log = null!;
        private Harmony? _harmony;

        private ConfigEntry<bool> _enabled = null!;
        internal static ConfigEntry<bool> DumpPrefabCatalogue = null!;

        private void Awake()
        {
            Log = Logger;

            _enabled = Config.Bind("General", "Enabled", true,
                "Master switch. When false the plugin loads but applies no patches.");

            DumpPrefabCatalogue = Config.Bind("Debug", "DumpPrefabCatalogue", false,
                "Write every creature and trophy prefab name to BepInEx/config/WarbandSummoner.catalogue.txt. " +
                "Trophies are written at the main menu; creatures once a world is loaded. Useful when editing the tier table.");

            if (!_enabled.Value)
            {
                Log.LogInfo($"{Name} {Version} disabled by config.");
                return;
            }

            _harmony = new Harmony(Guid);
            _harmony.PatchAll();

            Log.LogInfo($"{Name} {Version} loaded. Core={Core.CoreInfo.Version}");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
