using System;
using System.Collections.Generic;

namespace WarbandSummoner.Core.Config
{
    /// <summary>Where a loaded ladder came from.</summary>
    public enum LadderSource
    {
        /// <summary>Parsed and validated from the file.</summary>
        File,

        /// <summary>The built-in default, because the file was missing or that ladder was unusable.</summary>
        Default,
    }

    /// <summary>
    /// Outcome of <see cref="TierTableLoader.Load"/>. Both ladders are
    /// always present — the loader never yields zero tiers — and the lists
    /// say what went wrong and what was substituted.
    /// </summary>
    public sealed class TierLoadResult
    {
        public TierTable Melee { get; }
        public TierTable Ranged { get; }
        public LadderSource MeleeSource { get; }
        public LadderSource RangedSource { get; }

        /// <summary>True when there was no file text at all; the caller should write the defaults out.</summary>
        public bool FileWasMissing { get; }

        /// <summary>Problems that caused a ladder to be replaced by its default. Each names the ladder and, where it has one, the tier id.</summary>
        public IReadOnlyList<string> Errors { get; }

        /// <summary>Things worth telling the player that did not stop the file loading.</summary>
        public IReadOnlyList<string> Warnings { get; }

        public bool HasErrors => Errors.Count > 0;

        internal TierLoadResult(
            TierTable melee, LadderSource meleeSource,
            TierTable ranged, LadderSource rangedSource,
            bool fileWasMissing, IReadOnlyList<string> errors, IReadOnlyList<string> warnings)
        {
            Melee = melee;
            MeleeSource = meleeSource;
            Ranged = ranged;
            RangedSource = rangedSource;
            FileWasMissing = fileWasMissing;
            Errors = errors;
            Warnings = warnings;
        }
    }

    /// <summary>
    /// Turns the text of <c>tiers.json</c> into two validated ladders,
    /// substituting the built-in default for anything unusable (PLAN
    /// Phase 2). Ladders fall back independently: a broken melee list does
    /// not throw away a good ranged one. File I/O is the plugin's job so
    /// this stays testable with strings.
    /// </summary>
    public static class TierTableLoader
    {
        public const string MeleeLadderName = "melee";
        public const string RangedLadderName = "ranged";

        /// <param name="json">The file's text, or null when the file does not exist.</param>
        public static TierLoadResult Load(string? json)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            if (json == null)
            {
                return new TierLoadResult(
                    DefaultLadders.CreateMelee(), LadderSource.Default,
                    DefaultLadders.CreateRanged(), LadderSource.Default,
                    fileWasMissing: true, errors, warnings);
            }

            TierFileDocument document;
            try
            {
                document = TierFileJson.Deserialize(json);
            }
            catch (TierFileFormatException ex)
            {
                errors.Add($"file could not be parsed, using the built-in defaults for both ladders: {ex.Message}");
                return new TierLoadResult(
                    DefaultLadders.CreateMelee(), LadderSource.Default,
                    DefaultLadders.CreateRanged(), LadderSource.Default,
                    fileWasMissing: false, errors, warnings);
            }

            if (document.Version != TierFileDocument.CurrentVersion)
            {
                warnings.Add(
                    $"file is version {document.Version} but this build writes version {TierFileDocument.CurrentVersion}; " +
                    "fields may be missing or have changed meaning. Delete the file to regenerate the defaults.");
            }

            var melee = LoadLadder(MeleeLadderName, document.Melee, DefaultLadders.CreateMelee, errors, out var meleeSource);
            var ranged = LoadLadder(RangedLadderName, document.Ranged, DefaultLadders.CreateRanged, errors, out var rangedSource);

            return new TierLoadResult(melee, meleeSource, ranged, rangedSource, fileWasMissing: false, errors, warnings);
        }

        private static TierTable LoadLadder(
            string name, List<TierEntry>? entries, Func<TierTable> createDefault, List<string> errors, out LadderSource source)
        {
            if (entries == null)
            {
                errors.Add($"{name} ladder is missing from the file; using the built-in default {name} ladder");
                source = LadderSource.Default;
                return createDefault();
            }

            // A null entry (a bare "null" in the list) is left null so
            // TierTable.Validate reports it by position like any other problem.
            var tiers = new TierDefinition[entries.Count];
            for (int i = 0; i < entries.Count; i++) tiers[i] = entries[i]?.ToDefinition()!;

            var problems = TierTable.Validate(tiers);
            if (problems.Count > 0)
            {
                foreach (var problem in problems) errors.Add($"{name} ladder: {problem}");
                errors.Add($"{name} ladder has {problems.Count} problem(s); using the built-in default {name} ladder");
                source = LadderSource.Default;
                return createDefault();
            }

            source = LadderSource.File;
            return new TierTable(tiers);
        }
    }
}
