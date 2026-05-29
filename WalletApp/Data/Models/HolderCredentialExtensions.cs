using Library.Models.OpenBadges;
using System.Text.Json;

namespace WalletApp.Data.Models
{
    /// <summary>A single changed property surfaced by <see cref="HolderCredentialExtensions.GetChanges"/>.</summary>
    public record CredentialChange(string PropertyPath, string? OldValue, string? NewValue);

    public static class HolderCredentialExtensions
    {
        /// <summary>
        /// Returns the best name to display for the given credential, based on the following precedence:
        /// 1. The <c>Name</c> property of the credential.
        /// 2. The <c>Name</c> property of the associated achievement.
        /// 3. A fallback value of "Unnamed Credential".
        /// </summary>
        public static string GetCredentialName(this HolderCredential credential)
        {
            // Convert the CredentialJson to an AchievementCredential object to access the Name and Achievement properties.
            AchievementCredential? vc = null;
            if (!string.IsNullOrEmpty(credential.CredentialJson))
            {
                try
                {
                    vc = JsonSerializer.Deserialize<AchievementCredential>(credential.CredentialJson);
                }
                catch (JsonException) { }
            }

            if (!string.IsNullOrEmpty(vc?.Name))
                return vc.Name;

            var achievement = vc?.CredentialSubject?.Achievement;
            if (achievement != null && !string.IsNullOrEmpty(achievement.Name))
                return achievement.Name;

            return "Unnamed Credential";
        }

        /// <summary>
        /// Returns the most-specific type from the VerifiableCredential's <c>type</c> array —
        /// i.e. the first entry that is not <c>"VerifiableCredential"</c>.
        /// Falls back to <c>"VerifiableCredential"</c> if no other type is present or the
        /// JSON cannot be parsed.
        /// </summary>
        public static string GetCredentialType(this HolderCredential credential)
        {
            const string fallback = "VerifiableCredential";

            if (string.IsNullOrEmpty(credential.CredentialJson))
                return fallback;

            try
            {
                using var doc = JsonDocument.Parse(credential.CredentialJson);
                if (!doc.RootElement.TryGetProperty("type", out var typeProp))
                    return fallback;

                if (typeProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in typeProp.EnumerateArray())
                    {
                        var t = item.GetString();
                        if (t != null && t != "VerifiableCredential")
                            return t;
                    }
                }
                else if (typeProp.ValueKind == JsonValueKind.String)
                {
                    var t = typeProp.GetString();
                    if (t != null && t != "VerifiableCredential")
                        return t;
                }

                return fallback;
            }
            catch (JsonException)
            {
                return fallback;
            }
        }

        /// <summary>
        /// Compares <see cref="HolderCredential.CredentialJson"/> with
        /// <see cref="HolderCredential.PreviousCredentialJson"/> and returns a flat list
        /// of leaf-property changes (additions, removals, and value changes).
        /// Returns an empty list when there is no previous version or either JSON is invalid.
        /// </summary>
        public static List<CredentialChange> GetChanges(this HolderCredential credential)
        {
            var changes = new List<CredentialChange>();

            if (string.IsNullOrEmpty(credential.PreviousCredentialJson) ||
                string.IsNullOrEmpty(credential.CredentialJson))
                return changes;

            try
            {
                using var oldDoc = JsonDocument.Parse(credential.PreviousCredentialJson);
                using var newDoc = JsonDocument.Parse(credential.CredentialJson);
                DiffElements(oldDoc.RootElement, newDoc.RootElement, string.Empty, changes);
            }
            catch (JsonException) { }

            return changes;
        }

        private static void DiffElements(
            JsonElement oldEl, JsonElement newEl, string path, List<CredentialChange> changes)
        {
            if (oldEl.ValueKind == JsonValueKind.Object && newEl.ValueKind == JsonValueKind.Object)
            {
                var oldProps = new Dictionary<string, JsonElement>();
                foreach (var p in oldEl.EnumerateObject())
                    oldProps[p.Name] = p.Value;

                var newProps = new Dictionary<string, JsonElement>();
                foreach (var p in newEl.EnumerateObject())
                    newProps[p.Name] = p.Value;

                foreach (var key in oldProps.Keys.Union(newProps.Keys))
                {
                    var childPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";
                    if (!newProps.TryGetValue(key, out var newChild))
                        changes.Add(new CredentialChange(childPath, Stringify(oldProps[key]), null));
                    else if (!oldProps.TryGetValue(key, out var oldChild))
                        changes.Add(new CredentialChange(childPath, null, Stringify(newChild)));
                    else
                        DiffElements(oldChild, newChild, childPath, changes);
                }
            }
            else if (oldEl.ValueKind == JsonValueKind.Array && newEl.ValueKind == JsonValueKind.Array)
            {
                var oldItems = oldEl.EnumerateArray().ToList();
                var newItems = newEl.EnumerateArray().ToList();
                var len = Math.Max(oldItems.Count, newItems.Count);
                for (var i = 0; i < len; i++)
                {
                    var childPath = $"{path}[{i}]";
                    if (i >= oldItems.Count)
                        changes.Add(new CredentialChange(childPath, null, Stringify(newItems[i])));
                    else if (i >= newItems.Count)
                        changes.Add(new CredentialChange(childPath, Stringify(oldItems[i]), null));
                    else
                        DiffElements(oldItems[i], newItems[i], childPath, changes);
                }
            }
            else
            {
                var oldStr = Stringify(oldEl);
                var newStr = Stringify(newEl);
                if (oldStr != newStr)
                    changes.Add(new CredentialChange(path, oldStr, newStr));
            }
        }

        private static string Stringify(JsonElement el) =>
            el.ValueKind switch
            {
                JsonValueKind.String => el.GetString() ?? string.Empty,
                JsonValueKind.Null   => "(null)",
                _                    => el.ToString()
            };
    }
}
