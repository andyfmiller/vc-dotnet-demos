using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class CredentialTests
{
    private readonly ITestOutputHelper _output;

    public CredentialTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var credential = new Credential();

        Assert.NotNull(credential.AdditionalData);
        Assert.Empty(credential.AdditionalData);
    }

    [Fact]
    public void Constructor_LeavesOptionalProperties_AsNull()
    {
        var credential = new Credential();

        Assert.Null(credential.Context);
        Assert.Null(credential.Id);
        Assert.Null(credential.Type);
        Assert.Null(credential.Issuer);
        Assert.Null(credential.IssuanceDate);
        Assert.Null(credential.ExpirationDate);
        Assert.Null(credential.CredentialSubject);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var credential = new Credential
        {
            Id = "http://example.gov/credentials/3732",
            IssuanceDate = "2020-03-16T22:37:26.544Z",
            ExpirationDate = "2030-03-16T22:37:26.544Z",
            Context = ["https://www.w3.org/ns/credentials/v2", "https://www.w3.org/ns/credentials/examples/v2"],
            Type = ["VerifiableCredential", "UniversityDegreeCredential"]
        };

        Assert.Equal("http://example.gov/credentials/3732", credential.Id);
        Assert.Equal("2020-03-16T22:37:26.544Z", credential.IssuanceDate);
        Assert.Equal("2030-03-16T22:37:26.544Z", credential.ExpirationDate);
        Assert.Equal(2, credential.Context!.Count);
        Assert.Contains("https://www.w3.org/ns/credentials/v2", credential.Context);
        Assert.Equal(2, credential.Type!.Count);
        Assert.Contains("VerifiableCredential", credential.Type);
    }

    [Fact]
    public void Deserialize_FromSpecExample_ReadsContextAndType()
    {
        // Spec example from Credential.yml
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
                    "id": "did:example:123"
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, Credential.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("http://example.gov/credentials/3732", result.Id);
        Assert.Equal("2020-03-16T22:37:26.544Z", result.IssuanceDate);
        Assert.NotNull(result.Context);
        Assert.Equal(2, result.Context.Count);
        Assert.Contains("https://www.w3.org/ns/credentials/v2", result.Context);
        Assert.NotNull(result.Type);
        Assert.Equal(2, result.Type.Count);
        Assert.Contains("VerifiableCredential", result.Type);
        Assert.Contains("UniversityDegreeCredential", result.Type);
    }

    [Fact]
    public void Deserialize_WithStringIssuer_ReadsIssuerAsString()
    {
        var json = """
            {
                "@context": ["https://www.w3.org/ns/credentials/v2"],
                "type": ["VerifiableCredential"],
                "issuer": "did:example:123",
                "credentialSubject": { "id": "did:example:456" }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, Credential.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Issuer);
        Assert.Equal("did:example:123", result.Issuer.String);
    }

    [Fact]
    public void Deserialize_WithObjectIssuer_IssuerInstanceIsCreated()
    {
        // Note: the Kiota-generated Issuer composed type (oneOf string|object) has a known
        // limitation: without a discriminator property it cannot distinguish between the
        // two variants at deserialization time, so IssuerMember1 remains null.
        // This test verifies the Issuer instance is created even in that case.
        var json = """
            {
                "@context": ["https://www.w3.org/ns/credentials/v2"],
                "type": ["VerifiableCredential"],
                "issuer": { "id": "did:key:z6MkjRagNiMu91DduvCvgEsqLZDVzrJzFrwahc4tXLt9DoHd" },
                "credentialSubject": { "id": "did:example:456" }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, Credential.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Issuer);
    }

    [Fact]
    public void Deserialize_WithExpirationDate_ReadsExpirationDate()
    {
        var json = """
            {
                "@context": ["https://www.w3.org/ns/credentials/v2"],
                "type": ["VerifiableCredential"],
                "issuer": "did:example:123",
                "issuanceDate": "2020-03-16T22:37:26.544Z",
                "expirationDate": "2030-03-16T22:37:26.544Z",
                "credentialSubject": { "id": "did:example:456" }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, Credential.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("2020-03-16T22:37:26.544Z", result.IssuanceDate);
        Assert.Equal("2030-03-16T22:37:26.544Z", result.ExpirationDate);
    }

    [Fact]
    public void Serialize_ContextUsesAtContextKey_InJson()
    {
        var credential = new Credential
        {
            Context = ["https://www.w3.org/ns/credentials/v2"],
            Type = ["VerifiableCredential"],
            Id = "http://example.gov/credentials/3732",
            IssuanceDate = "2020-03-16T22:37:26.544Z"
        };

        var json = KiotaJsonHelper.Serialize(credential);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("@context", out var contextProp));
        Assert.Equal(JsonValueKind.Array, contextProp.ValueKind);
        Assert.Equal("https://www.w3.org/ns/credentials/v2", contextProp[0].GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesContextTypeAndId()
    {
        var original = new Credential
        {
            Context = ["https://www.w3.org/ns/credentials/v2", "https://www.w3.org/ns/credentials/examples/v2"],
            Type = ["VerifiableCredential", "UniversityDegreeCredential"],
            Id = "http://example.gov/credentials/3732",
            IssuanceDate = "2020-03-16T22:37:26.544Z"
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, Credential.CreateFromDiscriminatorValue);

        Assert.Equal(original.Id, roundTripped.Id);
        Assert.Equal(original.IssuanceDate, roundTripped.IssuanceDate);
        Assert.Equal(original.Context!.Count, roundTripped.Context!.Count);
        Assert.Equal(original.Type!.Count, roundTripped.Type!.Count);
    }
}
