using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialSchemaTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialSchemaTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Single Schema Tests

    [Fact]
    public void CredentialSchema_SingleSchema_Deserializes()
    {
        // Arrange - Single schema as object
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""credentialSchema"": {
                ""id"": ""https://example.org/examples/degree.json"",
                ""type"": ""JsonSchema""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSchema);
        Assert.Single(credential.CredentialSchema);
        
        var schema = credential.CredentialSchema.First();
        Assert.Equal("https://example.org/examples/degree.json", schema.Id);
        Assert.Equal("JsonSchema", schema.Type);
    }

    [Fact]
    public void CredentialSchema_SingleSchema_SerializesAsObject()
    {
        // Arrange
        var credentialSchema = new CredentialSchema
        {
            Id = "https://example.org/examples/degree.json",
            Type = "JsonSchema"
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
            CredentialSchema = new Collection<CredentialSchema> { credentialSchema }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialSchema\": {", json);
        Assert.DoesNotContain("\"credentialSchema\": [", json);
        Assert.Contains("\"type\": \"JsonSchema\"", json);
        Assert.Contains("\"id\": \"https://example.org/examples/degree.json\"", json);
    }

    [Fact]
    public void CredentialSchema_SingleSchema_RoundTrip_PreservesData()
    {
        // Arrange
        var originalSchema = new CredentialSchema
        {
            Id = "https://example.org/examples/degree.json",
            Type = "JsonSchema"
        };

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://university.example/issuers/14" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 19, 23, 24, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            CredentialSchema = new Collection<CredentialSchema> { originalSchema }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.CredentialSchema);
        Assert.Single(deserialized.CredentialSchema);
        Assert.Equal(original.CredentialSchema.First().Id, deserialized.CredentialSchema.First().Id);
        Assert.Equal(original.CredentialSchema.First().Type, deserialized.CredentialSchema.First().Type);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Multiple Schema Tests

    [Fact]
    public void CredentialSchema_MultipleSchemas_DeserializesFromSpec()
    {
        // Arrange - Example from spec with multiple schemas
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/examples/v2""
            ],
            ""id"": ""http://university.example/credentials/3732"",
            ""type"": [""VerifiableCredential"", ""ExampleDegreeCredential"", ""ExamplePersonCredential""],
            ""issuer"": ""https://university.example/issuers/14"",
            ""validFrom"": ""2010-01-01T19:23:24Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21"",
                ""degree"": {
                    ""type"": ""ExampleBachelorDegree"",
                    ""name"": ""Bachelor of Science and Arts""
                },
                ""alumniOf"": {
                    ""name"": ""Example University""
                }
            },
            ""credentialSchema"": [{
                ""id"": ""https://example.org/examples/degree.json"",
                ""type"": ""JsonSchema""
            },
            {
                ""id"": ""https://example.org/examples/alumni.json"",
                ""type"": ""JsonSchema""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSchema);
        Assert.Equal(2, credential.CredentialSchema.Count);
        
        var schemas = credential.CredentialSchema.ToArray();
        
        // First schema
        Assert.Equal("https://example.org/examples/degree.json", schemas[0].Id);
        Assert.Equal("JsonSchema", schemas[0].Type);
        
        // Second schema
        Assert.Equal("https://example.org/examples/alumni.json", schemas[1].Id);
        Assert.Equal("JsonSchema", schemas[1].Type);
    }

    [Fact]
    public void CredentialSchema_MultipleSchemas_SerializesAsArray()
    {
        // Arrange
        var schema1 = new CredentialSchema
        {
            Id = "https://example.org/examples/degree.json",
            Type = "JsonSchema"
        };
        
        var schema2 = new CredentialSchema
        {
            Id = "https://example.org/examples/alumni.json",
            Type = "JsonSchema"
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
            CredentialSchema = new Collection<CredentialSchema> { schema1, schema2 }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialSchema\": [", json);
        Assert.Contains("\"https://example.org/examples/degree.json\"", json);
        Assert.Contains("\"https://example.org/examples/alumni.json\"", json);
    }

    [Fact]
    public void CredentialSchema_MultipleSchemas_RoundTrip()
    {
        // Arrange
        var schema1 = new CredentialSchema 
        { 
            Id = "https://example.org/examples/schema1.json", 
            Type = "JsonSchema" 
        };
        var schema2 = new CredentialSchema 
        { 
            Id = "https://example.org/examples/schema2.json", 
            Type = "JsonSchema" 
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
            CredentialSchema = new Collection<CredentialSchema> { schema1, schema2 }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.CredentialSchema);
        Assert.Equal(2, deserialized.CredentialSchema.Count);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Missing/Null Schema Tests

    [Fact]
    public void CredentialSchema_MissingInJson_IsNull()
    {
        // Arrange - credentialSchema is optional per spec
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
        Assert.Null(credential.CredentialSchema); // Optional property should be null when missing
    }

    [Fact]
    public void CredentialSchema_NullInJson_DeserializesToNull()
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
            ""credentialSchema"": null
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Null(credential.CredentialSchema); // Null should deserialize to null
    }

    [Fact]
    public void CredentialSchema_EmptyCollection_SerializesAsOmitted()
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
            CredentialSchema = new Collection<CredentialSchema>() // Empty collection
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Converter writes null for empty collections, then JsonIgnore omits it
        Assert.DoesNotContain("\"credentialSchema\"", json);
    }

    [Fact]
    public void CredentialSchema_NullProperty_SerializesAsOmitted()
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
            CredentialSchema = null // Explicitly null
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Null should be omitted by JsonIgnore
        Assert.DoesNotContain("\"credentialSchema\"", json);
    }

    #endregion

    #region Required Properties Tests

    [Fact]
    public void CredentialSchema_WithoutId_FailsValidation()
    {
        // Arrange - Per spec: id is REQUIRED
        var credentialSchema = new CredentialSchema
        {
            Id = null!, // Missing required id
            Type = "JsonSchema"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credentialSchema);
        var isValid = Validator.TryValidateObject(
            credentialSchema, validationContext, validationResults, validateAllProperties: true);

        // Assert - Id is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Id"));
    }

    [Fact]
    public void CredentialSchema_WithoutType_FailsValidation()
    {
        // Arrange - Per spec: type is REQUIRED
        var credentialSchema = new CredentialSchema
        {
            Id = "https://example.org/schema.json",
            Type = null! // Missing required type
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credentialSchema);
        var isValid = Validator.TryValidateObject(
            credentialSchema, validationContext, validationResults, validateAllProperties: true);

        // Assert - Type is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Type"));
    }

    [Theory]
    [InlineData("JsonSchema")]
    [InlineData("1EdTechJsonSchemaValidator2019")]
    [InlineData("CustomSchemaType")]
    public void CredentialSchema_WithVariousTypes_Deserializes(string schemaType)
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
            ""credentialSchema"": {{
                ""id"": ""https://example.org/schema.json"",
                ""type"": ""{schemaType}""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSchema);
        Assert.Single(credential.CredentialSchema);
        Assert.Equal(schemaType, credential.CredentialSchema.First().Type);
    }

    [Theory]
    [InlineData("https://example.org/examples/degree.json")]
    [InlineData("https://purl.imsglobal.org/spec/clr/v2p0/schema/json/clr_v2p0_clrcredential_schema.json")]
    [InlineData("urn:uuid:3d4f3e8a-9c7b-4f2e-8d1a-5e6f7a8b9c0d")]
    public void CredentialSchema_WithVariousIdFormats_Deserializes(string schemaId)
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
            ""credentialSchema"": {{
                ""id"": ""{schemaId}"",
                ""type"": ""JsonSchema""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSchema);
        Assert.Single(credential.CredentialSchema);
        Assert.Equal(schemaId, credential.CredentialSchema.First().Id);
    }

    #endregion

    #region Additional Properties Tests

    [Fact]
    public void CredentialSchema_WithAdditionalProperties_PreservesData()
    {
        // Arrange
        var schema = new CredentialSchema
        {
            Id = "https://example.org/schema.json",
            Type = "JsonSchema"
        };
        schema.AdditionalProperties["version"] = "1.0";
        schema.AdditionalProperties["description"] = "Example schema";

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
            CredentialSchema = new Collection<CredentialSchema> { schema }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.CredentialSchema);
        var deserializedSchema = deserialized.CredentialSchema.First();
        Assert.True(deserializedSchema.AdditionalProperties.ContainsKey("version"));
        Assert.Equal("1.0", deserializedSchema.AdditionalProperties["version"]?.ToString());
        Assert.True(deserializedSchema.AdditionalProperties.ContainsKey("description"));
        Assert.Equal("Example schema", deserializedSchema.AdditionalProperties["description"]?.ToString());
    }

    #endregion
}