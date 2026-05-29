using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class VerifyCredentialRequestTests
{
    private readonly ITestOutputHelper _output;

    public VerifyCredentialRequestTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var request = new VerifyCredentialRequest();

        Assert.NotNull(request.AdditionalData);
        Assert.Empty(request.AdditionalData);
    }

    [Fact]
    public void Constructor_LeavesOptionalProperties_AsNull()
    {
        var request = new VerifyCredentialRequest();

        Assert.Null(request.VerifiableCredential);
        Assert.Null(request.Options);
    }

    [Fact]
    public void VerifyCredentialOptions_DefaultProperties_AreNull()
    {
        var options = new VerifyCredentialOptions();

        Assert.Null(options.ReturnCredential);
        Assert.Null(options.ReturnProblemDetails);
        Assert.Null(options.ReturnResults);
    }

    [Fact]
    public void VerifyCredentialOptions_Properties_CanBeSet()
    {
        var options = new VerifyCredentialOptions
        {
            ReturnCredential = true,
            ReturnProblemDetails = true,
            ReturnResults = true
        };

        Assert.True(options.ReturnCredential);
        Assert.True(options.ReturnProblemDetails);
        Assert.True(options.ReturnResults);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var vc = new VerifiableCredential
        {
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential"]
        };
        var options = new VerifyCredentialOptions
        {
            ReturnCredential = false,
            ReturnProblemDetails = true,
            ReturnResults = true
        };
        var request = new VerifyCredentialRequest
        {
            VerifiableCredential = vc,
            Options = options
        };

        Assert.NotNull(request.VerifiableCredential);
        Assert.Equal("http://example.gov/credentials/3732", request.VerifiableCredential.Id);
        Assert.NotNull(request.Options);
        Assert.False(request.Options.ReturnCredential);
        Assert.True(request.Options.ReturnProblemDetails);
        Assert.True(request.Options.ReturnResults);
    }

    [Fact]
    public void Deserialize_WithVerifiableCredentialAndOptions_ReadsAllProperties()
    {
        var json = """
            {
                "verifiableCredential": {
                    "@context": [
                        "https://www.w3.org/ns/credentials/v2",
                        "https://www.w3.org/ns/credentials/examples/v2"
                    ],
                    "id": "http://example.gov/credentials/3732",
                    "type": ["VerifiableCredential", "UniversityDegreeCredential"],
                    "issuer": "did:example:123",
                    "issuanceDate": "2020-03-16T22:37:26.544Z",
                    "credentialSubject": { "id": "did:example:123" },
                    "proof": {
                        "type": "DataIntegrityProof",
                        "cryptosuite": "ecdsa-rdfc-2019",
                        "created": "2020-04-02T18:28:08Z",
                        "verificationMethod": "did:example:123#key1",
                        "proofPurpose": "assertionMethod",
                        "proofValue": "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
                    }
                },
                "options": {
                    "returnCredential": false,
                    "returnProblemDetails": true,
                    "returnResults": true
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.VerifiableCredential);
        Assert.Equal("http://example.gov/credentials/3732", result.VerifiableCredential.Id);
        Assert.NotNull(result.VerifiableCredential.Proof);
        Assert.Equal("ecdsa-rdfc-2019", result.VerifiableCredential.Proof.Cryptosuite);
        Assert.NotNull(result.Options);
        Assert.False(result.Options.ReturnCredential);
        Assert.True(result.Options.ReturnProblemDetails);
        Assert.True(result.Options.ReturnResults);
    }

    [Fact]
    public void Deserialize_WithoutOptions_OptionsIsNull()
    {
        var json = """
            {
                "verifiableCredential": {
                    "@context": ["https://www.w3.org/ns/credentials/v2"],
                    "type": ["VerifiableCredential"],
                    "issuer": "did:example:123",
                    "credentialSubject": { "id": "did:example:123" },
                    "proof": {
                        "type": "DataIntegrityProof",
                        "proofValue": "zABCDEF"
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifyCredentialRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.VerifiableCredential);
        Assert.Null(result.Options);
    }

    [Fact]
    public void Serialize_WithOptions_IncludesReturnFlagsInJson()
    {
        var request = new VerifyCredentialRequest
        {
            Options = new VerifyCredentialOptions
            {
                ReturnCredential = true,
                ReturnProblemDetails = true,
                ReturnResults = false
            }
        };

        var json = KiotaJsonHelper.Serialize(request);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("options", out var optionsProp));
        Assert.True(optionsProp.GetProperty("returnCredential").GetBoolean());
        Assert.True(optionsProp.GetProperty("returnProblemDetails").GetBoolean());
        Assert.False(optionsProp.GetProperty("returnResults").GetBoolean());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesVerifiableCredentialAndOptions()
    {
        var original = new VerifyCredentialRequest
        {
            VerifiableCredential = new VerifiableCredential
            {
                Context = ["https://www.w3.org/ns/credentials/v2"],
                Id = "http://example.gov/credentials/3732",
                Type = ["VerifiableCredential"],
                Proof = new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    ProofValue = "zABCDEF"
                }
            },
            Options = new VerifyCredentialOptions
            {
                ReturnCredential = true,
                ReturnProblemDetails = true,
                ReturnResults = true
            }
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, VerifyCredentialRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(roundTripped.VerifiableCredential);
        Assert.Equal(original.VerifiableCredential.Id, roundTripped.VerifiableCredential.Id);
        Assert.NotNull(roundTripped.Options);
        Assert.Equal(original.Options.ReturnCredential, roundTripped.Options.ReturnCredential);
        Assert.Equal(original.Options.ReturnProblemDetails, roundTripped.Options.ReturnProblemDetails);
        Assert.Equal(original.Options.ReturnResults, roundTripped.Options.ReturnResults);
    }
}
