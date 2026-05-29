using Library.Models.OpenBadges;
using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class AchievementTests
{
    [Fact]
    public void Achievement_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Assert
        Assert.NotNull(achievement.Criteria);
        Assert.Equal("Test Achievement Description", achievement.Description);
        Assert.Equal("https://example.com/achievements/1", achievement.Id);
        Assert.Equal("Test Achievement", achievement.Name);
    }

    [Fact]
    public void Achievement_OptionalProperties_CanBeSet()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            AchievementType = AchievementType.FromType(AchievementType.Terms.Course),
            CreditsAvailable = 3.0,
            FieldOfStudy = "Computer Science",
            HumanCode = "CS101",
            InLanguage = "en-US",
            Specialization = "Web Development"
        };

        // Assert
        Assert.Equal(AchievementType.FromType(AchievementType.Terms.Course), achievement.AchievementType);
        Assert.Equal(3.0, achievement.CreditsAvailable);
        Assert.Equal("Computer Science", achievement.FieldOfStudy);
        Assert.Equal("CS101", achievement.HumanCode);
        Assert.Equal("en-US", achievement.InLanguage);
        Assert.Equal("Web Development", achievement.Specialization);
    }

    [Fact]
    public void Achievement_Alignment_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Act - When empty, should return empty collection
        var emptyAlignment = achievement.Alignment;

        // Add an item
        achievement.Alignment = new Collection<Alignment>
        {
            new Alignment
            {
                TargetName = "Standard 1",
                TargetUrl = "https://example.com/standard/1",
                Type = new Collection<string> { "Alignment" }
            }
        };

        // Assert
        Assert.NotNull(emptyAlignment);
        Assert.Empty(emptyAlignment);
        Assert.NotNull(achievement.Alignment);
        Assert.Single(achievement.Alignment);
    }

    [Fact]
    public void Achievement_Endorsement_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Assert - When empty, should return empty collection
        Assert.NotNull(achievement.Endorsement);
        Assert.Empty(achievement.Endorsement);

        // Act - Set to null should initialize empty collection
        achievement.Endorsement = null!;

        // Assert
        Assert.NotNull(achievement.Endorsement);
        Assert.Empty(achievement.Endorsement);
    }

    [Fact]
    public void Achievement_EndorsementJwt_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Act & Assert - When empty, should return empty collection
        Assert.NotNull(achievement.EndorsementJwt);
        Assert.Empty(achievement.EndorsementJwt);

        // Add JWT strings
        achievement.EndorsementJwt = new Collection<string> { "jwt.token.here" };

        Assert.NotNull(achievement.EndorsementJwt);
        Assert.Single(achievement.EndorsementJwt);
    }

    [Fact]
    public void Achievement_OtherIdentifier_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Act & Assert
        Assert.NotNull(achievement.OtherIdentifier);
        Assert.Empty(achievement.OtherIdentifier);

        achievement.OtherIdentifier = new Collection<IdentifierEntry>
        {
            new IdentifierEntry
            {
                Identifier = "12345",
                IdentifierType = IdentifierType.FromType(IdentifierType.Terms.Identifier),
                Type = "IdentifierEntry"
            }
        };

        Assert.NotNull(achievement.OtherIdentifier);
        Assert.Single(achievement.OtherIdentifier);
    }

    [Fact]
    public void Achievement_Related_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Assert
        Assert.NotNull(achievement.Related);
        Assert.Empty(achievement.Related);

        // Act
        achievement.Related = new Collection<Related>
        {
            new Related
            {
                Id = "https://example.com/achievements/2",
                Type = new Collection<string> { "Related" }
            }
        };

        // Assert
        Assert.NotNull(achievement.Related);
        Assert.Single(achievement.Related);
    }

    [Fact]
    public void Achievement_ResultDescription_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Assert
        Assert.NotNull(achievement.ResultDescription);
        Assert.Empty(achievement.ResultDescription);

        // Act
        achievement.ResultDescription = new Collection<ResultDescription>
        {
            new ResultDescription
            {
                Id = "https://example.com/results/1",
                Name = "Final Grade",
                Type = new Collection<string> { "ResultDescription" }
            }
        };

        // Assert
        Assert.NotNull(achievement.ResultDescription);
        Assert.Single(achievement.ResultDescription);
    }

    [Fact]
    public void Achievement_Tag_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement"
        };

        // Assert
        Assert.NotNull(achievement.Tag);
        Assert.Empty(achievement.Tag);

        // Act
        achievement.Tag = new Collection<string> { "programming", "web development" };

        // Assert
        Assert.NotNull(achievement.Tag);
        Assert.Equal(2, achievement.Tag.Count);
    }

    [Fact]
    public void Achievement_Type_IsRequired()
    {
        // Arrange & Act
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Type = new Collection<string> { "Achievement" }
        };

        // Assert
        Assert.NotEmpty(achievement.Type);
        Assert.Contains("Achievement", achievement.Type);
    }

    [Fact]
    public void Achievement_Creator_CanBeSet()
    {
        // Arrange
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" }
        };

        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Creator = profile
        };

        // Assert
        Assert.NotNull(achievement.Creator);
        Assert.Equal("https://example.com/issuer", achievement.Creator.Id);
    }

    [Fact]
    public void Achievement_Image_CanBeSet()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Image = new Image
            {
                Id = "https://example.com/image.png",
                Type = "Image"
            }
        };

        // Assert
        Assert.NotNull(achievement.Image);
        Assert.Equal("https://example.com/image.png", achievement.Image.Id);
    }

    [Fact]
    public void Achievement_Serialization_Roundtrip()
    {
        // Arrange
        var achievement = new Achievement
        {
            Criteria = new Criteria { Narrative = "Complete the course" },
            Description = "Test Achievement Description",
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Type = new Collection<string> { "Achievement" },
            AchievementType = AchievementType.FromType(AchievementType.Terms.Course),
            CreditsAvailable = 3.0
        };

        // Act
        var json = JsonSerializer.Serialize(achievement);
        var deserialized = JsonSerializer.Deserialize<Achievement>(json);

        // Assert - Verify deserialization worked
        Assert.NotNull(deserialized);
        Assert.Equal(achievement.Id, deserialized.Id);
        Assert.Equal(achievement.Name, deserialized.Name);
        Assert.Equal(achievement.Description, deserialized.Description);
        Assert.Equal(achievement.AchievementType, deserialized.AchievementType);
        Assert.Equal(achievement.CreditsAvailable, deserialized.CreditsAvailable);
        
        // Assert - Verify empty collections are omitted from JSON
        Assert.DoesNotContain("\"alignment\"", json);
        Assert.DoesNotContain("\"endorsement\"", json);
        Assert.DoesNotContain("\"endorsementJwt\"", json);
        Assert.DoesNotContain("\"otherIdentifier\"", json);
        Assert.DoesNotContain("\"related\"", json);
        Assert.DoesNotContain("\"resultDescription\"", json);
        Assert.DoesNotContain("\"tag\"", json);
        
        // Assert - Verify deserialized collections are initialized (not null)
        Assert.NotNull(deserialized.Alignment);
        Assert.Empty(deserialized.Alignment);
        Assert.NotNull(deserialized.Endorsement);
        Assert.Empty(deserialized.Endorsement);
    }
}