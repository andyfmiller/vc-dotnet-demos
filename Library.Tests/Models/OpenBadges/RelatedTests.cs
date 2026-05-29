using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class RelatedTests
{
    [Fact]
    public void Related_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var related = new Related
        {
            Id = "https://example.com/achievements/2",
            Type = new Collection<string> { "Related" }
        };

        // Assert
        Assert.Equal("https://example.com/achievements/2", related.Id);
        Assert.Contains("Related", related.Type);
    }

    [Fact]
    public void Related_InLanguage_CanBeSet()
    {
        // Arrange & Act
        var related = new Related
        {
            Id = "https://example.com/achievements/2",
            Type = new Collection<string> { "Related" },
            InLanguage = "es-MX"
        };

        // Assert
        Assert.Equal("es-MX", related.InLanguage);
    }

    [Fact]
    public void Related_Version_CanBeSet()
    {
        // Arrange & Act
        var related = new Related
        {
            Id = "https://example.com/achievements/2",
            Type = new Collection<string> { "Related" },
            Version = "2.0"
        };

        // Assert
        Assert.Equal("2.0", related.Version);
    }

    [Fact]
    public void Related_AlternateLanguageEdition()
    {
        // Arrange & Act - Spanish version of an achievement
        var related = new Related
        {
            Id = "https://example.com/achievements/spanish-version",
            Type = new Collection<string> { "Related" },
            InLanguage = "es"
        };

        // Assert
        Assert.Equal("es", related.InLanguage);
    }

    [Fact]
    public void Related_PreviousVersion()
    {
        // Arrange & Act
        var related = new Related
        {
            Id = "https://example.com/achievements/v1",
            Type = new Collection<string> { "Related" },
            Version = "1.0"
        };

        // Assert
        Assert.Equal("1.0", related.Version);
    }

    [Fact]
    public void Related_InLanguage_ValidFormat()
    {
        // Valid language tag formats based on regex: ^[a-z]{2,4}(-[A-Z][a-z]{3})?(-([A-Z]{2}|[0-9]{3}))?$
        
        var examples = new[]
        {
            "en",           // Two-letter
            "eng",          // Three-letter
            "en-US",        // With region
            "zh-Hans",      // With script
            "zh-Hans-CN"    // With script and region
        };

        foreach (var langCode in examples)
        {
            // Arrange & Act
            var related = new Related
            {
                Id = "https://example.com/achievements/1",
                Type = new Collection<string> { "Related" },
                InLanguage = langCode
            };

            // Assert
            Assert.Equal(langCode, related.InLanguage);
        }
    }

    [Fact]
    public void Related_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var related = new Related
        {
            Id = "https://example.com/achievements/2",
            Type = new Collection<string> { "Related" }
        };
        related.AdditionalProperties["relationship"] = "supersedes";

        // Assert
        Assert.Contains("relationship", related.AdditionalProperties.Keys);
        Assert.Equal("supersedes", related.AdditionalProperties["relationship"]);
    }

    [Fact]
    public void Related_Type_SupportsMultipleValues()
    {
        // Arrange & Act
        var related = new Related
        {
            Id = "https://example.com/achievements/2",
            Type = new Collection<string> { "Related", "Achievement" }
        };

        // Assert
        Assert.Equal(2, related.Type.Count);
        Assert.Contains("Related", related.Type);
        Assert.Contains("Achievement", related.Type);
    }

    [Fact]
    public void Related_Serialization_Roundtrip()
    {
        // Arrange
        var related = new Related
        {
            Id = "https://example.com/achievements/2",
            Type = new Collection<string> { "Related" },
            InLanguage = "es-MX",
            Version = "2.0"
        };

        // Act
        var json = JsonSerializer.Serialize(related);
        var deserialized = JsonSerializer.Deserialize<Related>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(related.Id, deserialized.Id);
        Assert.Equal(related.InLanguage, deserialized.InLanguage);
        Assert.Equal(related.Version, deserialized.Version);
    }
}