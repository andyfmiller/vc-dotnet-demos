using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class IssueCredentialResponseTests
{
    private readonly ITestOutputHelper _output;

    public IssueCredentialResponseTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // --- IssueCredentialResponse ---

    [Fact]
    public void IssueCredentialResponse_Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var response = new IssueCredentialResponse();

        Assert.NotNull(response.AdditionalData);
        Assert.Empty(response.AdditionalData);
    }

    [Fact]
    public void IssueCredentialResponse_Constructor_LeavesResponseProp_AsNull()
    {
        var response = new IssueCredentialResponse();

        Assert.Null(response.IssueCredentialResponseProp);
    }

    [Fact]
    public void IssueCredentialResponse_Properties_CanBeSet()
    {
        var vcResponse = new VerifiableCredentialResponse();
        var response = new IssueCredentialResponse
        {
            IssueCredentialResponseProp = vcResponse
        };

        Assert.NotNull(response.IssueCredentialResponseProp);
        Assert.Same(vcResponse, response.IssueCredentialResponseProp);
    }

    // --- VerifiableCredentialResponse ---

    [Fact]
    public void VerifiableCredentialResponse_Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var response = new VerifiableCredentialResponse();

        Assert.NotNull(response.AdditionalData);
        Assert.Empty(response.AdditionalData);
    }

    [Fact]
    public void VerifiableCredentialResponse_Constructor_LeavesVerifiableCredential_AsNull()
    {
        var response = new VerifiableCredentialResponse();

        Assert.Null(response.VerifiableCredential);
    }

    [Fact]
    public void VerifiableCredentialResponse_Properties_CanBeSet()
    {
        var vc = new VerifiableCredential
        {
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential"]
        };
        var wrapper = new VerifiableCredentialResponse.VerifiableCredentialResponse_verifiableCredential
        {
            VerifiableCredential = vc
        };
        var response = new VerifiableCredentialResponse
        {
            VerifiableCredential = wrapper
        };

        Assert.NotNull(response.VerifiableCredential);
        Assert.NotNull(response.VerifiableCredential.VerifiableCredential);
        Assert.Equal("http://example.gov/credentials/3732", response.VerifiableCredential.VerifiableCredential.Id);
    }

    [Fact]
    public void VerifiableCredentialResponse_Deserialize_FromSpecExample_PopulatesVerifiableCredential()
    {
        // Spec response body for POST /credentials/issue (https://w3c.github.io/vcalm/#issue-credential)
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
                    "credentialSubject": {
                        "id": "did:example:123",
                        "degree": {
                            "type": "BachelorDegree",
                            "name": "Bachelor of Science and Arts"
                        }
                    },
                    "proof": {
                        "type": "DataIntegrityProof",
                        "cryptosuite": "ecdsa-rdfc-2019",
                        "created": "2020-04-02T18:28:08Z",
                        "verificationMethod": "did:example:123#z6MksHh7qHWvybLg5QTPPdG2DgEjjduBDArV9EF9mRiRzMBN",
                        "proofPurpose": "assertionMethod",
                        "proofValue": "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
                    }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifiableCredentialResponse.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.VerifiableCredential);
    }

    [Fact]
    public void VerifiableCredentialResponse_Serialize_WithVerifiableCredential_ProducesCorrectJson()
    {
        var proof = new DataIntegrityProof
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Created = "2020-04-02T18:28:08Z",
            VerificationMethod = "did:example:123#z6MksHh7qHWvybLg5QTPPdG2DgEjjduBDArV9EF9mRiRzMBN",
            ProofPurpose = "assertionMethod",
            ProofValue = "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
        };
        var vc = new VerifiableCredential
        {
            Context = ["https://www.w3.org/ns/credentials/v2", "https://www.w3.org/ns/credentials/examples/v2"],
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential", "UniversityDegreeCredential"],
            Proof = proof
        };
        var response = new VerifiableCredentialResponse
        {
            VerifiableCredential = new VerifiableCredentialResponse.VerifiableCredentialResponse_verifiableCredential
            {
                VerifiableCredential = vc
            }
        };

        var json = KiotaJsonHelper.Serialize(response);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var vcElement = root.GetProperty("verifiableCredential");
        Assert.Equal("http://example.gov/credentials/3732", vcElement.GetProperty("id").GetString());
        Assert.Equal("DataIntegrityProof", vcElement.GetProperty("proof").GetProperty("type").GetString());
        Assert.Equal("ecdsa-rdfc-2019", vcElement.GetProperty("proof").GetProperty("cryptosuite").GetString());
    }

    [Fact]
    public void VerifiableCredentialResponse_Serialize_WithEddsaProof_ProducesCorrectJson()
    {
        // Spec example using eddsa-rdfc-2022 cryptosuite
        var proof = new DataIntegrityProof
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "eddsa-rdfc-2022",
            Created = "2024-01-11T19:14:04Z",
            VerificationMethod = "did:example:123#z6MksHh7qHWvybLg5QTPPdG2DgEjjduBDArV9EF9mRiRzMBN",
            ProofPurpose = "assertionMethod",
            ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
        };
        var vc = new VerifiableCredential
        {
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Id = "http://example.gov/credentials/1234",
            Type = ["VerifiableCredential"],
            Proof = proof
        };
        var response = new VerifiableCredentialResponse
        {
            VerifiableCredential = new VerifiableCredentialResponse.VerifiableCredentialResponse_verifiableCredential
            {
                VerifiableCredential = vc
            }
        };

        var json = KiotaJsonHelper.Serialize(response);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var vcElement = doc.RootElement.GetProperty("verifiableCredential");
        Assert.Equal("eddsa-rdfc-2022", vcElement.GetProperty("proof").GetProperty("cryptosuite").GetString());
        Assert.Equal("assertionMethod", vcElement.GetProperty("proof").GetProperty("proofPurpose").GetString());
    }

    [Fact]
    public void VerifiableCredentialResponse_Serialize_WithEnvelopedVerifiableCredential_ProducesCorrectJson()
    {
        // The response can also contain an EnvelopedVerifiableCredential for non-application/vc media types
        var enveloped = new EnvelopedVerifiableCredential
        {
            Id = "data:application/vc+ld+json;base64,eyJAY29udGV4dCI6..."
        };
        var response = new VerifiableCredentialResponse
        {
            VerifiableCredential = new VerifiableCredentialResponse.VerifiableCredentialResponse_verifiableCredential
            {
                EnvelopedVerifiableCredential = enveloped
            }
        };

        var json = KiotaJsonHelper.Serialize(response);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("verifiableCredential", out _));
    }

    [Fact]
    public void VerifiableCredentialResponse_RoundTrip_ProducesJsonWithVerifiableCredentialKey()
    {
        var vc = new VerifiableCredential
        {
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential"],
            Proof = new DataIntegrityProof
            {
                Type = "DataIntegrityProof",
                Cryptosuite = "eddsa-rdfc-2022",
                Created = "2020-04-02T18:28:08Z",
                VerificationMethod = "did:example:123#key-1",
                ProofPurpose = "assertionMethod",
                ProofValue = "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
            }
        };
        var original = new VerifiableCredentialResponse
        {
            VerifiableCredential = new VerifiableCredentialResponse.VerifiableCredentialResponse_verifiableCredential
            {
                VerifiableCredential = vc
            }
        };

        var json = KiotaJsonHelper.Serialize(original);
        _output.WriteLine(json);

        // Verify the serialized JSON can be parsed and has the correct structure per the spec
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("verifiableCredential", out var vcElement));
        Assert.Equal("http://example.gov/credentials/3732", vcElement.GetProperty("id").GetString());

        // Verify the deserialized response contains the verifiableCredential wrapper
        var deserialized = KiotaJsonHelper.Deserialize(json, VerifiableCredentialResponse.CreateFromDiscriminatorValue);
        Assert.NotNull(deserialized.VerifiableCredential);
    }
}
