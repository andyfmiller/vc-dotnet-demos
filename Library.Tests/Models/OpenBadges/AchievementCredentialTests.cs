using Library.Models.OpenBadges;
using Library.Models.Vc;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class AchievementCredentialTests
{
    [Fact]
    public void AchievementCredential_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var credential = new AchievementCredential
        {
            Context = new Collection<object>
            {
                "https://www.w3.org/ns/credentials/v2",
                "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
            },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile
            {
                Id = "https://example.com/issuer",
                Type = new Collection<string> { "Profile" }
            },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test Achievement",
                    Description = "Test Description",
                    Criteria = new Criteria { Narrative = "Complete the course" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            }
        };

        // Assert
        Assert.NotNull(credential.Context);
        Assert.Equal(2, credential.Context.Count);
        Assert.NotNull(credential.Type);
        Assert.Contains("AchievementCredential", credential.Type);
        Assert.NotNull(credential.CredentialSubject);
    }

    [Fact]
    public void AchievementCredential_Context_RequiresMinimumTwoItems()
    {
        // Arrange & Act
        var credential = new AchievementCredential
        {
            Context = new Collection<object>
            {
                "https://www.w3.org/ns/credentials/v2",
                "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
            },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile
            {
                Id = "https://example.com/issuer",
                Type = new Collection<string> { "Profile" }
            },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            }
        };

        // Assert
        Assert.True(credential.Context.Count >= 2);
        Assert.Equal("https://www.w3.org/ns/credentials/v2", credential.Context.ElementAt(0));
        Assert.Equal("https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json", credential.Context.ElementAt(1));
    }

    [Fact]
    public void AchievementCredential_Type_RequiresMinimumTwoItems()
    {
        // Arrange & Act
        var credential = new AchievementCredential
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile { Id = "https://example.com/issuer", Type = new Collection<string> { "Profile" } },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            }
        };

        // Assert
        Assert.True(credential.Type.Count >= 2);
        Assert.Contains("VerifiableCredential", credential.Type);
        Assert.Contains("AchievementCredential", credential.Type);
    }

    [Fact]
    public void AchievementCredential_AwardedDate_CanBeSet()
    {
        // Arrange
        var awardedDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var credential = new AchievementCredential
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile { Id = "https://example.com/issuer", Type = new Collection<string> { "Profile" } },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            },
            AwardedDate = awardedDate
        };

        // Assert
        Assert.Equal(awardedDate, credential.AwardedDate);
    }

    [Fact]
    public void AchievementCredential_Endorsement_CollectionBehavior()
    {
        // Arrange
        var credential = new AchievementCredential
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile { Id = "https://example.com/issuer", Type = new Collection<string> { "Profile" } },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            }
        };

        // Assert - Initially empty collection
        Assert.NotNull(credential.Endorsement);
        Assert.Empty(credential.Endorsement);

        // Act - Add endorsement
        credential.Endorsement = new Collection<EndorsementCredential>
        {
            new EndorsementCredential
            {
                Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
                Id = "https://example.com/endorsements/1",
                Type = new Collection<string> { "VerifiableCredential", "EndorsementCredential" },
                Issuer = new Profile { Id = "https://example.com/endorser", Type = new Collection<string> { "Profile" } },
                ValidFrom = DateTimeOffset.UtcNow,
                Name = "Endorsement",
                CredentialSubject = new EndorsementSubject
                {
                    Id = "https://example.com/subject",
                    Type = new Collection<string> { "EndorsementSubject" }
                }
            }
        };

        // Assert
        Assert.NotNull(credential.Endorsement);
        Assert.Single(credential.Endorsement);
    }

    [Fact]
    public void AchievementCredential_EndorsementJwt_CollectionBehavior()
    {
        // Arrange
        var credential = new AchievementCredential
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile { Id = "https://example.com/issuer", Type = new Collection<string> { "Profile" } },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            }
        };

        // Assert - Initially empty collection
        Assert.NotNull(credential.EndorsementJwt);
        Assert.Empty(credential.EndorsementJwt);

        // Act
        credential.EndorsementJwt = new Collection<string> { "eyJ0eXAiOiJKV1QiLCJhbGc..." };

        // Assert
        Assert.NotNull(credential.EndorsementJwt);
        Assert.Single(credential.EndorsementJwt);
    }

    [Fact]
    public void AchievementCredential_Image_CanBeSet()
    {
        // Arrange
        var image = new Image
        {
            Id = "https://example.com/badge.png",
            Type = "Image"
        };

        // Act
        var credential = new AchievementCredential
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile { Id = "https://example.com/issuer", Type = new Collection<string> { "Profile" } },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            },
            Image = image
        };

        // Assert
        Assert.NotNull(credential.Image);
        Assert.Equal("https://example.com/badge.png", credential.Image.Id);
    }

    [Fact]
    public void AchievementCredential_OpenBadgeCredential_TypeIsValid()
    {
        // Arrange & Act
        var credential = new AchievementCredential
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "OpenBadgeCredential" },
            Issuer = new Profile { Id = "https://example.com/issuer", Type = new Collection<string> { "Profile" } },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            }
        };

        // Assert
        Assert.Contains("OpenBadgeCredential", credential.Type);
    }

    [Fact]
    public void AchievementCredential_Serialization_Roundtrip()
    {
        // Arrange
        var credential = new AchievementCredential
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
            Id = "https://example.com/credentials/1",
            Type = new Collection<string> { "VerifiableCredential", "AchievementCredential" },
            Issuer = new Profile { Id = "https://example.com/issuer", Type = new Collection<string> { "Profile" } },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            AwardedDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new AchievementSubject()
            {
                Achievement = new Achievement
                {
                    Id = "https://example.com/achievements/1",
                    Name = "Test",
                    Description = "Test",
                    Criteria = new Criteria { Narrative = "Test" },
                    Type = new Collection<string> { "Achievement" }
                },
                Type = new Collection<string> { "AchievementSubject" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential);
        var deserialized = JsonSerializer.Deserialize<AchievementCredential>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(credential.Id, deserialized.Id);
        Assert.Equal(credential.AwardedDate, deserialized.AwardedDate);
    }
}