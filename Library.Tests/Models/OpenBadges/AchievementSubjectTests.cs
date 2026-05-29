
#pragma warning disable CS0618 // Type or member is obsolete

using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Library.Tests.Models2.OpenBadges;

public class AchievementSubjectTests
{
    [Fact]
    public void AchievementSubject_RequiredProperties_CanBeSet()
    {
        // Arrange
        Achievement achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        // Act
        AchievementSubject subject = new()
        {
            Achievement = achievement,
            Type = new Collection<string> { "AchievementSubject" }
        };

        // Assert
        Assert.NotNull(subject.Achievement);
        Assert.Equal("https://example.com/achievements/1", subject.Achievement.Id);
    }

    [Fact]
    public void AchievementSubject_Id_IsOptional()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        // Act
        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            Type = new Collection<string> { "AchievementSubject" }
        };

        // Assert
        Assert.Null(subject.Id);

        // Set Id
        subject.Id = "did:example:123";
        Assert.Equal("did:example:123", subject.Id);
    }

    [Fact]
    public void AchievementSubject_ActivityDates_CanBeSet()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        var startDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            ActivityStartDate = startDate,
            ActivityEndDate = endDate
        };

        // Assert
        Assert.Equal(startDate, subject.ActivityStartDate);
        Assert.Equal(endDate, subject.ActivityEndDate);
    }

    [Fact]
    public void AchievementSubject_CreditsEarned_CanBeSet()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        // Act
        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            CreditsEarned = 3.0
        };

        // Assert
        Assert.Equal(3.0, subject.CreditsEarned);
    }

    [Fact]
    public void AchievementSubject_Identifier_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            Type = new Collection<string> { "AchievementSubject" }
        };

        // Assert - Initially null when empty
        Assert.Null(subject.Identifier);

        // Act - Add identifier
        subject.Identifier = new Collection<IdentityObject>
        {
            new IdentityObject
            {
                IdentityHash = "test@example.com",
                IdentityType = IdentifierType.FromType(IdentifierType.Terms.EmailAddress),
                Type = "IdentityObject",
                Hashed = false
            }
        };

        // Assert
        Assert.NotNull(subject.Identifier);
        Assert.Single(subject.Identifier);
    }

    [Fact]
    public void AchievementSubject_Result_CollectionBehavior()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            Type = new Collection<string> { "AchievementSubject" }
        };

        // Assert - Initially null when empty
        Assert.Null(subject.Result);

        // Act - Add result
        subject.Result = new Collection<Result>
        {
            new Result
            {
                Type = new Collection<string> { "Result" },
                Value = "A"
            }
        };

        // Assert
        Assert.NotNull(subject.Result);
        Assert.Single(subject.Result);
        Assert.Equal("A", subject.Result.First().Value);
    }

    [Fact]
    public void AchievementSubject_Image_CanBeSet()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        var image = new Image
        {
            Id = "https://example.com/baked-badge.png",
            Type = "Image"
        };

        // Act
        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            Image = image
        };

        // Assert
        Assert.NotNull(subject.Image);
        Assert.Equal("https://example.com/baked-badge.png", subject.Image.Id);
    }

    [Fact]
    public void AchievementSubject_LicenseNumber_CanBeSet()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        // Act
        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            LicenseNumber = "LIC-12345"
        };

        // Assert
        Assert.Equal("LIC-12345", subject.LicenseNumber);
    }

    [Fact]
    public void AchievementSubject_Narrative_CanBeSet()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        // Act
        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            Narrative = "The student demonstrated excellence in all aspects of the course."
        };

        // Assert
        Assert.Equal("The student demonstrated excellence in all aspects of the course.", subject.Narrative);
    }

    [Fact]
    public void AchievementSubject_IdOrIdentifier_AtLeastOneRequired()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        var subjectWithId = new AchievementSubject()
        {
            Achievement = achievement,
            Id = "did:example:123"
        };

        var subjectWithIdentifier = new AchievementSubject()
        {
            Achievement = achievement,
            Identifier = new Collection<IdentityObject>
            {
                new IdentityObject
                {
                    IdentityHash = "test@example.com",
                    IdentityType = IdentifierType.FromType(IdentifierType.Terms.EmailAddress),
                    Type = "IdentityObject",
                    Hashed = false
                }
            }
        };

        // Assert
        Assert.NotNull(subjectWithId.Id);
        Assert.Null(subjectWithId.Identifier);

        Assert.Null(subjectWithIdentifier.Id);
        Assert.NotNull(subjectWithIdentifier.Identifier);
    }

    [Fact]
    public void AchievementSubject_Serialization_Roundtrip()
    {
        // Arrange
        var achievement = new Achievement
        {
            Id = "https://example.com/achievements/1",
            Name = "Test Achievement",
            Description = "Test Description",
            Criteria = new Criteria { Narrative = "Complete the course" },
            Type = new Collection<string> { "Achievement" }
        };

        var subject = new AchievementSubject()
        {
            Achievement = achievement,
            Id = "did:example:123",
            CreditsEarned = 3.0,
            LicenseNumber = "LIC-12345"
        };

        // Act
        var json = JsonSerializer.Serialize(subject);
        var deserialized = JsonSerializer.Deserialize<AchievementSubject>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(subject.Id, deserialized.Id);
        Assert.Equal(subject.CreditsEarned, deserialized.CreditsEarned);
        Assert.Equal(subject.LicenseNumber, deserialized.LicenseNumber);
    }
}

#pragma warning restore CS0618 // Type or member is obsolete