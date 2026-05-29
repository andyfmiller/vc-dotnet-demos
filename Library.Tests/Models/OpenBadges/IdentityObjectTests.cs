#pragma warning disable CS0618 // Type or member is obsolete

using Library.Models.OpenBadges;
using System.Text.Json;

namespace Library.Tests.Models2.OpenBadges;

public class IdentityObjectTests
{
    [Fact]
    public void IdentityObject_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var identity = new IdentityObject
        {
            IdentityHash = "test@example.com",
            IdentityType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentityObject",
            Hashed = false
        };

        // Assert
        Assert.Equal("test@example.com", identity.IdentityHash);
        Assert.Equal(IdentifierType.FromType("EmailAddress"), identity.IdentityType);
        Assert.Equal("IdentityObject", identity.Type);
        Assert.False(identity.Hashed);
    }

    [Fact]
    public void IdentityObject_PlaintextEmail_Recommended()
    {
        // Arrange & Act
        var identity = new IdentityObject
        {
            IdentityHash = "user@example.com",
            IdentityType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentityObject",
            Hashed = false
        };

        // Assert
        Assert.False(identity.Hashed);
        Assert.Equal("user@example.com", identity.IdentityHash);
    }

    [Fact]
    public void IdentityObject_Salt_IsObsolete()
    {
        // Arrange & Act
        var identity = new IdentityObject
        {
            IdentityHash = "hashedvalue",
            IdentityType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentityObject",
            Hashed = true,
            Salt = "randomsalt"
        };

        // Assert
        Assert.Equal("randomsalt", identity.Salt);
    }

    [Fact]
    public void IdentityObject_Hashed_IsObsolete()
    {
        // Arrange & Act
        var identity = new IdentityObject
        {
            IdentityHash = "test@example.com",
            IdentityType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentityObject",
            Hashed = false
        };

        // Assert - Even though obsolete, it's still required
        Assert.False(identity.Hashed);
    }

    [Fact]
    public void IdentityObject_DifferentIdentifierTypes_Supported()
    {
        // Arrange & Act
        var emailIdentity = new IdentityObject
        {
            IdentityHash = "test@example.com",
            IdentityType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentityObject",
            Hashed = false
        };

        var phoneIdentity = new IdentityObject
        {
            IdentityHash = "+1-555-1234",
            IdentityType = IdentifierType.FromType("PhoneNumber"),
            Type = "IdentityObject",
            Hashed = false
        };

        var urlIdentity = new IdentityObject
        {
            IdentityHash = "https://example.com/profile",
            IdentityType = IdentifierType.FromType("Url"),
            Type = "IdentityObject",
            Hashed = false
        };

        // Assert
        Assert.Equal(IdentifierType.FromType("EmailAddress"), emailIdentity.IdentityType);
        Assert.Equal(IdentifierType.FromType("PhoneNumber"), phoneIdentity.IdentityType);
        Assert.Equal(IdentifierType.FromType("Url"), urlIdentity.IdentityType);
    }

    [Fact]
    public void IdentityObject_Serialization_Roundtrip()
    {
        // Arrange
        var identity = new IdentityObject
        {
            IdentityHash = "test@example.com",
            IdentityType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentityObject",
            Hashed = false
        };

        // Act
        var json = JsonSerializer.Serialize(identity);
        var deserialized = JsonSerializer.Deserialize<IdentityObject>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(identity.IdentityHash, deserialized.IdentityHash);
        Assert.Equal(identity.IdentityType, deserialized.IdentityType);
        Assert.Equal(identity.Type, deserialized.Type);
        Assert.Equal(identity.Hashed, deserialized.Hashed);
    }

    [Fact]
    public void IdentityObject_Type_MustBeIdentityObject()
    {
        // Arrange & Act
        var identity = new IdentityObject
        {
            IdentityHash = "test@example.com",
            IdentityType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentityObject",
            Hashed = false
        };

        // Assert
        Assert.Equal("IdentityObject", identity.Type);
    }
}

#pragma warning restore CS0618 // Type or member is obsolete