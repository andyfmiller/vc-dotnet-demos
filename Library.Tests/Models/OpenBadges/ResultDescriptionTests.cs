using Library.Models.OpenBadges;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class ResultDescriptionTests
{
    [Fact]
    public void ResultDescription_RequiredProperties_CanBeSet()
    {
        // Arrange & Act
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/grade",
            Name = "Final Grade",
            Type = new Collection<string> { "ResultDescription" }
        };

        // Assert
        Assert.Equal("https://example.com/results/grade", resultDesc.Id);
        Assert.Equal("Final Grade", resultDesc.Name);
        Assert.Contains("ResultDescription", resultDesc.Type);
    }

    [Fact]
    public void ResultDescription_Alignment_CollectionBehavior()
    {
        // Arrange
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/grade",
            Name = "Final Grade",
            Type = new Collection<string> { "ResultDescription" }
        };

        // Assert - Initially null when empty
        Assert.Null(resultDesc.Alignment);

        // Act
        resultDesc.Alignment = new Collection<Alignment>
        {
            new Alignment
            {
                TargetName = "Standard 1",
                TargetUrl = "https://example.com/standards/1",
                Type = new Collection<string> { "Alignment" }
            }
        };

        // Assert
        Assert.NotNull(resultDesc.Alignment);
        Assert.Single(resultDesc.Alignment);
    }

    [Fact]
    public void ResultDescription_AllowedValue_CollectionBehavior()
    {
        // Arrange
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/grade",
            Name = "Final Grade",
            Type = new Collection<string> { "ResultDescription" }
        };

        // Assert - Initially null when empty
        Assert.Null(resultDesc.AllowedValue);

        // Act - Set allowed values (A through F)
        resultDesc.AllowedValue = new Collection<string> { "F", "D", "C", "B", "A" };

        // Assert
        Assert.NotNull(resultDesc.AllowedValue);
        Assert.Equal(5, resultDesc.AllowedValue.Count);
    }

    [Fact]
    public void ResultDescription_ResultType_CanBeSet()
    {
        // Arrange & Act
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/grade",
            Name = "Final Grade",
            Type = new Collection<string> { "ResultDescription" },
            ResultType = ResultType.FromType("LetterGrade")
        };

        // Assert
        Assert.Equal(ResultType.FromType("LetterGrade"), resultDesc.ResultType);
    }

    [Fact]
    public void ResultDescription_RequiredLevel_CanBeSet()
    {
        // Arrange & Act
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/performance",
            Name = "Performance Level",
            Type = new Collection<string> { "ResultDescription" },
            RequiredLevel = "https://example.com/rubric/proficient"
        };

        // Assert
        Assert.Equal("https://example.com/rubric/proficient", resultDesc.RequiredLevel);
    }

    [Fact]
    public void ResultDescription_RequiredValue_CanBeSet()
    {
        // Arrange & Act
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/grade",
            Name = "Final Grade",
            Type = new Collection<string> { "ResultDescription" },
            RequiredValue = "C"
        };

        // Assert
        Assert.Equal("C", resultDesc.RequiredValue);
    }

    [Fact]
    public void ResultDescription_RubricCriterionLevel_CollectionBehavior()
    {
        // Arrange
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/performance",
            Name = "Performance Assessment",
            Type = new Collection<string> { "ResultDescription" }
        };

        // Assert - Initially null when empty
        Assert.Null(resultDesc.RubricCriterionLevel);

        // Act
        resultDesc.RubricCriterionLevel = new Collection<RubricCriterionLevel>
        {
            new RubricCriterionLevel
            {
                Id = "https://example.com/rubric/level/1",
                Name = "Developing",
                Type = new Collection<string> { "RubricCriterionLevel" }
            },
            new RubricCriterionLevel
            {
                Id = "https://example.com/rubric/level/2",
                Name = "Proficient",
                Type = new Collection<string> { "RubricCriterionLevel" }
            }
        };

        // Assert
        Assert.NotNull(resultDesc.RubricCriterionLevel);
        Assert.Equal(2, resultDesc.RubricCriterionLevel.Count);
    }

    [Fact]
    public void ResultDescription_ValueRange_CanBeSet()
    {
        // Arrange & Act
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/score",
            Name = "Test Score",
            Type = new Collection<string> { "ResultDescription" },
            ValueMin = "0",
            ValueMax = "100"
        };

        // Assert
        Assert.Equal("0", resultDesc.ValueMin);
        Assert.Equal("100", resultDesc.ValueMax);
    }

    [Fact]
    public void ResultDescription_Serialization_Roundtrip()
    {
        // Arrange
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/grade",
            Name = "Final Grade",
            Type = new Collection<string> { "ResultDescription" },
            ResultType = ResultType.FromType("LetterGrade"),
            RequiredValue = "C",
            AllowedValue = new Collection<string> { "F", "D", "C", "B", "A" }
        };

        // Act
        var json = JsonSerializer.Serialize(resultDesc);
        var deserialized = JsonSerializer.Deserialize<ResultDescription>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(resultDesc.Id, deserialized.Id);
        Assert.Equal(resultDesc.Name, deserialized.Name);
        Assert.Equal(resultDesc.ResultType, deserialized.ResultType);
        Assert.Equal(resultDesc.RequiredValue, deserialized.RequiredValue);
    }

    [Fact]
    public void ResultDescription_WithRubric_Configuration()
    {
        // Arrange & Act
        var resultDesc = new ResultDescription
        {
            Id = "https://example.com/results/rubric",
            Name = "Writing Assessment",
            Type = new Collection<string> { "ResultDescription" },
            ResultType = ResultType.FromType(ResultType.Terms.RubricCriterion),
            RequiredLevel = "https://example.com/rubric/level/proficient",
            RubricCriterionLevel = new Collection<RubricCriterionLevel>
            {
                new RubricCriterionLevel
                {
                    Id = "https://example.com/rubric/level/developing",
                    Name = "Developing",
                    Level = "1",
                    Points = "1",
                    Type = new Collection<string> { "RubricCriterionLevel" }
                },
                new RubricCriterionLevel
                {
                    Id = "https://example.com/rubric/level/proficient",
                    Name = "Proficient",
                    Level = "2",
                    Points = "3",
                    Type = new Collection<string> { "RubricCriterionLevel" }
                }
            }
        };

        // Assert
        Assert.Equal(ResultType.FromType(ResultType.Terms.RubricCriterion), resultDesc.ResultType);
        Assert.Equal(2, resultDesc.RubricCriterionLevel.Count);
    }
}