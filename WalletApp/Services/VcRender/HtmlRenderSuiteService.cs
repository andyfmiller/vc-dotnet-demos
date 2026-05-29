using System.Text.Json;
using System.Text.Json.Nodes;

namespace WalletApp.Services.VcRender
{
    /// <summary>
    /// Implements the <em>host page</em> side of the W3C VC Rendering Methods v1.0
    /// <c>html</c> render suite (<c>TemplateRenderMethod</c> /
    /// <c>renderSuite: "html"</c>).
    ///
    /// Responsibilities:
    /// <list type="bullet">
    ///   <item>Locate the first <c>TemplateRenderMethod</c> with <c>renderSuite: "html"</c>
    ///         in the credential's <c>renderMethod</c> property.</item>
    ///   <item>Resolve the HTML template — either a <c>data:</c> URL or an HTTP URL.</item>
    ///   <item>Apply optional JSON Pointer (<c>renderProperty</c>) filtering to the
    ///         Verifiable Credential JSON.</item>
    ///   <item>Return a <see cref="HtmlRenderResult"/> containing the template fragment
    ///         and the (filtered) VC JSON so the Razor page can build the sandboxed
    ///         iframe wrapper code described in the specification.</item>
    /// </list>
    /// </summary>
    public class HtmlRenderSuiteService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HtmlRenderSuiteService> _logger;

        public HtmlRenderSuiteService(
            IHttpClientFactory httpClientFactory,
            ILogger<HtmlRenderSuiteService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Tries to find and resolve an <c>html</c> render suite entry in the supplied
        /// raw Verifiable Credential JSON.
        /// </summary>
        /// <param name="credentialJson">The raw JSON of the Verifiable Credential.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// An <see cref="HtmlRenderResult"/> if a usable <c>html</c> render method was
        /// found and resolved; otherwise <c>null</c>.
        /// </returns>
        public async Task<HtmlRenderResult?> TryResolveAsync(
            string credentialJson,
            CancellationToken cancellationToken = default)
        {
            JsonNode? root;
            try
            {
                root = JsonNode.Parse(credentialJson);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Could not parse credential JSON for HTML render suite.");
                return null;
            }

            if (root is null)
                return null;

            // ---- 1. Find renderMethod -----------------------------------------------
            // Per spec the property lives at VC top level. It may be a single object
            // or an array. We iterate all entries looking for the first html-suite match.

            var renderMethodNode = root["renderMethod"];
            if (renderMethodNode is null)
                return null;

            JsonObject? renderMethod = FindHtmlRenderMethod(renderMethodNode);
            if (renderMethod is null)
                return null;

            // ---- 2. Resolve template -------------------------------------------------
            string? templateHtml = await ResolveTemplateAsync(renderMethod, cancellationToken);
            if (templateHtml is null)
                return null;

            // ---- 3. Filter VC by renderProperty -------------------------------------
            var filteredJson = ApplyRenderPropertyFilter(root, renderMethod, credentialJson);

            return new HtmlRenderResult
            {
                TemplateHtml = templateHtml,
                VcJson = filteredJson
            };
        }

        // ─── helpers ─────────────────────────────────────────────────────────────────

        private static JsonObject? FindHtmlRenderMethod(JsonNode renderMethodNode)
        {
            // May be a single object or an array
            if (renderMethodNode is JsonObject obj)
                return IsHtmlSuite(obj) ? obj : null;

            if (renderMethodNode is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    if (item is JsonObject itemObj && IsHtmlSuite(itemObj))
                        return itemObj;
                }
            }

            return null;
        }

        private static bool IsHtmlSuite(JsonObject obj)
            => obj["type"]?.GetValue<string>() == "TemplateRenderMethod"
            && obj["renderSuite"]?.GetValue<string>() == "html";

