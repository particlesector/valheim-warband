using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using WarbandSummoner.Core.Config;

namespace WarbandSummoner.Config
{
    /// <summary>
    /// The disk side of the tier ladders: reads
    /// <c>BepInEx/config/WarbandSummoner.tiers.json</c> once at startup,
    /// writes the defaults when it is absent, and logs every problem the
    /// loader found. Read once only — tier indices are persisted on slots
    /// and minions, so reloading a reordered ladder mid-session would
    /// silently change what saved data means.
    /// </summary>
    internal static class TierConfigFile
    {
        public const string FileName = "WarbandSummoner.tiers.json";

        public static string FilePath => Path.Combine(Paths.ConfigPath, FileName);

        public static TierLoadResult Load(ManualLogSource log)
        {
            string path = FilePath;
            string? json = null;
            bool readFailed = false;

            if (File.Exists(path))
            {
                try
                {
                    json = File.ReadAllText(path);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // Treat as missing for loading purposes, but never overwrite a file we could not read.
                    log.LogError($"[Tiers] Could not read {path}: {ex.Message}. Using the built-in default ladders.");
                    readFailed = true;
                }
            }

            var result = TierTableLoader.Load(json);

            if (result.FileWasMissing && !readFailed) WriteDefaults(log, path);

            foreach (var error in result.Errors) log.LogError($"[Tiers] {FileName}: {error}");
            foreach (var warning in result.Warnings) log.LogWarning($"[Tiers] {FileName}: {warning}");

            log.LogInfo(
                $"[Tiers] melee ladder: {result.Melee.Count} tiers ({Describe(result.MeleeSource)}); " +
                $"ranged ladder: {result.Ranged.Count} tiers ({Describe(result.RangedSource)}).");

            return result;
        }

        private static void WriteDefaults(ManualLogSource log, string path)
        {
            try
            {
                File.WriteAllText(path, TierFileJson.Serialize(DefaultLadders.CreateDocument()));
                log.LogInfo($"[Tiers] No {FileName} found; wrote the default ladders to {path}.");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                log.LogError($"[Tiers] Could not write the default {FileName} to {path}: {ex.Message}. Continuing with the built-in defaults.");
            }
        }

        private static string Describe(LadderSource source) =>
            source == LadderSource.File ? "from file" : "built-in default";
    }
}
