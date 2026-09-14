using System;
using Newtonsoft.Json;

namespace WarbandSummoner.Core.Config
{
    /// <summary>
    /// The JSON layer for <see cref="TierFileDocument"/>. Strict on the way
    /// in — an unknown key is an error, because a misspelled field that
    /// silently does nothing is the worst way for a config file to fail.
    /// </summary>
    public static class TierFileJson
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Include,
        };

        public static string Serialize(TierFileDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            return JsonConvert.SerializeObject(document, Settings);
        }

        /// <summary>
        /// Parses a whole file. Throws <see cref="TierFileFormatException"/>
        /// with a human-readable message (line and position included where
        /// Newtonsoft provides them) for anything that is not a document.
        /// </summary>
        public static TierFileDocument Deserialize(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            TierFileDocument? document;
            try
            {
                document = JsonConvert.DeserializeObject<TierFileDocument>(json, Settings);
            }
            catch (JsonException ex)
            {
                throw new TierFileFormatException(ex.Message, ex);
            }

            // Newtonsoft returns null for the literal "null" and for whitespace-only input.
            return document ?? throw new TierFileFormatException("file is empty or contains no JSON object");
        }
    }

    /// <summary>The file could not be read as a <see cref="TierFileDocument"/> at all.</summary>
    public sealed class TierFileFormatException : Exception
    {
        public TierFileFormatException(string message) : base(message) { }

        public TierFileFormatException(string message, Exception inner) : base(message, inner) { }
    }
}
