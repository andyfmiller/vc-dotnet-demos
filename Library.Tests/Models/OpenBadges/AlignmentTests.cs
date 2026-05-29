using Library.Models.OpenBadges;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class AlignmentTests
{
    [Fact]
    public void Alignment_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var alignment = new Alignment
        {
            TargetName = "CCSS.ELA-Literacy.W.11-12.1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment" }
        };

        // Assert
        Assert.Equal("CCSS.ELA-Literacy.W.11-12.1", alignment.TargetName);
        Assert.Equal("https://example.com/standards/1", alignment.TargetUrl);
        Assert.Contains("Alignment", alignment.Type);
    }

    [Fact]
    public void Alignment_OptionalProperties_CanBeSet()
    {
        // Arrange & Act
        var alignment = new Alignment
        {
            TargetName = "Standard 1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment" },
            TargetCode = "STD-001",
            TargetDescription = "Write arguments to support claims",
            TargetFramework = "Common Core State Standards"
        };

        // Assert
        Assert.Equal("STD-001", alignment.TargetCode);
        Assert.Equal("Write arguments to support claims", alignment.TargetDescription);
        Assert.Equal("Common Core State Standards", alignment.TargetFramework);
    }

    [Fact]
    public void Alignment_TargetType_CanBeSet()
    {
        // Arrange & Act
        var alignment = new Alignment
        {
            TargetName = "Standard 1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment" },
            TargetType = AlignmentTargetType.FromType(AlignmentTargetType.Terms.CFItem)
        };

        // Assert
        Assert.Equal(AlignmentTargetType.FromType(AlignmentTargetType.Terms.CFItem), alignment.TargetType);
    }

    [Fact]
    public void Alignment_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var alignment = new Alignment
        {
            TargetName = "Standard 1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment" }
        };
        alignment.AdditionalProperties["customField"] = "customValue";

        // Assert
        Assert.Contains("customField", alignment.AdditionalProperties.Keys);
        Assert.Equal("customValue", alignment.AdditionalProperties["customField"]);
    }

    [Fact]
    public void Alignment_Type_IsCollection()
    {
        // Arrange & Act
        var alignment = new Alignment
        {
            TargetName = "Standard 1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment", "CustomType" }
        };

        // Assert
        Assert.Equal(2, alignment.Type.Count);
        Assert.Contains("Alignment", alignment.Type);
        Assert.Contains("CustomType", alignment.Type);
    }

    [Fact]
    public void Alignment_AllOptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var alignment = new Alignment
        {
            TargetName = "Standard 1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment" }
        };

        // Assert
        Assert.Null(alignment.TargetCode);
        Assert.Null(alignment.TargetDescription);
        Assert.Null(alignment.TargetFramework);
        Assert.Null(alignment.TargetType);
    }

    [Fact]
    public void Alignment_Serialization_Roundtrip()
    {
        // Arrange
        var alignment = new Alignment
        {
            TargetName = "Standard 1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment" },
            TargetCode = "STD-001",
            TargetDescription = "Test description",
            TargetFramework = "Test Framework",
            TargetType = AlignmentTargetType.FromType(AlignmentTargetType.Terms.CFItem)
        };

        // Act
        var json = JsonSerializer.Serialize(alignment);
        var deserialized = JsonSerializer.Deserialize<Alignment>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(alignment.TargetName, deserialized.TargetName);
        Assert.Equal(alignment.TargetUrl, deserialized.TargetUrl);
        Assert.Equal(alignment.TargetCode, deserialized.TargetCode);
        Assert.Equal(alignment.TargetDescription, deserialized.TargetDescription);
        Assert.Equal(alignment.TargetFramework, deserialized.TargetFramework);
        Assert.Equal(alignment.TargetType, deserialized.TargetType);
    }

    [Fact]
    public void Alignment_Serialization_WithAdditionalProperties()
    {
        // Arrange
        var alignment = new Alignment
        {
            TargetName = "Standard 1",
            TargetUrl = "https://example.com/standards/1",
            Type = new Collection<string> { "Alignment" }
        };
        alignment.AdditionalProperties["grade"] = "9-12";

        // Act
        var json = JsonSerializer.Serialize(alignment);
        var deserialized = JsonSerializer.Deserialize<Alignment>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Contains("grade", deserialized.AdditionalProperties.Keys);
    }
}