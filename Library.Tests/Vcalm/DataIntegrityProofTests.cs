using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class DataIntegrityProofTests
{
    private readonly ITestOutputHelper _output;

    public DataIntegrityProofTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var proof = new DataIntegrityProof();

        Assert.NotNull(proof.AdditionalData);
        Assert.Empty(proof.AdditionalData);
    }

    [Fact]
    public void Constructor_LeavesAllProperties_AsNull()
    {
        var proof = new DataIntegrityProof();

        Assert.Null(proof.Type);
        Assert.Null(proof.Cryptosuite);
        Assert.Null(proof.Created);
        Assert.Null(proof.Nonce);
        Assert.Null(proof.VerificationMethod);
        Assert.Null(proof.ProofPurpose);
        Assert.Null(proof.ProofValue);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var proof = new DataIntegrityProof
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Created = "2024-01-11T19:14:04Z",
            VerificationMethod = "https://di.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP",
            ProofPurpose = "assertionMethod",
            ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
        };

        Assert.Equal("DataIntegrityProof", proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", proof.Cryptosuite);
        Assert.Equal("2024-01-11T19:14:04Z", proof.Created);
        Assert.Equal("https://di.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP", proof.VerificationMethod);
        Assert.Equal("assertionMethod", proof.ProofPurpose);
        Assert.Equal("zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq", proof.ProofValue);
    }

    [Fact]
    public void Deserialize_FromSpecExample_ReadsAllProperties()
    {
        // Spec example from DataIntegrityProof.yml
        var json = """
            {
                "type": "DataIntegrityProof",
                "cryptosuite": "ecdsa-rdfc-2019",
                "created": "2024-01-11T19:14:04Z",
                "verificationMethod": "https://di.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP",
                "proofPurpose": "assertionMethod",
                "proofValue": "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, DataIntegrityProof.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("DataIntegrityProof", result.Type);
        Assert.Equal("ecdsa-rdfc-2019", result.Cryptosuite);
        Assert.Equal("2024-01-11T19:14:04Z", result.Created);
        Assert.Equal("https://di.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP", result.VerificationMethod);
        Assert.Equal("assertionMethod", result.ProofPurpose);
        Assert.Equal("zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq", result.ProofValue);
    }

    [Fact]
    public void Deserialize_WithOptionalNonce_ReadsNonce()
    {
        var json = """
            {
                "type": "DataIntegrityProof",
                "cryptosuite": "ecdsa-rdfc-2019",
                "created": "2024-01-11T19:14:04Z",
                "nonce": "randomNonce123",
                "verificationMethod": "https://di.example/issuer#key1",
                "proofPurpose": "assertionMethod",
                "proofValue": "zABCDEF"
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, DataIntegrityProof.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("randomNonce123", result.Nonce);
    }

    [Fact]
    public void Deserialize_WithoutNonce_LeavesNonceNull()
    {
        var json = """
            {
                "type": "DataIntegrityProof",
                "cryptosuite": "ecdsa-rdfc-2019",
                "created": "2024-01-11T19:14:04Z",
                "verificationMethod": "https://di.example/issuer#key1",
                "proofPurpose": "assertionMethod",
                "proofValue": "zABCDEF"
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, DataIntegrityProof.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Null(result.Nonce);
    }

    [Fact]
    public void Serialize_WithAllProperties_ProducesCorrectJson()
    {
        var proof = new DataIntegrityProof
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Created = "2024-01-11T19:14:04Z",
            VerificationMethod = "https://di.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP",
            ProofPurpose = "assertionMethod",
            ProofValue = "zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq"
        };

        var json = KiotaJsonHelper.Serialize(proof);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("DataIntegrityProof", root.GetProperty("type").GetString());
        Assert.Equal("ecdsa-rdfc-2019", root.GetProperty("cryptosuite").GetString());
        Assert.Equal("2024-01-11T19:14:04Z", root.GetProperty("created").GetString());
        Assert.Equal("https://di.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP", root.GetProperty("verificationMethod").GetString());
        Assert.Equal("assertionMethod", root.GetProperty("proofPurpose").GetString());
        Assert.Equal("zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq", root.GetProperty("proofValue").GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesAllProperties()
    {
        var original = new DataIntegrityProof
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "eddsa-rdfc-2022",
            Created = "2020-04-02T18:28:08Z",
            VerificationMethod = "did:example:123#z6MksHh7qHWvybLg5QTPPdG2DgEjjduBDArV9EF9mRiRzMBN",
            ProofPurpose = "assertionMethod",
            ProofValue = "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, DataIntegrityProof.CreateFromDiscriminatorValue);

        Assert.Equal(original.Type, roundTripped.Type);
        Assert.Equal(original.Cryptosuite, roundTripped.Cryptosuite);
        Assert.Equal(original.Created, roundTripped.Created);
        Assert.Equal(original.VerificationMethod, roundTripped.VerificationMethod);
        Assert.Equal(original.ProofPurpose, roundTripped.ProofPurpose);
        Assert.Equal(original.ProofValue, roundTripped.ProofValue);
    }
}
