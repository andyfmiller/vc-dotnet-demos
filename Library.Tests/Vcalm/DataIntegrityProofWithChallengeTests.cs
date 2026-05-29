using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class DataIntegrityProofWithChallengeTests
{
    private readonly ITestOutputHelper _output;

    public DataIntegrityProofWithChallengeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var proof = new DataIntegrityProofWithChallenge();

        Assert.NotNull(proof.AdditionalData);
        Assert.Empty(proof.AdditionalData);
    }

    [Fact]
    public void Constructor_LeavesAllProperties_AsNull()
    {
        var proof = new DataIntegrityProofWithChallenge();

        Assert.Null(proof.Type);
        Assert.Null(proof.Cryptosuite);
        Assert.Null(proof.Created);
        Assert.Null(proof.Challenge);
        Assert.Null(proof.Domain);
        Assert.Null(proof.Nonce);
        Assert.Null(proof.VerificationMethod);
        Assert.Null(proof.ProofPurpose);
        Assert.Null(proof.ProofValue);
    }

    [Fact]
    public void Properties_IncludeChallenge_AndDomain()
    {
        var proof = new DataIntegrityProofWithChallenge
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
            Domain = "domain.example",
            ProofPurpose = "authentication"
        };

        Assert.Equal("DataIntegrityProof", proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", proof.Cryptosuite);
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", proof.Challenge);
        Assert.Equal("domain.example", proof.Domain);
        Assert.Equal("authentication", proof.ProofPurpose);
    }

    [Fact]
    public void Deserialize_FromSpecExample_ReadsAllProperties()
    {
        // Spec example from DataIntegrityProofWithChallenge.yml
        var json = """
            {
                "type": "DataIntegrityProof",
                "cryptosuite": "ecdsa-rdfc-2019",
                "created": "2024-01-11T19:14:04Z",
                "domain": "domain.example",
                "challenge": "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
                "verificationMethod": "https://di.example/holder#zDnadqAtcdP6agfXsTWu5darHnbGyTDkKGFYmn8dQraYzCJMZ",
                "proofPurpose": "authentication",
                "proofValue": "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, DataIntegrityProofWithChallenge.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("DataIntegrityProof", result.Type);
        Assert.Equal("ecdsa-rdfc-2019", result.Cryptosuite);
        Assert.Equal("2024-01-11T19:14:04Z", result.Created);
        Assert.Equal("domain.example", result.Domain);
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", result.Challenge);
        Assert.Equal("https://di.example/holder#zDnadqAtcdP6agfXsTWu5darHnbGyTDkKGFYmn8dQraYzCJMZ", result.VerificationMethod);
        Assert.Equal("authentication", result.ProofPurpose);
        Assert.Equal("zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq", result.ProofValue);
    }

    [Fact]
    public void Deserialize_WithoutChallengeOrDomain_LeavesThemNull()
    {
        var json = """
            {
                "type": "DataIntegrityProof",
                "cryptosuite": "ecdsa-rdfc-2019",
                "verificationMethod": "https://di.example/holder#key1",
                "proofPurpose": "authentication",
                "proofValue": "zABCDEF"
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, DataIntegrityProofWithChallenge.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Null(result.Challenge);
        Assert.Null(result.Domain);
    }

    [Fact]
    public void Serialize_WithChallengeAndDomain_IncludesBothInJson()
    {
        var proof = new DataIntegrityProofWithChallenge
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Created = "2024-01-11T19:14:04Z",
            Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
            Domain = "domain.example",
            VerificationMethod = "https://di.example/holder#key1",
            ProofPurpose = "authentication",
            ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
        };

        var json = KiotaJsonHelper.Serialize(proof);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("ce2e12b0-35a0-11f0-85df-7bcb79038e44", root.GetProperty("challenge").GetString());
        Assert.Equal("domain.example", root.GetProperty("domain").GetString());
        Assert.Equal("authentication", root.GetProperty("proofPurpose").GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesAllProperties()
    {
        var original = new DataIntegrityProofWithChallenge
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Created = "2024-01-11T19:14:04Z",
            Challenge = "ce2e12b0-35a0-11f0-85df-7bcb79038e44",
            Domain = "domain.example",
            VerificationMethod = "https://di.example/holder#key1",
            ProofPurpose = "authentication",
            ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, DataIntegrityProofWithChallenge.CreateFromDiscriminatorValue);

        Assert.Equal(original.Type, roundTripped.Type);
        Assert.Equal(original.Cryptosuite, roundTripped.Cryptosuite);
        Assert.Equal(original.Created, roundTripped.Created);
        Assert.Equal(original.Challenge, roundTripped.Challenge);
        Assert.Equal(original.Domain, roundTripped.Domain);
        Assert.Equal(original.VerificationMethod, roundTripped.VerificationMethod);
        Assert.Equal(original.ProofPurpose, roundTripped.ProofPurpose);
        Assert.Equal(original.ProofValue, roundTripped.ProofValue);
    }
}
