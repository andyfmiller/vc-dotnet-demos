using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class IssueCredentialRequestTests
{
    private readonly ITestOutputHelper _output;

    public IssueCredentialRequestTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitializesAdditionalData_AsEmptyDictionary()
    {
        var request = new IssueCredentialRequest();

        Assert.NotNull(request.AdditionalData);
        Assert.Empty(request.AdditionalData);
    }

    [Fact]
    public void Constructor_LeavesOptionalProperties_AsNull()
    {
        var request = new IssueCredentialRequest();

        Assert.Null(request.Credential);
        Assert.Null(request.Options);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var unsecured = new UnsecuredCredential
        {
            Id = "http://example.gov/credentials/3732",
            Type = ["VerifiableCredential"],
            Context = ["https://www.w3.org/ns/credentials/v2"]
        };
        var options = new IssueCredentialOptions
        {
            CredentialId = "http://example.gov/credentials/3732"
        };
        var request = new IssueCredentialRequest
        {
            Credential = unsecured,
            Options = options
        };

        Assert.NotNull(request.Credential);
        Assert.Equal("http://example.gov/credentials/3732", request.Credential.Id);
        Assert.NotNull(request.Options);
        Assert.Equal("http://example.gov/credentials/3732", request.Options.CredentialId);
    }

    [Fact]
    public void Deserialize_WithCredentialAndOptions_ReadsAllProperties()
    {
        var json = """
            {
                "credential": {
                    "@context": [
                        "https://www.w3.org/ns/credentials/v2",
                        "https://www.w3.org/ns/credentials/examples/v2"
                    ],
                    "id": "http://example.gov/credentials/3732",
                    "type": ["VerifiableCredential", "UniversityDegreeCredential"],
                    "issuer": "did:example:123",
                    "validFrom": "2020-03-16T22:37:26.544Z",
                    "credentialSubject": {
                        "id": "did:example:123",
                        "degree": {
                            "type": "BachelorDegree",
                            "name": "Bachelor of Science and Arts"
                        }
                    }
                },
                "options": {
                    "credentialId": "http://example.gov/credentials/3732"
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, IssueCredentialRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Credential);
        Assert.Equal("http://example.gov/credentials/3732", result.Credential.Id);
        Assert.NotNull(result.Credential.Context);
        Assert.Equal(2, result.Credential.Context.Count);
        Assert.NotNull(result.Credential.Type);
        Assert.Contains("UniversityDegreeCredential", result.Credential.Type);
        Assert.Equal("2020-03-16T22:37:26.544Z", result.Credential.ValidFrom);
        Assert.NotNull(result.Options);
        Assert.Equal("http://example.gov/credentials/3732", result.Options.CredentialId);
    }

    [Fact]
    public void Deserialize_WithoutId_CredentialIdIsNull()
    {
        // Per spec: "The issuer SHOULD NOT auto-generate the id property if not provided"
        var json = """
            {
                "credential": {
                    "@context": ["https://www.w3.org/ns/credentials/v2"],
                    "type": ["VerifiableCredential"],
                    "issuer": "did:example:123",
                    "credentialSubject": { "id": "did:example:456" }
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, IssueCredentialRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Credential);
        Assert.Null(result.Credential.Id);
        Assert.Null(result.Options);
    }

    [Fact]
    public void IssueCredentialOptions_CredentialId_CanBeSet()
    {
        var options = new IssueCredentialOptions
        {
            CredentialId = "http://example.gov/credentials/3732"
        };

        Assert.Equal("http://example.gov/credentials/3732", options.CredentialId);
    }

    [Fact]
    public void IssueCredentialOptions_MandatoryPointers_CanBeSet()
    {
        var options = new IssueCredentialOptions
        {
            MandatoryPointers = ["/issuer", "/validFrom"]
        };

        Assert.NotNull(options.MandatoryPointers);
        Assert.Equal(2, options.MandatoryPointers.Count);
        Assert.Contains("/issuer", options.MandatoryPointers);
        Assert.Contains("/validFrom", options.MandatoryPointers);
    }

    [Fact]
    public void Deserialize_OptionsWithMandatoryPointers_ReadsMandatoryPointers()
    {
        var json = """
            {
                "credential": {
                    "@context": ["https://www.w3.org/ns/credentials/v2"],
                    "type": ["VerifiableCredential"],
                    "issuer": "did:example:123",
                    "credentialSubject": { "id": "did:example:456" }
                },
                "options": {
                    "mandatoryPointers": ["/issuer", "/validFrom"]
                }
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, IssueCredentialRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.NotNull(result.Options);
        Assert.NotNull(result.Options.MandatoryPointers);
        Assert.Equal(2, result.Options.MandatoryPointers.Count);
        Assert.Contains("/issuer", result.Options.MandatoryPointers);
        Assert.Contains("/validFrom", result.Options.MandatoryPointers);
    }

    [Fact]
    public void Serialize_WithCredential_IncludesCredentialInJson()
    {
        var request = new IssueCredentialRequest
        {
            Credential = new UnsecuredCredential
            {
                Context = ["https://www.w3.org/ns/credentials/v2"],
                Type = ["VerifiableCredential"],
                Id = "http://example.gov/credentials/3732",
                ValidFrom = "2020-03-16T22:37:26.544Z"
            }
        };

        var json = KiotaJsonHelper.Serialize(request);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("credential", out var credProp));
        Assert.Equal("http://example.gov/credentials/3732", credProp.GetProperty("id").GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesCredentialAndOptions()
    {
        var original = new IssueCredentialRequest
        {
            Credential = new UnsecuredCredential
            {
                Context = ["https://www.w3.org/ns/credentials/v2"],
                Type = ["VerifiableCredential"],
                Id = "http://example.gov/credentials/3732",
                ValidFrom = "2020-03-16T22:37:26.544Z"
            },
            Options = new IssueCredentialOptions
            {
                CredentialId = "http://example.gov/credentials/3732"
            }
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, IssueCredentialRequest.CreateFromDiscriminatorValue);

        Assert.NotNull(roundTripped.Credential);
        Assert.Equal(original.Credential.Id, roundTripped.Credential.Id);
        Assert.Equal(original.Credential.ValidFrom, roundTripped.Credential.ValidFrom);
        Assert.NotNull(roundTripped.Options);
        Assert.Equal(original.Options.CredentialId, roundTripped.Options.CredentialId);
    }
}
