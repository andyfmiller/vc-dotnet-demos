using Library.Models.OpenBadges;
using Library.Models.Vc;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class ProfileTests
{
    [Fact]
    public void Profile_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" }
        };

        // Assert
        Assert.Equal("https://example.com/issuer", profile.Id);
        Assert.Contains("Profile", profile.Type);
    }

    [Fact]
    public void Profile_Name_CanBeSet()
    {
        // Arrange & Act
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" },
            Name = "Example University"
        };

        // Assert
        Assert.Equal("Example University", profile.Name);
    }

    [Fact]
    public void Profile_PersonNames_CanBeSet()
    {
        // Arrange & Act
        var profile = new Profile
        {
            Id = "https://example.com/person",
            Type = new Collection<string> { "Profile" },
            GivenName = "John",
            FamilyName = "Doe",
            AdditionalName = "Michael",
            FamilyNamePrefix = "von",
            PatronymicName = "Ivanovich",
            HonorificPrefix = "Dr",
            HonorificSuffix = "PhD"
        };

        // Assert
        Assert.Equal("John", profile.GivenName);
        Assert.Equal("Doe", profile.FamilyName);
        Assert.Equal("Michael", profile.AdditionalName);
        Assert.Equal("von", profile.FamilyNamePrefix);
        Assert.Equal("Ivanovich", profile.PatronymicName);
        Assert.Equal("Dr", profile.HonorificPrefix);
        Assert.Equal("PhD", profile.HonorificSuffix);
    }

    [Fact]
    public void Profile_ContactInfo_CanBeSet()
    {
        // Arrange & Act
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" },
            Email = "contact@example.com",
            Phone = "+1-555-1234",
            Url = "https://example.com"
        };

        // Assert
        Assert.Equal("contact@example.com", profile.Email);
        Assert.Equal("+1-555-1234", profile.Phone);
        Assert.Equal("https://example.com", profile.Url);
    }

    [Fact]
    public void Profile_DateOfBirth_CanBeSet()
    {
        // Arrange
        var birthDate = new DateTime(1990, 5, 15);

        // Act
        var profile = new Profile
        {
            Id = "https://example.com/person",
            Type = new Collection<string> { "Profile" },
            DateOfBirth = birthDate
        };

        // Assert
        Assert.Equal(birthDate, profile.DateOfBirth);
    }

    [Fact]
    public void Profile_Description_CanBeSet()
    {
        // Arrange & Act
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" },
            Description = "A leading educational institution"
        };

        // Assert
        Assert.Equal("A leading educational institution", profile.Description);
    }

    [Fact]
    public void Profile_Address_CanBeSet()
    {
        // Arrange
        var address = new Address
        {
            StreetAddress = "123 Main St",
            AddressLocality = "Springfield",
            AddressRegion = "IL",
            PostalCode = "62701",
            AddressCountry = "USA",
            Type = new Collection<string> { "Address" }
        };

        // Act
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" },
            Address = address
        };

        // Assert
        Assert.NotNull(profile.Address);
        Assert.Equal("123 Main St", profile.Address.StreetAddress);
    }

    [Fact]
    public void Profile_Image_CanBeSet()
    {
        // Arrange
        var image = new Image
        {
            Id = "https://example.com/logo.png",
            Type = "Image"
        };

        // Act
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" },
            Image = image
        };

        // Assert
        Assert.NotNull(profile.Image);
        Assert.Equal("https://example.com/logo.png", profile.Image.Id);
    }

    [Fact]
    public void Profile_Official_CanBeSet()
    {
        // Arrange & Act
        var profile = new Profile
        {
            Id = "https://example.com/organization",
            Type = new Collection<string> { "Profile" },
            Official = "Jane Smith, Director"
        };

        // Assert
        Assert.Equal("Jane Smith, Director", profile.Official);
    }

    [Fact]
    public void Profile_ParentOrg_CanBeSet()
    {
        // Arrange
        var parentOrg = new Profile
        {
            Id = "https://example.com/parent-org",
            Type = new Collection<string> { "Profile" },
            Name = "Parent Organization"
        };

        // Act
        var profile = new Profile
        {
            Id = "https://example.com/department",
            Type = new Collection<string> { "Profile" },
            Name = "Department of Education",
            ParentOrg = parentOrg
        };

        // Assert
        Assert.NotNull(profile.ParentOrg);
        Assert.Equal("Parent Organization", profile.ParentOrg.Name);
    }

    [Fact]
    public void Profile_OtherIdentifier_CollectionBehavior()
    {
        // Arrange
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" }
        };

        // Assert - Initially empty collection
        Assert.NotNull(profile.OtherIdentifier);
        Assert.Empty(profile.OtherIdentifier);

        // Act
        profile.OtherIdentifier = new Collection<IdentifierEntry>
        {
            new IdentifierEntry
            {
                Identifier = "12345",
                IdentifierType = IdentifierType.FromType("Identifier"),
                Type = "IdentifierEntry"
            }
        };

        // Assert
        Assert.NotNull(profile.OtherIdentifier);
        Assert.Single(profile.OtherIdentifier);
    }

    [Fact]
    public void Profile_Endorsement_CollectionBehavior()
    {
        // Arrange
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" }
        };

        // Assert - Initially empty collection
        Assert.NotNull(profile.Endorsement);
        Assert.Empty(profile.Endorsement);

        // Act
        profile.Endorsement = new Collection<EndorsementCredential>
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
        Assert.NotNull(profile.Endorsement);
        Assert.Single(profile.Endorsement);
    }

    [Fact]
    public void Profile_EndorsementJwt_CollectionBehavior()
    {
        // Arrange
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" }
        };

        // Assert - Initially empty collection
        Assert.NotNull(profile.EndorsementJwt);
        Assert.Empty(profile.EndorsementJwt);

        // Act
        profile.EndorsementJwt = new Collection<string> { "eyJ0eXAiOiJKV1QiLCJhbGc..." };

        // Assert
        Assert.NotNull(profile.EndorsementJwt);
        Assert.Single(profile.EndorsementJwt);
    }

    [Fact]
    public void Profile_Serialization_Roundtrip()
    {
        // Arrange
        var profile = new Profile
        {
            Id = "https://example.com/issuer",
            Type = new Collection<string> { "Profile" },
            Name = "Example University",
            Email = "contact@example.com",
            Url = "https://example.com"
        };

        // Act
        var json = JsonSerializer.Serialize(profile);
        var deserialized = JsonSerializer.Deserialize<Profile>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(profile.Id, deserialized.Id);
        Assert.Equal(profile.Name, deserialized.Name);
        Assert.Equal(profile.Email, deserialized.Email);
        Assert.Equal(profile.Url, deserialized.Url);
    }
}