using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class VerifyCredentialResponseTests
{
    private readonly ITestOutputHelper _output;

    public VerifyCredentialResponseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_LeavesAllProperties_AsNull()
    {
        var response = new VerifyCredentialResponse();

        Assert.Null(response.Verified);
        Assert.Null(response.Credential);
        Assert.Null(response.ProblemDetails);
        Assert.Null(response.Results);
    }

    [Fact]
    public void Verified_CanBeSetToTrue()
    {
        var response = new VerifyCredentialResponse { Verified = true };

        Assert.True(response.Verified);
    }

    [Fact]
    public void Verified_CanBeSetToFalse()
    {
        var response = new VerifyCredentialResponse { Verified = false };

        Assert.False(response.Verified);
    }

    [Fact]
    public void Deserialize_FromSpecErrorExample_ReadsVerifiedAsFalse()
    {
        // Spec example from VerifyCredentialResult.yml (error case)
        var json = """
            {
                "verified": false,
                "credential": {},
                "problemDetails": [
                    {
                        "title": "PARSING_ERROR",
                        "type": "https://www.w3.org/TR/vc-data-model#PARSING_ERROR "
                    }
                ]
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.False(result.Verified);
        Assert.NotNull(result.ProblemDetails);
        Assert.Single(result.ProblemDetails);
        Assert.Equal("PARSING_ERROR", result.ProblemDetails[0].Title);
    }

    [Fact]
    public void Deserialize_VerifiedTrue_WithNoProblems()
    {
        var json = """
            {
                "verified": true
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.True(result.Verified);
        Assert.Null(result.ProblemDetails);
        Assert.Null(result.Results);
    }

    [Fact]
    public void Deserialize_WithResults_ValidFrom_ReadsVerificationResult()
    {
        var json = """
            {
                "verified": true,
                "results": {
                    "validFrom": {
                        "verified": true,
                        "input": "2020-03-16T22:37:26.544Z"
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.True(result.Verified);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.ValidFrom);
        Assert.True(result.Results.ValidFrom.Verified);
        Assert.Equal("2020-03-16T22:37:26.544Z", result.Results.ValidFrom.Input);
    }

    [Fact]
    public void Deserialize_WithResults_ValidUntil_ReadsVerificationResult()
    {
        var json = """
            {
                "verified": true,
                "results": {
                    "validUntil": {
                        "verified": true,
                        "input": "2030-03-16T22:37:26.544Z"
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.ValidUntil);
        Assert.True(result.Results.ValidUntil.Verified);
        Assert.Equal("2030-03-16T22:37:26.544Z", result.Results.ValidUntil.Input);
    }

    [Fact]
    public void Deserialize_WithResults_ProofList_ReadsProofResults()
    {
        var json = """
            {
                "verified": true,
                "results": {
                    "proof": [
                        {
                            "verified": true,
                            "input": {
                                "type": "DataIntegrityProof",
                                "cryptosuite": "ecdsa-rdfc-2019"
                            }
                        }
                    ]
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.Proof);
        Assert.Single(result.Results.Proof);
        Assert.True(result.Results.Proof[0].Verified);
    }

    [Fact]
    public void Deserialize_WithMultipleProblemDetails_ReadsAll()
    {
        var json = """
            {
                "verified": false,
                "problemDetails": [
                    {
                        "type": "https://www.w3.org/TR/vc-data-model#PARSING_ERROR",
                        "title": "PARSING_ERROR",
                        "detail": "Error 1"
                    },
                    {
                        "type": "https://www.w3.org/TR/vc-data-model#SCHEMA_ERROR",
                        "title": "SCHEMA_ERROR",
                        "detail": "Error 2"
                    }
                ]
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.False(result.Verified);
        Assert.NotNull(result.ProblemDetails);
        Assert.Equal(2, result.ProblemDetails.Count);
        Assert.Equal("PARSING_ERROR", result.ProblemDetails[0].Title);
        Assert.Equal("SCHEMA_ERROR", result.ProblemDetails[1].Title);
    }

    [Fact]
    public void Serialize_WithVerifiedFalseAndProblemDetails_ProducesCorrectJson()
    {
        var response = new VerifyCredentialResponse
        {
            Verified = false,
            ProblemDetails =
            [
                new ProblemDetails
                {
                    Type = "https://www.w3.org/TR/vc-data-model#PARSING_ERROR",
                    Title = "PARSING_ERROR",
                    Detail = "There was an error while parsing input."
                }
            ]
        };

        var json = KiotaJsonHelper.Serialize(response);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.False(root.GetProperty("verified").GetBoolean());
        Assert.True(root.TryGetProperty("problemDetails", out var pdProp));
        Assert.Equal(JsonValueKind.Array, pdProp.ValueKind);
        Assert.Equal(1, pdProp.GetArrayLength());
    }

    [Fact]
    public void RoundTrip_WithVerifiedAndProblemDetails_PreservesAllProperties()
    {
        var original = new VerifyCredentialResponse
        {
            Verified = false,
            ProblemDetails =
            [
                new ProblemDetails
                {
                    Type = "https://www.w3.org/TR/vc-data-model#PARSING_ERROR",
                    Title = "PARSING_ERROR",
                    Detail = "There was an error while parsing input."
                }
            ]
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, VerifyCredentialResponse.CreateFromDiscriminatorValue);

        Assert.Equal(original.Verified, roundTripped.Verified);
        Assert.NotNull(roundTripped.ProblemDetails);
        Assert.Single(roundTripped.ProblemDetails);
        Assert.Equal(original.ProblemDetails[0].Type, roundTripped.ProblemDetails[0].Type);
        Assert.Equal(original.ProblemDetails[0].Title, roundTripped.ProblemDetails[0].Title);
        Assert.Equal(original.ProblemDetails[0].Detail, roundTripped.ProblemDetails[0].Detail);
    }
}
