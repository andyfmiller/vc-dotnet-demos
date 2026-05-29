using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class VerifyPresentationResponseTests
{
    private readonly ITestOutputHelper _output;

    public VerifyPresentationResponseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifyPresentationResponse_InheritsVerificationResult()
    {
        var response = new VerifyPresentationResponse();

        Assert.IsAssignableFrom<VerificationResult>(response);
    }

    [Fact]
    public void Constructor_LeavesAllProperties_AsNull()
    {
        var response = new VerifyPresentationResponse();

        Assert.Null(response.Verified);
        Assert.Null(response.VerifiablePresentation);
        Assert.Null(response.ProblemDetails);
        Assert.Null(response.Results);
    }

    [Fact]
    public void Verified_CanBeSetToTrue()
    {
        var response = new VerifyPresentationResponse { Verified = true };

        Assert.True(response.Verified);
    }

    [Fact]
    public void Verified_CanBeSetToFalse()
    {
        var response = new VerifyPresentationResponse { Verified = false };

        Assert.False(response.Verified);
    }

    [Fact]
    public void Deserialize_FromSpecErrorExample_ReadsVerifiedAsFalse()
    {
        // Spec example from VerifyPresentationResult.yml (error case)
        var json = """
            {
                "verified": false,
                "verifiablePresentation": {},
                "problemDetails": [
                    {
                        "type": "https://www.w3.org/TR/vc-data-model-2.0/#PARSING_ERROR",
                        "title": "Parsing error",
                        "detail": "There was a parsing error on line 32, column 5."
                    }
                ]
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.False(result.Verified);
        Assert.NotNull(result.ProblemDetails);
        Assert.Single(result.ProblemDetails);
        Assert.Equal("https://www.w3.org/TR/vc-data-model-2.0/#PARSING_ERROR", result.ProblemDetails[0].Type);
        Assert.Equal("Parsing error", result.ProblemDetails[0].Title);
    }

    [Fact]
    public void Deserialize_WithResults_PresentationChallenge_ReadsResult()
    {
        // Spec example from VerifyPresentationResult.yml (success case results)
        var json = """
            {
                "verified": true,
                "results": {
                    "presentation": {
                        "challenge": {
                            "verified": true,
                            "input": "d436f0c8-fbd9-4e48-bbb2-55fc5d0920a8"
                        }
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.True(result.Verified);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.Presentation);
        Assert.NotNull(result.Results.Presentation.Challenge);
        Assert.True(result.Results.Presentation.Challenge.Verified);
        Assert.Equal("d436f0c8-fbd9-4e48-bbb2-55fc5d0920a8", result.Results.Presentation.Challenge.Input);
    }

    [Fact]
    public void Deserialize_WithResults_PresentationDomain_ReadsResult()
    {
        var json = """
            {
                "verified": true,
                "results": {
                    "presentation": {
                        "domain": {
                            "verified": true,
                            "input": "example.com"
                        }
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.Presentation);
        Assert.NotNull(result.Results.Presentation.Domain);
        Assert.True(result.Results.Presentation.Domain.Verified);
        Assert.Equal("example.com", result.Results.Presentation.Domain.Input);
    }

    [Fact]
    public void Deserialize_WithResults_PresentationHolder_ReadsResult()
    {
        var json = """
            {
                "verified": true,
                "results": {
                    "presentation": {
                        "holder": {
                            "verified": true,
                            "input": "did:example:123"
                        }
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.Presentation);
        Assert.NotNull(result.Results.Presentation.Holder);
        Assert.True(result.Results.Presentation.Holder.Verified);
        Assert.Equal("did:example:123", result.Results.Presentation.Holder.Input);
    }

    [Fact]
    public void Deserialize_WithResults_CredentialsList_ReadsAllCredentials()
    {
        var json = """
            {
                "verified": true,
                "results": {
                    "credentials": [
                        {
                            "verified": true,
                            "results": {
                                "validFrom": {
                                    "verified": true,
                                    "input": "2020-03-16T22:37:26.544Z"
                                }
                            }
                        }
                    ]
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.Credentials);
        Assert.Single(result.Results.Credentials);
        Assert.True(result.Results.Credentials[0].Verified);
    }

    [Fact]
    public void Deserialize_WithResults_PresentationProofs_ReadsPresentationProofs()
    {
        var json = """
            {
                "verified": true,
                "results": {
                    "presentation": {
                        "proof": [
                            {
                                "verified": true
                            }
                        ]
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.Presentation);
        Assert.NotNull(result.Results.Presentation.Proof);
        Assert.Single(result.Results.Presentation.Proof);
        Assert.True(result.Results.Presentation.Proof[0].Verified);
    }

    [Fact]
    public void Deserialize_FullSpecSuccessExample_ReadsAllResults()
    {
        // Full success example combining challenge and domain verification
        var json = """
            {
                "verified": true,
                "results": {
                    "presentation": {
                        "challenge": {
                            "verified": true,
                            "input": "d436f0c8-fbd9-4e48-bbb2-55fc5d0920a8"
                        },
                        "domain": {
                            "verified": true,
                            "input": "example.com"
                        }
                    },
                    "credentials": [
                        {
                            "verified": true
                        }
                    ]
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.True(result.Verified);
        Assert.NotNull(result.Results);
        Assert.NotNull(result.Results.Presentation);
        Assert.NotNull(result.Results.Presentation.Challenge);
        Assert.True(result.Results.Presentation.Challenge.Verified);
        Assert.Equal("d436f0c8-fbd9-4e48-bbb2-55fc5d0920a8", result.Results.Presentation.Challenge.Input);
        Assert.NotNull(result.Results.Presentation.Domain);
        Assert.True(result.Results.Presentation.Domain.Verified);
        Assert.Equal("example.com", result.Results.Presentation.Domain.Input);
        Assert.NotNull(result.Results.Credentials);
        Assert.Single(result.Results.Credentials);
    }

    [Fact]
    public void Serialize_WithVerifiedTrueAndPresentationResults_ProducesCorrectJson()
    {
        var response = new VerifyPresentationResponse
        {
            Verified = true,
            Results = new VerificationResult_results
            {
                Presentation = new VerificationResult_results_presentation
                {
                    Challenge = new VerificationResult_results_presentation_challenge
                    {
                        Verified = true,
                        Input = "d436f0c8-fbd9-4e48-bbb2-55fc5d0920a8"
                    }
                }
            }
        };

        var json = KiotaJsonHelper.Serialize(response);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.GetProperty("verified").GetBoolean());
        Assert.True(root.TryGetProperty("results", out var resultsProp));
        Assert.True(resultsProp.TryGetProperty("presentation", out var presentationProp));
        Assert.True(presentationProp.TryGetProperty("challenge", out var challengeProp));
        Assert.True(challengeProp.GetProperty("verified").GetBoolean());
        Assert.Equal("d436f0c8-fbd9-4e48-bbb2-55fc5d0920a8", challengeProp.GetProperty("input").GetString());
    }

    [Fact]
    public void RoundTrip_WithFullResults_PreservesAllProperties()
    {
        var original = new VerifyPresentationResponse
        {
            Verified = false,
            ProblemDetails =
            [
                new ProblemDetails
                {
                    Type = "https://www.w3.org/TR/vc-data-model-2.0/#PARSING_ERROR",
                    Title = "Parsing error",
                    Detail = "There was a parsing error."
                }
            ]
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, VerifyPresentationResponse.CreateFromDiscriminatorValue);

        Assert.Equal(original.Verified, roundTripped.Verified);
        Assert.NotNull(roundTripped.ProblemDetails);
        Assert.Single(roundTripped.ProblemDetails);
        Assert.Equal(original.ProblemDetails[0].Type, roundTripped.ProblemDetails[0].Type);
        Assert.Equal(original.ProblemDetails[0].Title, roundTripped.ProblemDetails[0].Title);
        Assert.Equal(original.ProblemDetails[0].Detail, roundTripped.ProblemDetails[0].Detail);
    }
}
