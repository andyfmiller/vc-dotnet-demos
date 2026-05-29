using Library.Vcalm.Client.Models;
using System.Text.Json;

namespace Library.Tests.Vcalm;

public class IssueCredentialOptionsTests
{
    private readonly ITestOutputHelper _output;

    public IssueCredentialOptionsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_LeavesAllProperties_AsNull()
    {
        var options = new IssueCredentialOptions();

        Assert.Null(options.CredentialId);
        Assert.Null(options.MandatoryPointers);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var options = new IssueCredentialOptions
        {
            CredentialId = "http://example.gov/credentials/3732",
            MandatoryPointers = ["/credentialSubject/id", "/credentialSubject/name"]
        };

        Assert.Equal("http://example.gov/credentials/3732", options.CredentialId);
        Assert.NotNull(options.MandatoryPointers);
        Assert.Equal(2, options.MandatoryPointers.Count);
        Assert.Contains("/credentialSubject/id", options.MandatoryPointers);
        Assert.Contains("/credentialSubject/name", options.MandatoryPointers);
    }

    [Fact]
    public void Deserialize_WithCredentialId_ReadsCredentialId()
    {
        var json = """
            {
                "credentialId": "http://example.gov/credentials/3732"
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, IssueCredentialOptions.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("http://example.gov/credentials/3732", result.CredentialId);
        Assert.Null(result.MandatoryPointers);
    }

    [Fact]
    public void Deserialize_WithMandatoryPointers_ReadsMandatoryPointers()
    {
        // mandatoryPointers are used with selective disclosure schemes (e.g., bbs-2023, ecdsa-sd-2023)
        var json = """
            {
                "mandatoryPointers": ["/credentialSubject/id", "/credentialSubject/name"]
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, IssueCredentialOptions.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Null(result.CredentialId);
        Assert.NotNull(result.MandatoryPointers);
        Assert.Equal(2, result.MandatoryPointers.Count);
        Assert.Contains("/credentialSubject/id", result.MandatoryPointers);
        Assert.Contains("/credentialSubject/name", result.MandatoryPointers);
    }

    [Fact]
    public void Deserialize_WithAllProperties_ReadsAllProperties()
    {
        var json = """
            {
                "credentialId": "http://example.gov/credentials/3732",
                "mandatoryPointers": ["/credentialSubject/id"]
            }
            """;

        var result = KiotaJsonHelper.Deserialize(json, IssueCredentialOptions.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Equal("http://example.gov/credentials/3732", result.CredentialId);
        Assert.NotNull(result.MandatoryPointers);
        Assert.Single(result.MandatoryPointers);
        Assert.Equal("/credentialSubject/id", result.MandatoryPointers[0]);
    }

    [Fact]
    public void Deserialize_WithEmptyObject_LeavesAllPropertiesNull()
    {
        var json = "{}";

        var result = KiotaJsonHelper.Deserialize(json, IssueCredentialOptions.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Null(result.CredentialId);
        Assert.Null(result.MandatoryPointers);
    }

    [Fact]
    public void Serialize_WithCredentialId_ProducesCorrectJson()
    {
        var options = new IssueCredentialOptions
        {
            CredentialId = "http://example.gov/credentials/3732"
        };

        var json = KiotaJsonHelper.Serialize(options);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("http://example.gov/credentials/3732", root.GetProperty("credentialId").GetString());
    }

    [Fact]
    public void Serialize_WithMandatoryPointers_ProducesCorrectJson()
    {
        var options = new IssueCredentialOptions
        {
            MandatoryPointers = ["/credentialSubject/id", "/credentialSubject/name"]
        };

        var json = KiotaJsonHelper.Serialize(options);
        _output.WriteLine(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var pointers = root.GetProperty("mandatoryPointers");
        Assert.Equal(JsonValueKind.Array, pointers.ValueKind);
        Assert.Equal(2, pointers.GetArrayLength());
        Assert.Equal("/credentialSubject/id", pointers[0].GetString());
        Assert.Equal("/credentialSubject/name", pointers[1].GetString());
    }

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesAllProperties()
    {
        var original = new IssueCredentialOptions
        {
            CredentialId = "http://example.gov/credentials/3732",
            MandatoryPointers = ["/credentialSubject/id", "/credentialSubject/name"]
        };

        var json = KiotaJsonHelper.Serialize(original);
        var roundTripped = KiotaJsonHelper.Deserialize(json, IssueCredentialOptions.CreateFromDiscriminatorValue);

        Assert.Equal(original.CredentialId, roundTripped.CredentialId);
        Assert.NotNull(roundTripped.MandatoryPointers);
        Assert.Equal(original.MandatoryPointers.Count, roundTripped.MandatoryPointers.Count);
        Assert.Contains("/credentialSubject/id", roundTripped.MandatoryPointers);
        Assert.Contains("/credentialSubject/name", roundTripped.MandatoryPointers);
    }
}
