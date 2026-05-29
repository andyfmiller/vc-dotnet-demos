using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class ProblemDetailsTests
{
    private readonly ITestOutputHelper _output;

    public ProblemDetailsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var problemDetails = new ProblemDetails();

        Assert.NotNull(problemDetails.AdditionalData);
        Assert.Empty(problemDetails.AdditionalData);
    }

    [Fact]
    public void Constructor_LeavesOptionalProperties_AsNull()
    {
        var problemDetails = new ProblemDetails();

        Assert.Null(problemDetails.Type);
        Assert.Null(problemDetails.Title);
        Assert.Null(problemDetails.Detail);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://www.w3.org/TR/vc-data-model#PARSING_ERROR",
            Title = "PARSING_ERROR",
            Detail = "There was an error while parsing input."
        };

        Assert.Equal("https://www.w3.org/TR/vc-data-model#PARSING_ERROR", problemDetails.Type);
        Assert.Equal("PARSING_ERROR", problemDetails.Title);
        Assert.Equal("There was an error while parsing input.", problemDetails.Detail);
    }

    [Fact]
    public void Deserialize_FromSpecExample_ReadsAllProperties()
    {
        // Spec example from ProblemDetails.yml
        var json = """
            {
                "type": "https://www.w3.org/TR/vc-data-model#PARSING_ERROR",
                "title": "PARSING_ERROR",
                "detail": "There was an error while parsing input."
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, ProblemDetails.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("https://www.w3.org/TR/vc-data-model#PARSING_ERROR", result.Type);
        Assert.Equal("PARSING_ERROR", result.Title);
        Assert.Equal("There was an error while parsing input.", result.Detail);
    }

    [Fact]
    public void Deserialize_WithMissingOptionalFields_LeavesThemNull()
    {
        var json = """{ "type": "https://www.w3.org/TR/vc-data-model#PARSING_ERROR" }""";

        var result = KiotaJsonHelper.Deserialize(json, ProblemDetails.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("https://www.w3.org/TR/vc-data-model#PARSING_ERROR", result.Type);
        Assert.Null(result.Title);
        Assert.Null(result.Detail);
    }

    [Fact]
    public void Serialize_WithAllProperties_ProducesCorrectJson()
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://www.w3.org/TR/vc-data-model#PARSING_ERROR",
            Title = "PARSING_ERROR",
            Detail = "There was an error while parsing input."
        };

        var json = KiotaJsonHelper.Serialize(problemDetails);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("https://www.w3.org/TR/vc-data-model#PARSING_ERROR", root.GetProperty("type").GetString());
        Assert.Equal("PARSING_ERROR", root.GetProperty("title").GetString());
        Assert.Equal("There was an error while parsing input.", root.GetProperty("detail").GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesAllProperties()
    {
        var original = new ProblemDetails
        {
            Type = "https://www.w3.org/TR/vc-data-model#PARSING_ERROR",
            Title = "PARSING_ERROR",
            Detail = "There was an error while parsing input."
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, ProblemDetails.CreateFromDiscriminatorValue);

        Assert.Equal(original.Type, roundTripped.Type);
        Assert.Equal(original.Title, roundTripped.Title);
        Assert.Equal(original.Detail, roundTripped.Detail);
    }

    [Fact]
    public void Deserialize_VerifyCredentialModel2SpecExample_ReadsCorrectly()
    {
        // Spec example from VerifyCredentialResult.yml
        var json = """
            {
                "title": "PARSING_ERROR",
                "type": "https://www.w3.org/TR/vc-data-model#PARSING_ERROR "
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, ProblemDetails.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("PARSING_ERROR", result.Title);
    }

    [Fact]
    public void Deserialize_VerifyPresentationSpecExample_ReadsCorrectly()
    {
        // Spec example from VerifyPresentationResult.yml
        var json = """
            {
                "type": "https://www.w3.org/TR/vc-data-model-2.0/#PARSING_ERROR",
                "title": "Parsing error",
                "detail": "There was a parsing error on line 32, column 5."
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, ProblemDetails.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("https://www.w3.org/TR/vc-data-model-2.0/#PARSING_ERROR", result.Type);
        Assert.Equal("Parsing error", result.Title);
        Assert.Equal("There was a parsing error on line 32, column 5.", result.Detail);
    }
}
