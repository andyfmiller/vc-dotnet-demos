using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class ResultTests
{
    [Fact]
    public void Result_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result" }
        };

        // Assert
        Assert.Contains("Result", result.Type);
    }

    [Fact]
    public void Result_AchievedLevel_CanBeSet()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result" },
            AchievedLevel = "https://example.com/rubric/level/mastered"
        };

        // Assert
        Assert.Equal("https://example.com/rubric/level/mastered", result.AchievedLevel);
    }

    [Fact]
    public void Result_Alignment_CollectionBehavior()
    {
        // Arrange
        var result = new Result
        {
            Type = new Collection<string> { "Result" }
        };

        // Assert - Initially null when empty
        Assert.Null(result.Alignment);

        // Act
        result.Alignment = new Collection<Alignment>
        {
            new Alignment
            {
                TargetName = "Standard 1",
                TargetUrl = "https://example.com/standards/1",
                Type = new Collection<string> { "Alignment" }
            }
        };

        // Assert
        Assert.NotNull(result.Alignment);
        Assert.Single(result.Alignment);
    }

    [Fact]
    public void Result_ResultDescription_CanBeSet()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result" },
            ResultDescription = "https://example.com/results/grade"
        };

        // Assert
        Assert.Equal("https://example.com/results/grade", result.ResultDescription);
    }

    [Fact]
    public void Result_Status_CanBeSet()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result" },
            Status = ResultStatusType.Completed
        };

        // Assert
        Assert.Equal(ResultStatusType.Completed, result.Status);
    }

    [Fact]
    public void Result_Value_CanBeSet()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result" },
            Value = "A"
        };

        // Assert
        Assert.Equal("A", result.Value);
    }

    [Fact]
    public void Result_AllOptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result" }
        };

        // Assert
        Assert.Null(result.AchievedLevel);
        Assert.Null(result.Alignment);
        Assert.Null(result.ResultDescription);
        Assert.Null(result.Status);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Result_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result" }
        };
        result.AdditionalProperties["customField"] = "customValue";

        // Assert
        Assert.Contains("customField", result.AdditionalProperties.Keys);
        Assert.Equal("customValue", result.AdditionalProperties["customField"]);
    }

    [Fact]
    public void Result_Type_SupportsMultipleValues()
    {
        // Arrange & Act
        var result = new Result
        {
            Type = new Collection<string> { "Result", "GradeResult" }
        };

        // Assert
        Assert.Equal(2, result.Type.Count);
        Assert.Contains("Result", result.Type);
        Assert.Contains("GradeResult", result.Type);
    }

    [Fact]
    public void Result_Serialization_Roundtrip()
    {
        // Arrange
        var result = new Result
        {
            Type = new Collection<string> { "Result" },
            AchievedLevel = "https://example.com/level/mastered",
            ResultDescription = "https://example.com/results/grade",
            Status = ResultStatusType.Completed,
            Value = "A"
        };

        // Act
        var json = JsonSerializer.Serialize(result);
        var deserialized = JsonSerializer.Deserialize<Result>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(result.AchievedLevel, deserialized.AchievedLevel);
        Assert.Equal(result.ResultDescription, deserialized.ResultDescription);
        Assert.Equal(result.Status, deserialized.Status);
        Assert.Equal(result.Value, deserialized.Value);
    }

    [Fact]
    public void Result_Serialization_WithAlignment()
    {
        // Arrange
        var result = new Result
        {
            Type = new Collection<string> { "Result" },
            Alignment = new Collection<Alignment>
            {
                new Alignment
                {
                    TargetName = "Standard 1",
                    TargetUrl = "https://example.com/standards/1",
                    Type = new Collection<string> { "Alignment" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(result);
        var deserialized = JsonSerializer.Deserialize<Result>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Alignment);
        Assert.Single(deserialized.Alignment);
    }
}