        private async Task<string?> ResolveTemplateAsync(
            JsonObject renderMethod,
            CancellationToken cancellationToken)
        {
            var templateNode = renderMethod["template"];
            if (templateNode is null)
                return null;

            // Per spec, template may be a string (URL or data: URL) or an object
            // with an "id" property that is a URL.
            string? templateUrl = templateNode switch
            {
                JsonValue v => v.GetValue<string>(),
                JsonObject o => o["id"]?.GetValue<string>(),
                _ => null
            };

            if (templateUrl is null)
                return null;

            if (templateUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return ResolveDataUrl(templateUrl);

            return await FetchUrlAsync(templateUrl, cancellationToken);
        }

        /// <summary>
        /// Decodes a <c>data:[mediatype];base64,...</c> URL.
        /// </summary>
        private static string? ResolveDataUrl(string dataUrl)
        {
            // Format: data:[<mediatype>][;base64],<data>
            var commaIndex = dataUrl.IndexOf(',');
            if (commaIndex < 0)
                return null;

            var header = dataUrl[5..commaIndex]; // after "data:"
            var data = dataUrl[(commaIndex + 1)..];

            if (header.EndsWith(";base64", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var bytes = Convert.FromBase64String(data);
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return null;
                }
            }

            // Plain text, URL-encoded
            return Uri.UnescapeDataString(data);
        }

        private async Task<string?> FetchUrlAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(Constants.HttpClient.Default);
                var response = await client.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch HTML render template from {Url}.", url);
                return null;
            }
        }

        /// <summary>
        /// Applies JSON Pointer (<c>renderProperty</c>) filtering to the VC JSON.
        /// If <c>renderProperty</c> is absent the full VC JSON is returned unchanged.
        /// </summary>
        private static string ApplyRenderPropertyFilter(
            JsonNode vcRoot,
            JsonObject renderMethod,
            string originalJson)
        {
            var renderPropertyNode = renderMethod["renderProperty"];
            if (renderPropertyNode is not JsonArray renderPropertyArray)
                return originalJson;

            var pointers = renderPropertyArray
                .OfType<JsonValue>()
                .Select(v => v.GetValue<string>())
                .ToList();

            if (pointers.Count == 0)
                return originalJson;

            // Build a new JSON object containing only the selected paths.
            var filtered = new JsonObject();

            // Always include @context and type so the embedded VC remains valid.
            CopyNodeIfPresent(vcRoot, filtered, "@context");
            CopyNodeIfPresent(vcRoot, filtered, "type");

            foreach (var pointer in pointers)
            {
                ApplyPointer(vcRoot, filtered, pointer);
            }

            return filtered.ToJsonString();
        }

        private static void CopyNodeIfPresent(JsonNode source, JsonObject dest, string key)
        {
            var node = source[key];
            if (node is not null && !dest.ContainsKey(key))
                dest[key] = node.DeepClone();
        }

        /// <summary>
        /// Resolves a single RFC 6901 JSON Pointer against <paramref name="source"/> and
        /// grafts the result into the same path in <paramref name="dest"/>.
        /// </summary>
        private static void ApplyPointer(JsonNode source, JsonObject dest, string pointer)
        {
            if (string.IsNullOrEmpty(pointer) || pointer == "/")
                return;

            // Split pointer into segments, unescape ~1 → / and ~0 → ~
            var segments = pointer.TrimStart('/').Split('/')
                .Select(s => s.Replace("~1", "/").Replace("~0", "~"))
                .ToArray();

            // Walk source
            JsonNode? sourceNode = source;
            foreach (var seg in segments)
            {
                sourceNode = sourceNode switch
                {
                    JsonObject o => o[seg],
                    JsonArray a when int.TryParse(seg, out var i) && i < a.Count => a[i],
                    _ => null
                };
                if (sourceNode is null) return;
            }

            // Graft into dest
            JsonObject current = dest;
            for (int i = 0; i < segments.Length - 1; i++)
            {
                var seg = segments[i];
                if (!current.ContainsKey(seg))
                    current[seg] = new JsonObject();

                if (current[seg] is JsonObject child)
                    current = child;
                else
                    return; // collision — skip
            }

            var lastSeg = segments[^1];
            if (!current.ContainsKey(lastSeg))
                current[lastSeg] = sourceNode.DeepClone();
        }
    }
}
