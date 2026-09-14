using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using WarbandSummoner.Config;
using WarbandSummoner.Core.Config;

namespace WarbandSummoner
{
    [BepInPlugin(Guid, Name, Version)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Guid = "particlesector.WarbandSummoner";
        public const string Name = "WarbandSummoner";
        public const string Version = "0.1.0";

        internal static ManualLogSource Log = null!;

        /// <summary>Every scalar setting, bound to the .cfg. Set before any patch runs.</summary>
        internal static WarbandConfig Settings = null!;

        /// <summary>The tier ladders, loaded once from tiers.json. Null while the plugin is disabled.</summary>
        internal static TierLoadResult? Tiers;

        private Harmony? _harmony;

        private void Awake()
        {
            Log = Logger;
            Settings = new WarbandConfig(Config);

            if (!Settings.Enabled.Value)
            {
                Log.LogInfo($"{Name} {Version} disabled by config.");
                return;
            }

            Tiers = TierConfigFile.Load(Log);

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
