using Library.Models.OpenBadges;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class IdentifierEntryTests
{
    [Fact]
    public void IdentifierEntry_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var entry = new IdentifierEntry
        {
            Identifier = "12345",
            IdentifierType = IdentifierType.FromType(IdentifierType.Terms.Identifier),
            Type = "IdentifierEntry"
        };

        // Assert
        Assert.Equal("12345", entry.Identifier);
        Assert.Equal(IdentifierType.FromType(IdentifierType.Terms.Identifier), entry.IdentifierType);
        Assert.Equal("IdentifierEntry", entry.Type);
    }

    [Fact]
    public void IdentifierEntry_EmailAddress_Type()
    {
        // Arrange & Act
        var entry = new IdentifierEntry
        {
            Identifier = "user@example.com",
            IdentifierType = IdentifierType.FromType(IdentifierType.Terms.EmailAddress),
            Type = "IdentifierEntry"
        };

        // Assert
        Assert.Equal("user@example.com", entry.Identifier);
        Assert.Equal(IdentifierType.FromType(IdentifierType.Terms.EmailAddress), entry.IdentifierType);
    }

    [Fact]
    public void IdentifierEntry_PhoneNumber_Type()
    {
        // Arrange & Act
        var entry = new IdentifierEntry
        {
            Identifier = "+1-555-1234",
            IdentifierType = IdentifierType.FromType("PhoneNumber"),
            Type = "IdentifierEntry"
        };

        // Assert
        Assert.Equal("+1-555-1234", entry.Identifier);
        Assert.Equal(IdentifierType.FromType("PhoneNumber"), entry.IdentifierType);
    }

    [Fact]
    public void IdentifierEntry_Url_Type()
    {
        // Arrange & Act
        var entry = new IdentifierEntry
        {
            Identifier = "https://example.com/profile",
            IdentifierType = IdentifierType.FromType("Url"),
            Type = "IdentifierEntry"
        };

        // Assert
        Assert.Equal("https://example.com/profile", entry.Identifier);
        Assert.Equal(IdentifierType.FromType("Url"), entry.IdentifierType);
    }

    [Fact]
    public void IdentifierEntry_NationalNumber_Type()
    {
        // Arrange & Act
        var entry = new IdentifierEntry
        {
            Identifier = "123-45-6789",
            IdentifierType = IdentifierType.FromType("NationalNumber"),
            Type = "IdentifierEntry"
        };

        // Assert
        Assert.Equal("123-45-6789", entry.Identifier);
        Assert.Equal(IdentifierType.FromType("NationalNumber"), entry.IdentifierType);
    }

    [Fact]
    public void IdentifierEntry_Type_MustBeIdentifierEntry()
    {
        // Arrange & Act
        var entry = new IdentifierEntry
        {
            Identifier = "12345",
            IdentifierType = IdentifierType.FromType("Identifier"),
            Type = "IdentifierEntry"
        };

        // Assert
        Assert.Equal("IdentifierEntry", entry.Type);
    }

    [Fact]
    public void IdentifierEntry_Serialization_Roundtrip()
    {
        // Arrange
        var entry = new IdentifierEntry
        {
            Identifier = "user@example.com",
            IdentifierType = IdentifierType.FromType("EmailAddress"),
            Type = "IdentifierEntry"
        };

        // Act
        var json = JsonSerializer.Serialize(entry);
        var deserialized = JsonSerializer.Deserialize<IdentifierEntry>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(entry.Identifier, deserialized.Identifier);
        Assert.Equal(entry.IdentifierType, deserialized.IdentifierType);
        Assert.Equal(entry.Type, deserialized.Type);
    }

    [Fact]
    public void IdentifierEntry_MultipleTypes_Supported()
    {
        // Test different identifier types
        var identifiers = new[]
        {
            new IdentifierEntry
            {
                Identifier = "user@example.com",
                IdentifierType = IdentifierType.FromType("EmailAddress"),
                Type = "IdentifierEntry"
            },
            new IdentifierEntry
            {
                Identifier = "+1-555-1234",
                IdentifierType = IdentifierType.FromType("PhoneNumber"),
                Type = "IdentifierEntry"
            },
            new IdentifierEntry
            {
                Identifier = "https://example.com/user",
                IdentifierType = IdentifierType.FromType("Url"),
                Type = "IdentifierEntry"
            },
            new IdentifierEntry
            {
                Identifier = "ID123456",
                IdentifierType = IdentifierType.FromType("Identifier"),
                Type = "IdentifierEntry"
            }
        };

        // Assert
        Assert.All(identifiers, id => Assert.Equal("IdentifierEntry", id.Type));
        Assert.Equal(IdentifierType.FromType("EmailAddress"), identifiers[0].IdentifierType);
        Assert.Equal(IdentifierType.FromType("PhoneNumber"), identifiers[1].IdentifierType);
        Assert.Equal(IdentifierType.FromType("Url"), identifiers[2].IdentifierType);
        Assert.Equal(IdentifierType.FromType("Identifier"), identifiers[3].IdentifierType);
    }
}