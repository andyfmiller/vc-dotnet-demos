using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialTermsOfUseTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialTermsOfUseTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Single TermsOfUse Tests

    [Fact]
    public void TermsOfUse_SingleTerm_DeserializesFromSpec()
    {
        // Arrange - Example from spec with single terms of use
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/undefined-terms/v2""
            ],
            ""id"": ""urn:uuid:08e26d22-8dca-4558-9c14-6e7aa7275b9b"",
            ""type"": [
                ""VerifiableCredential"",
                ""VerifiableAttestation""
            ],
            ""issuer"": ""did:ebsi:zZeKyEJfUTGwajhNyNX928z"",
            ""validFrom"": ""2021-11-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:ebsi:zvHWX359A3CvfJnCYaAiAde""
            },
            ""termsOfUse"": {
                ""type"": ""TrustFrameworkPolicy"",
                ""trustFramework"": ""Employment&Life"",
                ""policyId"": ""https://policy.example/policies/125"",
                ""legalBasis"": ""professional qualifications directive""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Single(credential.TermsOfUse);
        
        var term = credential.TermsOfUse.First();
        Assert.Equal("TrustFrameworkPolicy", term.Type);
        Assert.True(term.AdditionalProperties.ContainsKey("trustFramework"));
        Assert.Equal("Employment&Life", term.AdditionalProperties["trustFramework"]?.ToString());
        Assert.True(term.AdditionalProperties.ContainsKey("policyId"));
        Assert.Equal("https://policy.example/policies/125", term.AdditionalProperties["policyId"]?.ToString());
        Assert.True(term.AdditionalProperties.ContainsKey("legalBasis"));
        Assert.Equal("professional qualifications directive", term.AdditionalProperties["legalBasis"]?.ToString());
    }

    [Fact]
    public void TermsOfUse_SingleTerm_WithIdAndType_Deserializes()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""termsOfUse"": {
                ""id"": ""https://example.com/terms/1"",
                ""type"": ""IssuerPolicy""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Single(credential.TermsOfUse);
        
        var term = credential.TermsOfUse.First();
        Assert.Equal("https://example.com/terms/1", term.Id);
        Assert.Equal("IssuerPolicy", term.Type);
    }

    [Fact]
    public void TermsOfUse_SingleTerm_SerializesAsObject()
    {
        // Arrange
        var termsOfUse = new TermsOfUse
        {
            Id = "https://example.com/terms/1",
            Type = "TrustFrameworkPolicy"
        };
        termsOfUse.AdditionalProperties["trustFramework"] = "Employment&Life";
        termsOfUse.AdditionalProperties["policyId"] = "https://policy.example/policies/125";

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            TermsOfUse = new Collection<TermsOfUse> { termsOfUse }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"termsOfUse\": {", json);
        Assert.DoesNotContain("\"termsOfUse\": [", json);
        Assert.Contains("\"type\": \"TrustFrameworkPolicy\"", json);
        Assert.Contains("\"trustFramework\"", json);
        Assert.Contains("\"policyId\"", json);
    }

    [Fact]
    public void TermsOfUse_SingleTerm_RoundTrip_PreservesData()
    {
        // Arrange
        var originalTerm = new TermsOfUse
        {
            Id = "https://policy.example/policies/125",
            Type = "TrustFrameworkPolicy"
        };
        originalTerm.AdditionalProperties["trustFramework"] = "Employment&Life";
        originalTerm.AdditionalProperties["legalBasis"] = "professional qualifications directive";

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2021, 11, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            TermsOfUse = new Collection<TermsOfUse> { originalTerm }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.TermsOfUse);
        Assert.Single(deserialized.TermsOfUse);
        Assert.Equal(original.TermsOfUse.First().Id, deserialized.TermsOfUse.First().Id);
        Assert.Equal(original.TermsOfUse.First().Type, deserialized.TermsOfUse.First().Type);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Multiple TermsOfUse Tests

    [Fact]
    public void TermsOfUse_MultipleTerms_Deserializes()
    {
        // Arrange - Multiple terms of use as array
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""termsOfUse"": [{
                ""type"": ""IssuerPolicy"",
                ""id"": ""https://example.com/policies/issuer""
            }, {
                ""type"": ""HolderPolicy"",
                ""id"": ""https://example.com/policies/holder""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Equal(2, credential.TermsOfUse.Count);
        
        var terms = credential.TermsOfUse.ToArray();
        
        // First term
        Assert.Equal("https://example.com/policies/issuer", terms[0].Id);
        Assert.Equal("IssuerPolicy", terms[0].Type);
        
        // Second term
        Assert.Equal("https://example.com/policies/holder", terms[1].Id);
        Assert.Equal("HolderPolicy", terms[1].Type);
    }

    [Fact]
    public void TermsOfUse_MultipleTerms_SerializesAsArray()
    {
        // Arrange
        var term1 = new TermsOfUse
        {
            Id = "https://example.com/policies/issuer",
            Type = "IssuerPolicy"
        };
        
        var term2 = new TermsOfUse
        {
            Id = "https://example.com/policies/holder",
            Type = "HolderPolicy"
        };

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            TermsOfUse = new Collection<TermsOfUse> { term1, term2 }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"termsOfUse\": [", json);
        Assert.Contains("\"https://example.com/policies/issuer\"", json);
        Assert.Contains("\"https://example.com/policies/holder\"", json);
    }

    [Fact]
    public void TermsOfUse_MultipleTerms_RoundTrip()
    {
        // Arrange
        var term1 = new TermsOfUse 
        { 
            Id = "https://example.com/terms/1", 
            Type = "Type1" 
        };
        var term2 = new TermsOfUse 
        { 
            Id = "https://example.com/terms/2", 
            Type = "Type2" 
        };

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            TermsOfUse = new Collection<TermsOfUse> { term1, term2 }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.TermsOfUse);
        Assert.Equal(2, deserialized.TermsOfUse.Count);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Missing/Null TermsOfUse Tests

    [Fact]
    public void TermsOfUse_MissingInJson_IsNull()
    {
        // Arrange - termsOfUse is optional per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Null(credential.TermsOfUse); // Optional property should be null when missing
    }

    [Fact]
    public void TermsOfUse_NullInJson_DeserializesToNull()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""termsOfUse"": null
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Null(credential.TermsOfUse); // Null should deserialize to null
    }

    [Fact]
    public void TermsOfUse_EmptyCollection_SerializesAsOmitted()
    {
        // Arrange - Empty collection should serialize to null and be omitted
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            TermsOfUse = new Collection<TermsOfUse>() // Empty collection
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Converter writes null for empty collections, then JsonIgnore omits it
        Assert.DoesNotContain("\"termsOfUse\"", json);
    }

    [Fact]
    public void TermsOfUse_NullProperty_SerializesAsOmitted()
    {
        // Arrange - Null property should be omitted
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            TermsOfUse = null // Explicitly null
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Null should be omitted by JsonIgnore
        Assert.DoesNotContain("\"termsOfUse\"", json);
    }

    #endregion

    #region Type Property Tests

    [Fact]
    public void TermsOfUse_WithRequiredType_Deserializes()
    {
        // Arrange - Type is required per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""termsOfUse"": {
                ""type"": ""TrustFrameworkPolicy""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Single(credential.TermsOfUse);
        Assert.Equal("TrustFrameworkPolicy", credential.TermsOfUse.First().Type);
    }

    [Fact]
    public void TermsOfUse_WithoutType_FailsValidation()
    {
        // Arrange - Per spec: type is REQUIRED
        var termsOfUse = new TermsOfUse
        {
            Id = "https://example.com/terms/1",
            Type = null! // Missing required type
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(termsOfUse);
        var isValid = Validator.TryValidateObject(
            termsOfUse, validationContext, validationResults, validateAllProperties: true);

        // Assert - Type is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Type"));
    }

    [Theory]
    [InlineData("TrustFrameworkPolicy")]
    [InlineData("IssuerPolicy")]
    [InlineData("HolderPolicy")]
    [InlineData("CustomPolicyType")]
    public void TermsOfUse_WithVariousTypes_Deserializes(string policyType)
    {
        // Arrange
        var json = $@"{{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {{
                ""id"": ""did:example:123""
            }},
            ""termsOfUse"": {{
                ""type"": ""{policyType}""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Single(credential.TermsOfUse);
        Assert.Equal(policyType, credential.TermsOfUse.First().Type);
    }

    #endregion

    #region ID Property Tests

    [Fact]
    public void TermsOfUse_WithOptionalId_Deserializes()
    {
        // Arrange - Id is optional (MAY specify)
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""termsOfUse"": {
                ""type"": ""TrustFrameworkPolicy"",
                ""id"": ""https://policy.example/policies/125""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Single(credential.TermsOfUse);
        Assert.Equal("https://policy.example/policies/125", credential.TermsOfUse.First().Id);
    }

    [Fact]
    public void TermsOfUse_WithoutId_IsValid()
    {
        // Arrange - Per spec: id is OPTIONAL (MAY specify)
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""termsOfUse"": {
                ""type"": ""TrustFrameworkPolicy""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Single(credential.TermsOfUse);
        Assert.Null(credential.TermsOfUse.First().Id);
        Assert.Equal("TrustFrameworkPolicy", credential.TermsOfUse.First().Type);
    }

    [Theory]
    [InlineData("https://policy.example/policies/125")]
    [InlineData("https://example.com/terms/issuer-policy")]
    [InlineData("urn:uuid:3d4f3e8a-9c7b-4f2e-8d1a-5e6f7a8b9c0d")]
    public void TermsOfUse_WithVariousIdFormats_Deserializes(string termsId)
    {
        // Arrange
        var json = $@"{{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {{
                ""id"": ""did:example:123""
            }},
            ""termsOfUse"": {{
                ""id"": ""{termsId}"",
                ""type"": ""TrustFrameworkPolicy""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.TermsOfUse);
        Assert.Single(credential.TermsOfUse);
        Assert.Equal(termsId, credential.TermsOfUse.First().Id);
    }

    #endregion

    #region Additional Properties Tests

    [Fact]
    public void TermsOfUse_WithAdditionalProperties_PreservesData()
    {
        // Arrange
        var term = new TermsOfUse
        {
            Id = "https://policy.example/policies/125",
            Type = "TrustFrameworkPolicy"
        };
        term.AdditionalProperties["trustFramework"] = "Employment&Life";
        term.AdditionalProperties["policyId"] = "https://policy.example/policies/125";
        term.AdditionalProperties["legalBasis"] = "professional qualifications directive";
        term.AdditionalProperties["jurisdiction"] = "EU";

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            TermsOfUse = new Collection<TermsOfUse> { term }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.TermsOfUse);
        var deserializedTerm = deserialized.TermsOfUse.First();
        
        Assert.True(deserializedTerm.AdditionalProperties.ContainsKey("trustFramework"));
        Assert.Equal("Employment&Life", deserializedTerm.AdditionalProperties["trustFramework"]?.ToString());
        
        Assert.True(deserializedTerm.AdditionalProperties.ContainsKey("policyId"));
        Assert.Equal("https://policy.example/policies/125", deserializedTerm.AdditionalProperties["policyId"]?.ToString());
        
        Assert.True(deserializedTerm.AdditionalProperties.ContainsKey("legalBasis"));
        Assert.Equal("professional qualifications directive", deserializedTerm.AdditionalProperties["legalBasis"]?.ToString());
        
        Assert.True(deserializedTerm.AdditionalProperties.ContainsKey("jurisdiction"));
        Assert.Equal("EU", deserializedTerm.AdditionalProperties["jurisdiction"]?.ToString());
    }

    #endregion
}