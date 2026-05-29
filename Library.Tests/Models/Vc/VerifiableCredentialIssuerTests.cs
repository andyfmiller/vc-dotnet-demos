using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialIssuerTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialIssuerTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Simple URL Form Tests

    [Fact]
    public void Issuer_SimpleUrlForm_DeserializesFromSpec()
    {
        // Arrange - Example from VC 2.0 spec with simple URL issuer
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://w3id.org/age/v1""
            ],
            ""type"": [""VerifiableCredential"", ""AgeVerificationCredential""],
            ""issuer"": ""did:key:z6MksFxi8wnHkNq4zgEskSZF45SuWQ4HndWSAVYRRGe9qDks"",
            ""validFrom"": ""2024-04-03T00:00:00.000Z"",
            ""credentialSubject"": {
                ""overAge"": 21
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Issuer);
        Assert.Equal("did:key:z6MksFxi8wnHkNq4zgEskSZF45SuWQ4HndWSAVYRRGe9qDks", credential.Issuer.Id);
        Assert.Empty(credential.Issuer.AdditionalProperties);
    }

    [Fact]
    public void Issuer_SimpleUrlForm_SerializesAsString()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Should serialize as simple string, not object
        Assert.Contains("\"issuer\": \"https://example.com/issuer\"", json);
        Assert.DoesNotContain("\"issuer\": {", json);
    }

    [Fact]
    public void Issuer_SimpleUrlForm_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "did:example:ebfeb1f712ebc6f1c276e12ec21" },
            ValidFrom = new DateTimeOffset(2024, 4, 3, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Issuer);
        Assert.Equal(original.Issuer.Id, deserialized.Issuer.Id);
        Assert.Empty(deserialized.Issuer.AdditionalProperties);
        Assert.Equal(json, reserializedJson);
    }

    [Fact]
    public void Issuer_FromUrl_CreatesSimpleIssuer()
    {
        // Arrange
        var url = "https://example.com/issuers/1234";

        // Act
        var issuer = Issuer.FromUrl(url);

        // Assert
        Assert.NotNull(issuer);
        Assert.Equal(url, issuer.Id);
        Assert.Empty(issuer.AdditionalProperties);
    }

    #endregion

    #region Object Form Tests

    [Fact]
    public void Issuer_ObjectForm_WithIdOnly_Deserializes()
    {
        // Arrange - Issuer as object with just id property
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": {
                ""id"": ""https://example.com/issuer""
            },
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Issuer);
        Assert.Equal("https://example.com/issuer", credential.Issuer.Id);
    }

    [Fact]
    public void Issuer_ObjectForm_WithAdditionalProperties_Deserializes()
    {
        // Arrange - Issuer with additional properties like name, description
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": {
                ""id"": ""https://university.example/issuers/565049"",
                ""name"": ""Example University"",
                ""description"": ""A public university focusing on teaching examples.""
            },
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Issuer);
        Assert.Equal("https://university.example/issuers/565049", credential.Issuer.Id);
        Assert.NotNull(credential.Issuer.AdditionalProperties);
        Assert.True(credential.Issuer.AdditionalProperties.ContainsKey("name"));
        Assert.Equal("Example University", credential.Issuer.AdditionalProperties["name"]?.ToString());
        Assert.True(credential.Issuer.AdditionalProperties.ContainsKey("description"));
        Assert.Equal("A public university focusing on teaching examples.", credential.Issuer.AdditionalProperties["description"]?.ToString());
    }

    [Fact]
    public void Issuer_ObjectForm_WithAdditionalProperties_SerializesAsObject()
    {
        // Arrange
        var issuer = new Issuer { Id = "https://university.example/issuers/565049" };
        issuer.AdditionalProperties = new Dictionary<string, object>
        {
            ["name"] = "Example University",
            ["description"] = "A public university focusing on teaching examples."
        };

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = issuer,
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Should serialize as object when additional properties exist
        Assert.Contains("\"issuer\": {", json);
        Assert.Contains("\"id\": \"https://university.example/issuers/565049\"", json);
        Assert.Contains("\"name\": \"Example University\"", json);
        Assert.Contains("\"description\"", json);
    }

    [Fact]
    public void Issuer_ObjectForm_RoundTrip_PreservesData()
    {
        // Arrange
        var issuer = new Issuer { Id = "https://university.example/issuers/565049" };
        issuer.AdditionalProperties = new Dictionary<string, object>
        {
            ["name"] = "Example University",
            ["type"] = "Organization",
            ["url"] = "https://university.example"
        };

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = issuer,
            ValidFrom = new DateTimeOffset(2024, 4, 3, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        _output.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Issuer);
        Assert.Equal(original.Issuer.Id, deserialized.Issuer.Id);
        Assert.NotNull(deserialized.Issuer.AdditionalProperties);
        Assert.Equal(3, deserialized.Issuer.AdditionalProperties.Count);
        Assert.Equal("Example University", deserialized.Issuer.AdditionalProperties["name"]?.ToString());
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Required Property Tests

    [Fact]
    public void Issuer_WithoutId_FailsValidation()
    {
        // Arrange - Per spec: id is REQUIRED
        var issuer = new Issuer
        {
            Id = null! // Missing required id
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(issuer);
        var isValid = Validator.TryValidateObject(
            issuer, validationContext, validationResults, validateAllProperties: true);

        // Assert - Id is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Id"));
    }

    [Fact]
    public void Issuer_ObjectWithoutId_ThrowsJsonException()
    {
        // Arrange - Issuer object must have id property per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": {
                ""name"": ""Example University""
            },
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act & Assert
        Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions));
    }

    [Fact]
    public void Issuer_MissingInJson_ThrowsJsonException()
    {
        // Arrange - issuer is required per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act & Assert - Should fail because Issuer is required
        Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions));
    }

    [Fact]
    public void Issuer_NullInJson_ThrowsJsonException()
    {
        // Arrange - issuer cannot be null per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": null,
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act & Assert
        Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions));
    }

    #endregion

    #region Various URL Format Tests

    [Theory]
    [InlineData("https://example.com/issuers/1234")]
    [InlineData("https://university.example/issuers/565049")]
    [InlineData("did:key:z6MksFxi8wnHkNq4zgEskSZF45SuWQ4HndWSAVYRRGe9qDks")]
    [InlineData("did:web:example.com")]
    [InlineData("did:ion:EiClkZMDxPKqC9c-umQfTkR8vvZ9JPhl_xLDI9Nfk38w5w")]
    [InlineData("urn:uuid:3d4f3e8a-9c7b-4f2e-8d1a-5e6f7a8b9c0d")]
    public void Issuer_WithVariousUrlFormats_Deserializes(string issuerId)
    {
        // Arrange
        var json = $@"{{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""{issuerId}"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {{
                ""id"": ""did:example:123""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Issuer);
        Assert.Equal(issuerId, credential.Issuer.Id);
    }

    [Theory]
    [InlineData("https://example.com/issuers/1234")]
    [InlineData("did:key:z6MksFxi8wnHkNq4zgEskSZF45SuWQ4HndWSAVYRRGe9qDks")]
    [InlineData("did:web:example.com")]
    public void Issuer_WithVariousUrlFormats_SerializesCorrectly(string issuerId)
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = issuerId },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains($"\"issuer\": \"{issuerId}\"", json);
    }

    #endregion

    #region Additional Properties Tests

    [Fact]
    public void Issuer_WithMultipleAdditionalProperties_PreservesAllData()
    {
        // Arrange
        var issuer = new Issuer { Id = "https://university.example/issuers/565049" };
        issuer.AdditionalProperties = new Dictionary<string, object>
        {
            ["name"] = "Example University",
            ["description"] = "A public university",
            ["type"] = "Organization",
            ["url"] = "https://university.example",
            ["email"] = "contact@university.example",
            ["image"] = "https://university.example/logo.png"
        };

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = issuer,
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Issuer);
        Assert.NotNull(deserialized.Issuer.AdditionalProperties);
        Assert.Equal(6, deserialized.Issuer.AdditionalProperties.Count);
        
        Assert.Equal("Example University", deserialized.Issuer.AdditionalProperties["name"]?.ToString());
        Assert.Equal("A public university", deserialized.Issuer.AdditionalProperties["description"]?.ToString());
        Assert.Equal("Organization", deserialized.Issuer.AdditionalProperties["type"]?.ToString());
        Assert.Equal("https://university.example", deserialized.Issuer.AdditionalProperties["url"]?.ToString());
        Assert.Equal("contact@university.example", deserialized.Issuer.AdditionalProperties["email"]?.ToString());
        Assert.Equal("https://university.example/logo.png", deserialized.Issuer.AdditionalProperties["image"]?.ToString());
    }

    [Fact]
    public void Issuer_WithNestedObjectInAdditionalProperties_Deserializes()
    {
        // Arrange - Additional properties can be complex objects
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": {
                ""id"": ""https://university.example/issuers/565049"",
                ""name"": ""Example University"",
                ""address"": {
                    ""street"": ""123 University Ave"",
                    ""city"": ""Example City"",
                    ""state"": ""EX"",
                    ""zip"": ""12345""
                }
            },
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Issuer);
        Assert.NotNull(credential.Issuer.AdditionalProperties);
        Assert.True(credential.Issuer.AdditionalProperties.ContainsKey("name"));
        Assert.True(credential.Issuer.AdditionalProperties.ContainsKey("address"));
    }

    [Fact]
    public void Issuer_WithEmptyAdditionalProperties_SerializesAsString()
    {
        // Arrange - Empty additional properties should result in simple string form
        var issuer = new Issuer { Id = "https://example.com/issuer" };
        issuer.AdditionalProperties = new Dictionary<string, object>(); // Empty

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = issuer,
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Empty additional properties should serialize as simple string
        Assert.Contains("\"issuer\": \"https://example.com/issuer\"", json);
        Assert.DoesNotContain("\"issuer\": {", json);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void Issuer_InvalidJsonType_ThrowsJsonException()
    {
        // Arrange - issuer as number (invalid)
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": 12345,
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act & Assert
        Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions));
    }

    [Fact]
    public void Issuer_EmptyStringId_Deserializes()
    {
        // Arrange - technically invalid per spec, but test behavior
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": """",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert - Deserializes but validation would fail
        Assert.NotNull(credential);
        Assert.NotNull(credential.Issuer);
        Assert.Equal(string.Empty, credential.Issuer.Id);
    }

    [Fact]
    public void Issuer_ArrayForm_ThrowsJsonException()
    {
        // Arrange - issuer as array (invalid per spec)
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": [""https://example.com/issuer1"", ""https://example.com/issuer2""],
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act & Assert
        Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions));
    }

    #endregion

    #region Controlled Identifier Document Tests

    [Fact]
    public void Issuer_RecommendedControlledIdentifierFormat_Deserializes()
    {
        // Arrange - Per spec recommendation for controlled identifier documents
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": {
                ""id"": ""https://university.example/issuers/565049"",
                ""name"": ""Example University"",
                ""description"": ""A public university"",
                ""image"": ""https://university.example/logo.png""
            },
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert - This format allows for dereferencing to get controlled identifier document
        Assert.NotNull(credential);
        Assert.NotNull(credential.Issuer);
        Assert.Equal("https://university.example/issuers/565049", credential.Issuer.Id);
        Assert.NotNull(credential.Issuer.AdditionalProperties);
        Assert.True(credential.Issuer.AdditionalProperties.ContainsKey("name"));
    }

    #endregion

    #region Conversion Between Forms Tests

    [Fact]
    public void Issuer_ConvertFromSimpleToObjectForm_Works()
    {
        // Arrange - Start with simple form
        var issuer = new Issuer { Id = "https://example.com/issuer" };
        
        // Act - Add additional properties to convert to object form
        issuer.AdditionalProperties = new Dictionary<string, object>
        {
            ["name"] = "Example Organization"
        };

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = issuer,
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Should now serialize as object
        Assert.Contains("\"issuer\": {", json);
        Assert.Contains("\"id\": \"https://example.com/issuer\"", json);
        Assert.Contains("\"name\": \"Example Organization\"", json);
    }

    #endregion
}