using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class EndorsementSubjectTests
{
    [Fact]
    public void EndorsementSubject_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/achievements/1",
            Type = new Collection<string> { "EndorsementSubject" }
        };

        // Assert
        Assert.Equal("https://example.com/achievements/1", subject.Id);
        Assert.Contains("EndorsementSubject", subject.Type);
    }

    [Fact]
    public void EndorsementSubject_EndorsementComment_CanBeSet()
    {
        // Arrange & Act
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/achievements/1",
            Type = new Collection<string> { "EndorsementSubject" },
            EndorsementComment = "This is an excellent achievement that demonstrates mastery."
        };

        // Assert
        Assert.Equal("This is an excellent achievement that demonstrates mastery.", subject.EndorsementComment);
    }

    [Fact]
    public void EndorsementSubject_Type_SupportsMultipleValues()
    {
        // Arrange & Act
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/achievements/1",
            Type = new Collection<string> { "EndorsementSubject", "Achievement" }
        };

        // Assert
        Assert.Equal(2, subject.Type.Count);
        Assert.Contains("EndorsementSubject", subject.Type);
        Assert.Contains("Achievement", subject.Type);
    }

    [Fact]
    public void EndorsementSubject_CanEndorseProfile()
    {
        // Arrange & Act
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/profiles/issuer123",
            Type = new Collection<string> { "EndorsementSubject", "Profile" },
            EndorsementComment = "This issuer is highly trusted and reputable."
        };

        // Assert
        Assert.Contains("Profile", subject.Type);
        Assert.Equal("This issuer is highly trusted and reputable.", subject.EndorsementComment);
    }

    [Fact]
    public void EndorsementSubject_CanEndorseCredential()
    {
        // Arrange & Act
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/credentials/abc123",
            Type = new Collection<string> { "EndorsementSubject", "VerifiableCredential" },
            EndorsementComment = "This credential meets our quality standards."
        };

        // Assert
        Assert.Contains("VerifiableCredential", subject.Type);
    }

    [Fact]
    public void EndorsementSubject_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/achievements/1",
            Type = new Collection<string> { "EndorsementSubject" }
        };
        subject.AdditionalProperties["rating"] = 5;

        // Assert
        Assert.Contains("rating", subject.AdditionalProperties.Keys);
    }

    [Fact]
    public void EndorsementSubject_EndorsementComment_IsOptional()
    {
        // Arrange & Act
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/achievements/1",
            Type = new Collection<string> { "EndorsementSubject" }
        };

        // Assert
        Assert.Null(subject.EndorsementComment);
    }

    [Fact]
    public void EndorsementSubject_Serialization_Roundtrip()
    {
        // Arrange
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/achievements/1",
            Type = new Collection<string> { "EndorsementSubject" },
            EndorsementComment = "Excellent work!"
        };

        // Act
        var json = JsonSerializer.Serialize(subject);
        var deserialized = JsonSerializer.Deserialize<EndorsementSubject>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(subject.Id, deserialized.Id);
        Assert.Equal(subject.EndorsementComment, deserialized.EndorsementComment);
        Assert.Contains("EndorsementSubject", deserialized.Type);
    }

    [Fact]
    public void EndorsementSubject_Serialization_WithAdditionalProperties()
    {
        // Arrange
        var subject = new EndorsementSubject
        {
            Id = "https://example.com/achievements/1",
            Type = new Collection<string> { "EndorsementSubject" }
        };
        subject.AdditionalProperties["endorsedDate"] = "2024-01-01";

        // Act
        var json = JsonSerializer.Serialize(subject);
        var deserialized = JsonSerializer.Deserialize<EndorsementSubject>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Contains("endorsedDate", deserialized.AdditionalProperties.Keys);
    }
}