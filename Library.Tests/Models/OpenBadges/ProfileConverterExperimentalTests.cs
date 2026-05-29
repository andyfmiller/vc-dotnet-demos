using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

/// <summary>
/// Tests for the experimental ProfileConverter class-level converter
/// </summary>
public class ProfileConverterExperimentalTests
{
    [Fact]
    public void Profile_WithClassLevelConverter_OmitsEmptyCollections()
    {
        // Arrange
        var profile = new Profile { Id = "https://example.org/issuer" };

        // Act
        var json = JsonSerializer.Serialize(profile);

        // Assert - Empty collections should not appear in JSON
        Assert.DoesNotContain("\"endorsement\"", json);
        Assert.DoesNotContain("\"endorsementJwt\"", json);
        Assert.DoesNotContain("\"otherIdentifier\"", json);
    }

    [Fact]
    public void Profile_WithClassLevelConverter_IncludesNonEmptyCollections()
    {
        // Arrange
        var profile = new Profile 
        { 
            Id = "https://example.org/issuer",
            Type = new Collection<string> { "Profile" }
        };
        
        profile.EndorsementJwt.Add("test-jwt-token");

        // Act
        var json = JsonSerializer.Serialize(profile);

        // Assert - Non-empty collections should appear
        Assert.Contains("\"endorsementJwt\"", json);
        Assert.Contains("\"type\"", json);
        
        // Empty collections should still be omitted
        Assert.DoesNotContain("\"endorsement\"", json);
        Assert.DoesNotContain("\"otherIdentifier\"", json);
    }

    [Fact]
    public void Profile_WithClassLevelConverter_CollectionsAreInitialized()
    {
        // Arrange & Act
        var profile = new Profile { Id = "https://example.org/issuer" };

        // Assert - Collections should be initialized and accessible
        Assert.NotNull(profile.Endorsement);
        Assert.Empty(profile.Endorsement);
        Assert.NotNull(profile.EndorsementJwt);
        Assert.Empty(profile.EndorsementJwt);
        Assert.NotNull(profile.OtherIdentifier);
        Assert.Empty(profile.OtherIdentifier);
        
        // Should be able to add items
        profile.EndorsementJwt.Add("test");
        Assert.Single(profile.EndorsementJwt);
    }

    [Fact]
    public void Profile_WithClassLevelConverter_CanDeserialize()
    {
        // Arrange
        var json = """
        {
            "id": "https://example.org/issuer",
            "type": ["Profile"],
            "name": "Test Issuer",
            "endorsementJwt": ["jwt1", "jwt2"]
        }
        """;

        // Act
        var profile = JsonSerializer.Deserialize<Profile>(json);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal("https://example.org/issuer", profile.Id);
        Assert.Equal("Test Issuer", profile.Name);
        Assert.Equal(2, profile.EndorsementJwt.Count);
        Assert.Contains("jwt1", profile.EndorsementJwt);
        Assert.Contains("jwt2", profile.EndorsementJwt);
    }
}
