using Library.Models.OpenBadges;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Library.Tests.Models2.OpenBadges;

public class CriteriaTests
{
    [Fact]
    public void Criteria_AllProperties_CanBeNull()
    {
        // Arrange & Act
        var criteria = new Criteria();

        // Assert
        Assert.Null(criteria.Id);
        Assert.Null(criteria.Narrative);
    }

    [Fact]
    public void Criteria_Id_CanBeSet()
    {
        // Arrange & Act
        var criteria = new Criteria
        {
            Id = "https://example.com/criteria/1"
        };

        // Assert
        Assert.Equal("https://example.com/criteria/1", criteria.Id);
    }

    [Fact]
    public void Criteria_Narrative_CanBeSet()
    {
        // Arrange & Act
        var criteria = new Criteria
        {
            Narrative = "Complete all course assignments with a grade of B or higher."
        };

        // Assert
        Assert.Equal("Complete all course assignments with a grade of B or higher.", criteria.Narrative);
    }

    [Fact]
    public void Criteria_BothProperties_CanBeSet()
    {
        // Arrange & Act
        var criteria = new Criteria
        {
            Id = "https://example.com/criteria/1",
            Narrative = "Complete the required coursework."
        };

        // Assert
        Assert.Equal("https://example.com/criteria/1", criteria.Id);
        Assert.Equal("Complete the required coursework.", criteria.Narrative);
    }

    [Fact]
    public void Criteria_AdditionalProperties_CanBeSet()
    {
        // Arrange & Act
        var criteria = new Criteria
        {
            Id = "https://example.com/criteria/1",
            Narrative = "Complete the coursework."
        };
        criteria.AdditionalProperties["customField"] = "customValue";

        // Assert
        Assert.Contains("customField", criteria.AdditionalProperties.Keys);
        Assert.Equal("customValue", criteria.AdditionalProperties["customField"]);
    }

    [Fact]
    public void Criteria_AdditionalProperties_InitializedWhenNull()
    {
        // Arrange & Act
        var criteria = new Criteria();
        
        // Access AdditionalProperties should initialize it
        var props = criteria.AdditionalProperties;

        // Assert
        Assert.NotNull(props);
        Assert.Empty(props);
    }

    [Fact]
    public void Criteria_Serialization_Roundtrip()
    {
        // Arrange
        var criteria = new Criteria
        {
            Id = "https://example.com/criteria/1",
            Narrative = "Complete all assignments"
        };

        // Act
        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<Criteria>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(criteria.Id, deserialized.Id);
        Assert.Equal(criteria.Narrative, deserialized.Narrative);
    }

    [Fact]
    public void Criteria_Serialization_WithAdditionalProperties()
    {
        // Arrange
        var criteria = new Criteria
        {
            Id = "https://example.com/criteria/1",
            Narrative = "Complete coursework"
        };
        criteria.AdditionalProperties["level"] = "advanced";
        criteria.AdditionalProperties["duration"] = "6 months";

        // Act
        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<Criteria>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.AdditionalProperties.Count);
        Assert.Equal("advanced", deserialized.AdditionalProperties["level"].ToString());
        Assert.Equal("6 months", deserialized.AdditionalProperties["duration"].ToString());
    }

    [Fact]
    public void Criteria_NarrativeSupportsMarkdown()
    {
        // Arrange & Act
        var criteria = new Criteria
        {
            Narrative = "# Requirements\n\n- Complete **all** assignments\n- Achieve *minimum* grade of B"
        };

        // Assert
        Assert.Contains("**all**", criteria.Narrative);
        Assert.Contains("*minimum*", criteria.Narrative);
        Assert.Contains("#", criteria.Narrative);
    }
}