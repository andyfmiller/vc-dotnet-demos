using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialEvidenceTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialEvidenceTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Single Evidence Tests

    [Fact]
    public void Evidence_SingleItem_DeserializesFromSpec()
    {
        // Arrange - Example from spec with single evidence (note: spec shows as array, but we deserialize both)
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json""
            ],
            ""id"": ""http://1edtech.edu/credentials/3732"",
            ""type"": [""VerifiableCredential"", ""OpenBadgeCredential""],
            ""issuer"": {
                ""id"": ""https://1edtech.edu/issuers/565049"",
                ""type"": ""Profile""
            },
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21""
            },
            ""evidence"": [{
                ""id"": ""https://videos.example/training/alice-espresso.mp4"",
                ""type"": [""Evidence""],
                ""name"": ""Talk-aloud video of double espresso preparation"",
                ""description"": ""This is a talk-aloud video of Alice demonstrating preparation of a double espresso drink."",
                ""digestMultibase"": ""uELq9FnJ5YLa5iAszyJ518bXcnlc5P7xp1u-5uJRDYKvc""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Single(credential.Evidence);
        
        var evidence = credential.Evidence.First();
        Assert.Equal("https://videos.example/training/alice-espresso.mp4", evidence.Id);
        Assert.Contains("Evidence", evidence.Type);
        Assert.Equal("Talk-aloud video of double espresso preparation", evidence.Name);
        Assert.Equal("This is a talk-aloud video of Alice demonstrating preparation of a double espresso drink.", evidence.Description);
        Assert.True(evidence.AdditionalProperties.ContainsKey("digestMultibase"));
        Assert.Equal("uELq9FnJ5YLa5iAszyJ518bXcnlc5P7xp1u-5uJRDYKvc", evidence.AdditionalProperties["digestMultibase"]?.ToString());
    }

    [Fact]
    public void Evidence_SingleItem_AsObject_Deserializes()
    {
        // Arrange - Single evidence as object (not array)
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""evidence"": {
                ""id"": ""https://example.com/evidence/1"",
                ""type"": [""Evidence""]
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Single(credential.Evidence);
        
        var evidence = credential.Evidence.First();
        Assert.Equal("https://example.com/evidence/1", evidence.Id);
        Assert.Contains("Evidence", evidence.Type);
    }

    [Fact]
    public void Evidence_SingleItem_SerializesAsObject()
    {
        // Arrange - Single item should serialize as object, not array
        var evidence = new Evidence
        {
            Id = "https://videos.example/training/video.mp4",
            Type = new Collection<string> { "Evidence" },
            Name = "Training video",
            Description = "Video evidence of training completion"
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
            Evidence = new Collection<Evidence> { evidence }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"evidence\": {", json);
        Assert.DoesNotContain("\"evidence\": [", json);
        Assert.Contains("\"type\": \"Evidence\"", json);
        Assert.Contains("\"https://videos.example/training/video.mp4\"", json);
        Assert.Contains("\"name\"", json);
    }

    [Fact]
    public void Evidence_SingleItem_RoundTrip_PreservesData()
    {
        // Arrange
        var originalEvidence = new Evidence
        {
            Id = "https://videos.example/training/alice-espresso.mp4",
            Type = new Collection<string> { "Evidence" },
            Name = "Talk-aloud video of double espresso preparation",
            Description = "This is a talk-aloud video of Alice demonstrating preparation of a double espresso drink."
        };
        originalEvidence.AdditionalProperties["digestMultibase"] = "uELq9FnJ5YLa5iAszyJ518bXcnlc5P7xp1u-5uJRDYKvc";

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            Evidence = new Collection<Evidence> { originalEvidence }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Evidence);
        Assert.Single(deserialized.Evidence);
        Assert.Equal(original.Evidence.First().Id, deserialized.Evidence.First().Id);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Multiple Evidence Tests

    [Fact]
    public void Evidence_MultipleItems_Deserializes()
    {
        // Arrange - Multiple evidence items as array
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""evidence"": [{
                ""id"": ""https://example.com/evidence/document1"",
                ""type"": [""DocumentEvidence""],
                ""name"": ""Passport verification""
            }, {
                ""id"": ""https://example.com/evidence/video1"",
                ""type"": [""VideoEvidence""],
                ""name"": ""Identity verification video""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Equal(2, credential.Evidence.Count);
        
        var items = credential.Evidence.ToArray();
        
        // First evidence
        Assert.Equal("https://example.com/evidence/document1", items[0].Id);
        Assert.Contains("DocumentEvidence", items[0].Type);
        
        // Second evidence
        Assert.Equal("https://example.com/evidence/video1", items[1].Id);
        Assert.Contains("VideoEvidence", items[1].Type);
    }

    [Fact]
    public void Evidence_MultipleItems_SerializesAsArray()
    {
        // Arrange
        var evidence1 = new Evidence
        {
            Id = "https://example.com/evidence/document1",
            Type = new Collection<string> { "DocumentEvidence" }
        };
        
        var evidence2 = new Evidence
        {
            Id = "https://example.com/evidence/video1",
            Type = new Collection<string> { "VideoEvidence" }
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
            Evidence = new Collection<Evidence> { evidence1, evidence2 }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"evidence\": [", json);
        Assert.Contains("\"https://example.com/evidence/document1\"", json);
        Assert.Contains("\"https://example.com/evidence/video1\"", json);
    }

    [Fact]
    public void Evidence_MultipleItems_RoundTrip()
    {
        // Arrange
        var evidence1 = new Evidence 
        { 
            Id = "https://example.com/evidence/1", 
            Type = new Collection<string> { "Type1" } 
        };
        var evidence2 = new Evidence 
        { 
            Id = "https://example.com/evidence/2", 
            Type = new Collection<string> { "Type2" } 
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
            Evidence = new Collection<Evidence> { evidence1, evidence2 }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Evidence);
        Assert.Equal(2, deserialized.Evidence.Count);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Missing/Null Evidence Tests

    [Fact]
    public void Evidence_MissingInJson_IsNull()
    {
        // Arrange - evidence is optional per spec
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
        Assert.Null(credential.Evidence); // Optional property should be null when missing
    }

    [Fact]
    public void Evidence_NullInJson_DeserializesToNull()
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
            ""evidence"": null
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Null(credential.Evidence); // Null should deserialize to null
    }

    [Fact]
    public void Evidence_EmptyCollection_SerializesAsOmitted()
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
            Evidence = new Collection<Evidence>() // Empty collection
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Converter writes null for empty collections, then JsonIgnore omits it
        Assert.DoesNotContain("\"evidence\"", json);
    }

    [Fact]
    public void Evidence_NullProperty_SerializesAsOmitted()
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
            Evidence = null // Explicitly null
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Null should be omitted by JsonIgnore
        Assert.DoesNotContain("\"evidence\"", json);
    }

    #endregion

    #region Type Property Tests

    [Fact]
    public void Evidence_WithRequiredType_Deserializes()
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
            ""evidence"": {
                ""type"": [""Evidence""]
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Single(credential.Evidence);
        Assert.Contains("Evidence", credential.Evidence.First().Type);
    }

    [Fact]
    public void Evidence_WithoutType_FailsValidation()
    {
        // Arrange - Per spec: type is REQUIRED
        var evidence = new Evidence
        {
            Id = "https://example.com/evidence/1",
            Type = null! // Missing required type
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(evidence);
        var isValid = Validator.TryValidateObject(
            evidence, validationContext, validationResults, validateAllProperties: true);

        // Assert - Type is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Type"));
    }

    [Theory]
    [InlineData("Evidence")]
    [InlineData("DocumentEvidence")]
    [InlineData("VideoEvidence")]
    [InlineData("CustomEvidenceType")]
    public void Evidence_WithVariousTypes_Deserializes(string evidenceType)
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
            ""evidence"": {{
                ""type"": [""{evidenceType}""]
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Single(credential.Evidence);
        Assert.Contains(evidenceType, credential.Evidence.First().Type);
    }

    #endregion

    #region ID Property Tests

    [Fact]
    public void Evidence_WithOptionalId_Deserializes()
    {
        // Arrange - Id is optional (MAY be used)
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""evidence"": {
                ""id"": ""https://videos.example/training/video.mp4"",
                ""type"": [""Evidence""]
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Single(credential.Evidence);
        Assert.Equal("https://videos.example/training/video.mp4", credential.Evidence.First().Id);
    }

    [Fact]
    public void Evidence_WithoutId_IsValid()
    {
        // Arrange - Per spec: id is OPTIONAL
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""evidence"": {
                ""type"": [""Evidence""]
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Single(credential.Evidence);
        Assert.Null(credential.Evidence.First().Id);
        Assert.Contains("Evidence", credential.Evidence.First().Type);
    }

    [Theory]
    [InlineData("https://videos.example/training/alice-espresso.mp4")]
    [InlineData("https://example.com/documents/passport-scan.pdf")]
    [InlineData("urn:uuid:3d4f3e8a-9c7b-4f2e-8d1a-5e6f7a8b9c0d")]
    public void Evidence_WithVariousIdFormats_Deserializes(string evidenceId)
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
            ""evidence"": {{
                ""id"": ""{evidenceId}"",
                ""type"": [""Evidence""]
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Evidence);
        Assert.Single(credential.Evidence);
        Assert.Equal(evidenceId, credential.Evidence.First().Id);
    }

    #endregion

    #region Additional Properties Tests

    [Fact]
    public void Evidence_WithAdditionalProperties_PreservesData()
    {
        // Arrange
        var evidence = new Evidence
        {
            Id = "https://videos.example/training/alice-espresso.mp4",
            Type = new Collection<string> { "Evidence" },
            Name = "Talk-aloud video of double espresso preparation",
            Description = "This is a talk-aloud video of Alice demonstrating preparation of a double espresso drink."
        };
        evidence.AdditionalProperties["digestMultibase"] = "uELq9FnJ5YLa5iAszyJ518bXcnlc5P7xp1u-5uJRDYKvc";
        evidence.AdditionalProperties["genre"] = "training";

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
            Evidence = new Collection<Evidence> { evidence }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Evidence);
        var deserializedEvidence = deserialized.Evidence.First();

        Assert.NotNull(deserializedEvidence.Name);
        Assert.Equal("Talk-aloud video of double espresso preparation", deserializedEvidence.Name);

        Assert.NotNull(deserializedEvidence.Description);
        Assert.Equal("This is a talk-aloud video of Alice demonstrating preparation of a double espresso drink.", deserializedEvidence.Description);

        Assert.NotNull(deserializedEvidence.Genre);
        Assert.Equal("training", deserializedEvidence.Genre);

        Assert.True(deserializedEvidence.AdditionalProperties.ContainsKey("digestMultibase"));
        Assert.Equal("uELq9FnJ5YLa5iAszyJ518bXcnlc5P7xp1u-5uJRDYKvc", deserializedEvidence.AdditionalProperties["digestMultibase"]?.ToString());
    }

    #endregion
}