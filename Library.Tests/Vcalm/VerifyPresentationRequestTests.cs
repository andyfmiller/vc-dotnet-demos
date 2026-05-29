using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class VerifyPresentationRequestTests
{
    private readonly ITestOutputHelper _output;

    public VerifyPresentationRequestTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var request = new VerifyPresentationRequest();

        Assert.NotNull(request.AdditionalData);
        Assert.Empty(request.AdditionalData);
    }

    [Fact]
    public void Constructor_LeavesOptionalProperties_AsNull()
    {
        var request = new VerifyPresentationRequest();

        Assert.Null(request.VerifiablePresentation);
        Assert.Null(request.Options);
    }

    [Fact]
    public void VerifyPresentationOptions_DefaultProperties_AreNull()
    {
        var options = new VerifyPresentationOptions();

        Assert.Null(options.Challenge);
        Assert.Null(options.Domain);
        Assert.Null(options.ReturnPresentation);
    }

    [Fact]
    public void VerifyPresentationOptions_Properties_CanBeSet()
    {
        var options = new VerifyPresentationOptions
        {
            Challenge = "6e62f66e-67de-11eb-b490-ef3eeefa55f2",
            Domain = "website.example",
            ReturnPresentation = true
        };

        Assert.Equal("6e62f66e-67de-11eb-b490-ef3eeefa55f2", options.Challenge);
        Assert.Equal("website.example", options.Domain);
        Assert.True(options.ReturnPresentation);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var vp = new VerifiablePresentation
        {
            Type = ["VerifiablePresentation"],
            Context = ["https://www.w3.org/ns/credentials/v2"]
        };
        var options = new VerifyPresentationOptions
        {
            Challenge = "6e62f66e-67de-11eb-b490-ef3eeefa55f2",
            Domain = "website.example",
            ReturnPresentation = false
        };
        var request = new VerifyPresentationRequest
        {
            VerifiablePresentation = vp,
            Options = options
        };

        Assert.NotNull(request.VerifiablePresentation);
        Assert.Contains("VerifiablePresentation", request.VerifiablePresentation.Type!);
        Assert.NotNull(request.Options);
        Assert.Equal("6e62f66e-67de-11eb-b490-ef3eeefa55f2", request.Options.Challenge);
        Assert.Equal("website.example", request.Options.Domain);
        Assert.False(request.Options.ReturnPresentation);
    }

    [Fact]
    public void Deserialize_WithVerifiablePresentationAndOptions_ReadsAllProperties()
    {
        var json = """
            {
                "verifiablePresentation": {
                    "@context": [
                        "https://www.w3.org/ns/credentials/v2",
                        "https://www.w3.org/ns/credentials/examples/v2"
                    ],
                    "type": ["VerifiablePresentation"],
                    "holder": "did:example:123",
                    "proof": {
                        "type": "DataIntegrityProof",
                        "cryptosuite": "ecdsa-rdfc-2019",
                        "created": "2024-01-11T19:14:04Z",
                        "challenge": "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                        "domain": "website.example",
                        "verificationMethod": "did:example:123#key1",
                        "proofPurpose": "authentication",
                        "proofValue": "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
                    }
                },
                "options": {
                    "challenge": "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                    "domain": "website.example",
                    "returnPresentation": true
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.VerifiablePresentation);
        Assert.NotNull(result.VerifiablePresentation.Context);
        Assert.Equal(2, result.VerifiablePresentation.Context.Count);
        Assert.Contains("VerifiablePresentation", result.VerifiablePresentation.Type!);
        Assert.NotNull(result.VerifiablePresentation.Proof);
        Assert.Equal("ecdsa-rdfc-2019", result.VerifiablePresentation.Proof.Cryptosuite);
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", result.VerifiablePresentation.Proof.Challenge);
        Assert.NotNull(result.Options);
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", result.Options.Challenge);
        Assert.Equal("website.example", result.Options.Domain);
        Assert.True(result.Options.ReturnPresentation);
    }

    [Fact]
    public void Deserialize_WithoutOptions_OptionsIsNull()
    {
        var json = """
            {
                "verifiablePresentation": {
                    "@context": ["https://www.w3.org/ns/credentials/v2"],
                    "type": ["VerifiablePresentation"],
                    "proof": {
                        "type": "DataIntegrityProof",
                        "proofValue": "zABCDEF"
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyPresentationRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.VerifiablePresentation);
        Assert.Null(result.Options);
    }

    [Fact]
    public void Serialize_WithOptions_IncludesChallengeAndDomainInJson()
    {
        var request = new VerifyPresentationRequest
        {
            Options = new VerifyPresentationOptions
            {
                Challenge = "6e62f66e-67de-11eb-b490-ef3eeefa55f2",
                Domain = "website.example",
                ReturnPresentation = true
            }
        };

        var json = KiotaJsonHelper.Serialize(request);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("options", out var optionsProp));
        Assert.Equal("6e62f66e-67de-11eb-b490-ef3eeefa55f2", optionsProp.GetProperty("challenge").GetString());
        Assert.Equal("website.example", optionsProp.GetProperty("domain").GetString());
        Assert.True(optionsProp.GetProperty("returnPresentation").GetBoolean());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesAllProperties()
    {
        var original = new VerifyPresentationRequest
        {
            VerifiablePresentation = new VerifiablePresentation
            {
                Context = ["https://www.w3.org/ns/credentials/v2"],
                Type = ["VerifiablePresentation"],
                Proof = new DataIntegrityProofWithChallenge
                {
                    Type = "DataIntegrityProof",
                    Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                    Domain = "website.example",
                    ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
                }
            },
            Options = new VerifyPresentationOptions
            {
                Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                Domain = "website.example",
                ReturnPresentation = false
            }
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, VerifyPresentationRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(roundTripped.VerifiablePresentation);
        Assert.NotNull(roundTripped.VerifiablePresentation.Proof);
        Assert.Equal(original.VerifiablePresentation.Proof.Challenge, roundTripped.VerifiablePresentation.Proof.Challenge);
        Assert.NotNull(roundTripped.Options);
        Assert.Equal(original.Options.Challenge, roundTripped.Options.Challenge);
        Assert.Equal(original.Options.Domain, roundTripped.Options.Domain);
        Assert.Equal(original.Options.ReturnPresentation, roundTripped.Options.ReturnPresentation);
    }
}
