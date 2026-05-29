using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class VerifiableCredentialTests
{
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifiableCredential_InheritsCredentialProperties()
    {
        var vc = new VerifiableCredential
        {
            Id = "http://example.gov/credentials/3732",
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Type = ["VerifiableCredential"],
            IssuanceDate = "2020-03-16T22:37:26.544Z"
        };

        Assert.Equal("http://example.gov/credentials/3732", vc.Id);
        Assert.NotNull(vc.Context);
        Assert.Single(vc.Context);
        Assert.NotNull(vc.Type);
        Assert.Single(vc.Type);
        Assert.Equal("2020-03-16T22:37:26.544Z", vc.IssuanceDate);
    }

    [Fact]
    public void VerifiableCredential_HasProofProperty()
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
        var vc = new VerifiableCredential { Proof = proof };

        Assert.NotNull(vc.Proof);
        Assert.Equal("DataIntegrityProof", vc.Proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", vc.Proof.Cryptosuite);
    }

    [Fact]
    public void Deserialize_FromSpecExample_ReadsCredentialAndProof()
    {
        // Spec example from VerifiableCredential.yml
        var json = """
            {
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
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifiableCredential.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("http://example.gov/credentials/3732", result.Id);
        Assert.NotNull(result.Context);
        Assert.Equal(2, result.Context.Count);
        Assert.NotNull(result.Type);
        Assert.Equal(2, result.Type.Count);
        Assert.Contains("UniversityDegreeCredential", result.Type);
        Assert.Equal("2020-03-16T22:37:26.544Z", result.IssuanceDate);
        Assert.NotNull(result.Proof);
        Assert.Equal("DataIntegrityProof", result.Proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", result.Proof.Cryptosuite);
        Assert.Equal("2020-04-02T18:28:08Z", result.Proof.Created);
        Assert.Equal("assertionMethod", result.Proof.ProofPurpose);
        Assert.Equal("zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw", result.Proof.ProofValue);
    }

    [Fact]
    public void Deserialize_WithoutProof_LeavesProofNull()
    {
        var json = """
            {
                "@context": ["https://www.w3.org/ns/credentials/v2"],
                "id": "http://example.gov/credentials/1234",
                "type": ["VerifiableCredential"],
                "issuer": "did:example:123",
                "credentialSubject": { "id": "did:example:456" }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifiableCredential.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Null(result.Proof);
    }

    [Fact]
    public void Serialize_WithProof_IncludesProofInJson()
    {
        var vc = new VerifiableCredential
        {
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential"],
            Proof = new DataIntegrityProof
            {
                Type = "DataIntegrityProof",
                Cryptosuite = "ecdsa-rdfc-2019",
                Created = "2020-04-02T18:28:08Z",
                VerificationMethod = "did:example:123#key1",
                ProofPurpose = "assertionMethod",
                ProofValue = "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
            }
        };

        var json = KiotaJsonHelper.Serialize(vc);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("proof", out var proofProp));
        Assert.Equal("DataIntegrityProof", proofProp.GetProperty("type").GetString());
        Assert.Equal("ecdsa-rdfc-2019", proofProp.GetProperty("cryptosuite").GetString());
        Assert.Equal("assertionMethod", proofProp.GetProperty("proofPurpose").GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesCredentialAndProof()
    {
        var original = new VerifiableCredential
        {
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential"],
            IssuanceDate = "2020-03-16T22:37:26.544Z",
            Proof = new DataIntegrityProof
            {
                Type = "DataIntegrityProof",
                Cryptosuite = "ecdsa-rdfc-2019",
                Created = "2020-04-02T18:28:08Z",
                VerificationMethod = "did:example:123#key1",
                ProofPurpose = "assertionMethod",
                ProofValue = "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
            }
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, VerifiableCredential.CreateFromDiscriminatorValue);

        Assert.Equal(original.Id, roundTripped.Id);
        Assert.Equal(original.IssuanceDate, roundTripped.IssuanceDate);
        Assert.NotNull(roundTripped.Proof);
        Assert.Equal(original.Proof.Type, roundTripped.Proof.Type);
        Assert.Equal(original.Proof.Cryptosuite, roundTripped.Proof.Cryptosuite);
        Assert.Equal(original.Proof.ProofValue, roundTripped.Proof.ProofValue);
    }
}
