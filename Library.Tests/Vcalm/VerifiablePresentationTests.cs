using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class VerifiablePresentationTests
{
    private readonly ITestOutputHelper _output;

    public VerifiablePresentationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifiablePresentation_InheritsPresentationProperties()
    {
        var vp = new VerifiablePresentation
        {
            Id = "http://example.com/presentations/1234",
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Type = ["VerifiablePresentation"]
        };

        Assert.Equal("http://example.com/presentations/1234", vp.Id);
        Assert.NotNull(vp.Context);
        Assert.Single(vp.Context);
        Assert.NotNull(vp.Type);
        Assert.Single(vp.Type);
    }

    [Fact]
    public void VerifiablePresentation_HasProofWithChallenge()
    {
        var proof = new DataIntegrityProofWithChallenge
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
            Domain = "example.com",
            ProofPurpose = "authentication"
        };
        var vp = new VerifiablePresentation { Proof = proof };

        Assert.NotNull(vp.Proof);
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", vp.Proof.Challenge);
        Assert.Equal("example.com", vp.Proof.Domain);
        Assert.Equal("authentication", vp.Proof.ProofPurpose);
    }

    [Fact]
    public void VerifiablePresentation_CanContainVerifiableCredentials()
    {
        var vc = new VerifiableCredential
        {
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential"],
            Context = ["https://www.w3.org/ns/credentials/v2"]
        };
        var vp = new VerifiablePresentation
        {
            VerifiableCredential = [vc]
        };

        Assert.NotNull(vp.VerifiableCredential);
        Assert.Single(vp.VerifiableCredential);
        Assert.Equal("http://example.gov/credentials/3732", vp.VerifiableCredential[0].Id);
    }

    [Fact]
    public void Deserialize_FromSpecExample_ReadsHolderAndProof()
    {
        // Spec example from VerifiablePresentation.yml
        var json = """
            {
                "@context": [
                    "https://www.w3.org/ns/credentials/v2",
                    "https://www.w3.org/ns/credentials/examples/v2"
                ],
                "type": ["VerifiablePresentation"],
                "holder": "did:example:123",
                "verifiableCredential": [
                    {
                        "@context": ["https://www.w3.org/ns/credentials/v2"],
                        "id": "http://example.gov/credentials/3732",
                        "type": ["VerifiableCredential", "UniversityDegreeCredential"],
                        "issuer": "did:example:123",
                        "issuanceDate": "2020-03-16T22:37:26.544Z",
                        "credentialSubject": { "id": "did:example:123" },
                        "proof": {
                            "type": "DataIntegrityProof",
                            "cryptosuite": "eddsa-rdfc-2022",
                            "created": "2020-04-02T18:28:08Z",
                            "verificationMethod": "did:example:123#z6MksHh7qHWvybLg5QTPPdG2DgEjjduBDArV9EF9mRiRzMBN",
                            "proofPurpose": "assertionMethod",
                            "proofValue": "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
                        }
                    }
                ],
                "proof": {
                    "type": "DataIntegrityProof",
                    "cryptosuite": "ecdsa-rdfc-2019",
                    "created": "2024-01-11T19:14:04Z",
                    "challenge": "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                    "domain": "example.com",
                    "verificationMethod": "https://di.example/holder#key1",
                    "proofPurpose": "authentication",
                    "proofValue": "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifiablePresentation.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Context);
        Assert.Equal(2, result.Context.Count);
        Assert.NotNull(result.Type);
        Assert.Contains("VerifiablePresentation", result.Type);
        Assert.NotNull(result.VerifiableCredential);
        Assert.Single(result.VerifiableCredential);
        Assert.Equal("http://example.gov/credentials/3732", result.VerifiableCredential[0].Id);
        var vcProof = result.VerifiableCredential[0].Proof;
        Assert.NotNull(vcProof);
        Assert.Equal("eddsa-rdfc-2022", vcProof.Cryptosuite);
        Assert.NotNull(result.Proof);
        Assert.Equal("DataIntegrityProof", result.Proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", result.Proof.Cryptosuite);
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", result.Proof.Challenge);
        Assert.Equal("example.com", result.Proof.Domain);
        Assert.Equal("authentication", result.Proof.ProofPurpose);
    }

    [Fact]
    public void Deserialize_WithoutProof_LeavesProofNull()
    {
        var json = """
            {
                "@context": ["https://www.w3.org/ns/credentials/v2"],
                "type": ["VerifiablePresentation"],
                "holder": "did:example:123"
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifiablePresentation.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Null(result.Proof);
    }

    [Fact]
    public void Deserialize_WithoutVerifiableCredentials_VerifiableCredentialIsNull()
    {
        var json = """
            {
                "@context": ["https://www.w3.org/ns/credentials/v2"],
                "type": ["VerifiablePresentation"]
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, VerifiablePresentation.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Null(result.VerifiableCredential);
    }

    [Fact]
    public void Serialize_WithProof_IncludesChallengeAndDomainInJson()
    {
        var vp = new VerifiablePresentation
        {
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Type = ["VerifiablePresentation"],
            Proof = new DataIntegrityProofWithChallenge
            {
                Type = "DataIntegrityProof",
                Cryptosuite = "ecdsa-rdfc-2019",
                Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                Domain = "example.com",
                ProofPurpose = "authentication",
                ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
            }
        };

        var json = KiotaJsonHelper.Serialize(vp);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("proof", out var proofProp));
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", proofProp.GetProperty("challenge").GetString());
        Assert.Equal("example.com", proofProp.GetProperty("domain").GetString());
        Assert.Equal("authentication", proofProp.GetProperty("proofPurpose").GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesAllProperties()
    {
        var original = new VerifiablePresentation
        {
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Type = ["VerifiablePresentation"],
            VerifiableCredential =
            [
                new VerifiableCredential
                {
                    Context = ["https://www.w3.org/ns/credentials/v2"],
                    Id = "http://example.gov/credentials/3732",
                    Type = ["VerifiableCredential"],
                    Proof = new DataIntegrityProof
                    {
                        Type = "DataIntegrityProof",
                        ProofValue = "zABCDEF"
                    }
                }
            ],
            Proof = new DataIntegrityProofWithChallenge
            {
                Type = "DataIntegrityProof",
                Cryptosuite = "ecdsa-rdfc-2019",
                Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                Domain = "example.com",
                ProofPurpose = "authentication",
                ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
            }
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, VerifiablePresentation.CreateFromDiscriminatorValue);

        Assert.NotNull(roundTripped.Context);
        Assert.Single(roundTripped.Context);
        Assert.NotNull(roundTripped.VerifiableCredential);
        Assert.Single(roundTripped.VerifiableCredential);
        Assert.Equal("http://example.gov/credentials/3732", roundTripped.VerifiableCredential[0].Id);
        Assert.NotNull(roundTripped.Proof);
        Assert.Equal(original.Proof.Challenge, roundTripped.Proof.Challenge);
        Assert.Equal(original.Proof.Domain, roundTripped.Proof.Domain);
        Assert.Equal(original.Proof.ProofValue, roundTripped.Proof.ProofValue);
    }
}
