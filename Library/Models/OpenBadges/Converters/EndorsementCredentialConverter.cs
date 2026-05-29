namespace Library.Models.OpenBadges.Converters;

using Library.Models.Converters;
using Library.Models.OpenBadges;

/// <summary>
/// Class-level converter for EndorsementCredential that uses smart collection serialization.
/// Empty collections marked with [JsonIgnore(WhenWritingNull)] are omitted from JSON output
/// while remaining initialized in memory for backward compatibility.
/// </summary>
public class EndorsementCredentialConverter : BaseSmartSerializerConverter<EndorsementCredential>
{
    // Base class handles all serialization logic
}
