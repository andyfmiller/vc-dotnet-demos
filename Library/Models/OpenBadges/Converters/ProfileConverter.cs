namespace Library.Models.OpenBadges.Converters;

using Library.Models.Converters;
using Library.Models.OpenBadges;

/// <summary>
/// Class-level converter for Profile that uses smart collection serialization.
/// Empty collections marked with [JsonIgnore(WhenWritingNull)] are omitted from JSON output
/// while remaining initialized in memory for backward compatibility.
/// </summary>
public class ProfileConverter : BaseSmartSerializerConverter<Profile>
{
    // Base class handles all serialization logic
}

