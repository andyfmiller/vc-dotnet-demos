using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using VDS.RDF.JsonLd;
using VDS.RDF.JsonLd.Syntax;

namespace Library.Crypto
{
    /// <summary>
    /// Canonicalizes a JSON-LD document using the URDNA2015 algorithm
    /// (RDF Dataset Normalization / RDFC-1.0), as required by the
    /// eddsa-rdfc-2022 Data Integrity cryptosuite.
    /// (https://www.w3.org/TR/vc-di-eddsa/#eddsa-rdfc-2022)
    /// </summary>
    public interface IJsonLdCanonicalizationService
    {
        /// <summary>
        /// Parses <paramref name="json"/> as JSON-LD, normalizes it with URDNA2015,
        /// and returns the resulting N-Quads document as UTF-8 bytes ready to be hashed.
        /// </summary>
        byte[] Canonicalize(string json);
    }

    /// <summary>
    /// URDNA2015 canonicalization using dotNetRdf.Core.
    ///
    /// Callers supply a seed dictionary of pre-loaded context JSON strings (keyed by
    /// context URL) via the constructor. This avoids embedding context files in the
    /// Library assembly itself — each host application (IssuerApp, WalletApp) owns its
    /// embedded resources and passes them in at registration time.
    ///
    /// Any context URL not found in the seed is fetched from the network on first use
    /// and then cached for the lifetime of the instance.
    /// </summary>
    public class JsonLdCanonicalizationService : IJsonLdCanonicalizationService
    {
        private static readonly HttpClient _http = new();
        private static readonly JsonSerializerSettings _jsonSettings =
            new() { DateParseHandling = DateParseHandling.None };

        // Per-instance cache so that multiple registrations (e.g. in tests) are isolated.
        private readonly ConcurrentDictionary<string, JToken> _contextCache = new();

        /// <summary>
        /// Well-known context URLs bundled as embedded resources in the Library assembly,
        /// mapped to their resource names.
        /// </summary>
        public static readonly (string Url, string ResourceName)[] LibraryContextEntries =
        [
            ("https://www.w3.org/ns/credentials/v2",
             "Library.JsonLdContexts.credentials_v2.json"),
            ("https://www.w3.org/2018/credentials/v1",
             "Library.JsonLdContexts.credentials_v1.json"),
            ("https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json",
             "Library.JsonLdContexts.ob_v3p0_context-3.0.3.json"),
            ("https://openbadgespec.org/v2",
             "Library.JsonLdContexts.openbadgespec_v2.json"),
        ];

        /// <summary>
        /// Creates a <see cref="JsonLdCanonicalizationService"/> pre-seeded with all
        /// context documents bundled in the Library assembly.
        /// </summary>
        public static JsonLdCanonicalizationService CreateWithLibraryContexts()
        {
            var contexts = LoadEmbeddedContexts(
                typeof(JsonLdCanonicalizationService).Assembly,
                LibraryContextEntries);
            return new JsonLdCanonicalizationService(contexts);
        }

        /// <summary>
        /// Loads embedded JSON-LD context resources from <paramref name="assembly"/> and
        /// returns them as a URL → JSON-string dictionary.
        /// </summary>
        public static IReadOnlyDictionary<string, string> LoadEmbeddedContexts(
            Assembly assembly,
            (string Url, string ResourceName)[] entries)
        {
            var dict = new Dictionary<string, string>();
            foreach (var (url, resourceName) in entries)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null) continue;
                using var reader = new StreamReader(stream, Encoding.UTF8);
                dict[url] = reader.ReadToEnd();
            }
            return dict;
        }

        /// <summary>
        /// Initializes the service and pre-populates the context cache from
        /// <paramref name="preloadedContexts"/>.
        /// </summary>
        /// <param name="preloadedContexts">
        /// Map of context URL → raw JSON string for well-known contexts the host has
        /// available as embedded resources. Pass an empty dictionary if none are available.
        /// </param>
        public JsonLdCanonicalizationService(IReadOnlyDictionary<string, string> preloadedContexts)
        {
            foreach (var (url, json) in preloadedContexts)
            {
                var token = JsonConvert.DeserializeObject<JToken>(json, _jsonSettings);
                if (token is not null)
                    _contextCache[url] = token;
            }
        }

        public byte[] Canonicalize(string json)
        {
            var jtoken = JsonConvert.DeserializeObject<JToken>(json, _jsonSettings)
                ?? throw new InvalidOperationException("JSON-LD input deserialized to null.");

            var options = new JsonLdProcessorOptions
            {
                ProcessingMode = JsonLdProcessingMode.JsonLd11,
                DocumentLoader = (uri, _) =>
                {
                    var url = uri.ToString();
                    var doc = _contextCache.GetOrAdd(url, u =>
                    {
                        var jsonStr = _http.GetStringAsync(u).GetAwaiter().GetResult();
                        return JsonConvert.DeserializeObject<JToken>(jsonStr, _jsonSettings)!;
                    });
                    return new RemoteDocument { DocumentUrl = uri, Document = doc };
                }
            };

            var expanded = JsonLdProcessor.Expand(jtoken, options);
            var nquads = JsonLdProcessor.Canonicalize(expanded);
            return Encoding.UTF8.GetBytes(nquads);
        }
    }
}
