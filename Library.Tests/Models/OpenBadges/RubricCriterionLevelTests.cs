using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class RubricCriterionLevelTests
{
    [Fact]
    public void RubricCriterionLevel_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel" }
        };

        // Assert
        Assert.Equal("https://example.com/rubric/level/1", level.Id);
        Assert.Equal("Proficient", level.Name);
        Assert.Contains("RubricCriterionLevel", level.Type);
    }

    [Fact]
    public void RubricCriterionLevel_Description_CanBeSet()
    {
        // Arrange & Act
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel" },
            Description = "Demonstrates clear understanding and application of concepts."
        };

        // Assert
        Assert.Equal("Demonstrates clear understanding and application of concepts.", level.Description);
    }

    [Fact]
    public void RubricCriterionLevel_Level_CanBeSet()
    {
        // Arrange & Act
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel" },
            Level = "3"
        };

        // Assert
        Assert.Equal("3", level.Level);
    }

    [Fact]
    public void RubricCriterionLevel_Points_CanBeSet()
    {
        // Arrange & Act
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel" },
            Points = "10"
        };

        // Assert
        Assert.Equal("10", level.Points);
    }

    [Fact]
    public void RubricCriterionLevel_Alignment_CollectionBehavior()
    {
        // Arrange
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel" }
        };

        // Assert - Initially null when empty
        Assert.Null(level.Alignment);

        // Act
        level.Alignment = new Collection<Alignment>
        {
            new Alignment
            {
                TargetName = "Standard 1",
                TargetUrl = "https://example.com/standards/1",
                Type = new Collection<string> { "Alignment" }
            }
        };

        // Assert
        Assert.NotNull(level.Alignment);
        Assert.Single(level.Alignment);
    }

    [Fact]
    public void RubricCriterionLevel_CompleteExample()
    {
        // Arrange & Act
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/mastery",
            Name = "Mastery",
            Type = new Collection<string> { "RubricCriterionLevel" },
            Description = "Demonstrates exceptional understanding and innovative application.",
            Level = "4",
            Points = "20"
        };

        // Assert
        Assert.Equal("Mastery", level.Name);
        Assert.Equal("4", level.Level);
        Assert.Equal("20", level.Points);
        Assert.NotNull(level.Description);
    }

    [Fact]
    public void RubricCriterionLevel_MultiplePerformanceLevels()
    {
        // Arrange - Typical rubric with 4 levels
        var levels = new[]
        {
            new RubricCriterionLevel
            {
                Id = "https://example.com/rubric/level/1",
                Name = "Beginning",
                Level = "1",
                Points = "5",
                Type = new Collection<string> { "RubricCriterionLevel" }
            },
            new RubricCriterionLevel
            {
                Id = "https://example.com/rubric/level/2",
                Name = "Developing",
                Level = "2",
                Points = "10",
                Type = new Collection<string> { "RubricCriterionLevel" }
            },
            new RubricCriterionLevel
            {
                Id = "https://example.com/rubric/level/3",
                Name = "Proficient",
                Level = "3",
                Points = "15",
                Type = new Collection<string> { "RubricCriterionLevel" }
            },
            new RubricCriterionLevel
            {
                Id = "https://example.com/rubric/level/4",
                Name = "Mastery",
                Level = "4",
                Points = "20",
                Type = new Collection<string> { "RubricCriterionLevel" }
            }
        };

        // Assert - Verify ordered from low to high
        Assert.Equal("1", levels[0].Level);
        Assert.Equal("2", levels[1].Level);
        Assert.Equal("3", levels[2].Level);
        Assert.Equal("4", levels[3].Level);
    }

    [Fact]
    public void RubricCriterionLevel_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel" }
        };
        level.AdditionalProperties["color"] = "#00FF00";

        // Assert
        Assert.Contains("color", level.AdditionalProperties.Keys);
    }

    [Fact]
    public void RubricCriterionLevel_Serialization_Roundtrip()
    {
        // Arrange
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel" },
            Description = "Test description",
            Level = "3",
            Points = "15"
        };

        // Act
        var json = JsonSerializer.Serialize(level);
        var deserialized = JsonSerializer.Deserialize<RubricCriterionLevel>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(level.Id, deserialized.Id);
        Assert.Equal(level.Name, deserialized.Name);
        Assert.Equal(level.Description, deserialized.Description);
        Assert.Equal(level.Level, deserialized.Level);
        Assert.Equal(level.Points, deserialized.Points);
    }

    [Fact]
    public void RubricCriterionLevel_Type_SupportsMultipleValues()
    {
        // Arrange & Act
        var level = new RubricCriterionLevel
        {
            Id = "https://example.com/rubric/level/1",
            Name = "Proficient",
            Type = new Collection<string> { "RubricCriterionLevel", "PerformanceLevel" }
        };

        // Assert
        Assert.Equal(2, level.Type.Count);
        Assert.Contains("RubricCriterionLevel", level.Type);
        Assert.Contains("PerformanceLevel", level.Type);
    }
}