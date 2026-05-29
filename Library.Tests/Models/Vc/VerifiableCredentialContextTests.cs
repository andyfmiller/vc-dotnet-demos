using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialContextTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public VerifiableCredentialContextTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    [Fact]
    public void Context_SerializesStringOnly_Correctly()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object>
            {
                "https://www.w3.org/ns/credentials/v2",
                "https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json"
            },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Context.Count);
        Assert.Equal("https://www.w3.org/ns/credentials/v2", deserialized.Context.First().ToString());
    }

    [Fact]
    public void Context_SerializesMixedTypes_Correctly()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object>
            {
                "https://www.w3.org/ns/credentials/v2",
                "https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json",
                new Dictionary<string, object>
                {
                    { "ex", "https://example.org/vocab#" },
                    { "customField", "ex:customField" }
                }
            },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("@context", json);
        Assert.Contains("https://www.w3.org/ns/credentials/v2", json);
        Assert.Contains("https://example.org/vocab#", json);
    }

    [Fact]
    public void Context_DeserializesMixedTypes_Correctly()
    {
        // Arrange
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json"",
                {
                    ""ex"": ""https://example.org/vocab#"",
                    ""customField"": ""ex:customField""
                }
            ],
            ""type"": [""VerifiableCredential""],
            ""issuer"": {
                ""id"": ""https://example.com/issuer""
            },
            ""credentialSubject"": {},
            ""validFrom"": ""2024-01-01T00:00:00Z""
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Equal(3, credential.Context.Count);
        Assert.IsType<string>(credential.Context.ElementAt(0));
        Assert.IsType<string>(credential.Context.ElementAt(1));
        Assert.IsAssignableFrom<JsonElement>(credential.Context.ElementAt(2));
    }

    [Fact]
    public void Context_RoundTrip_PreservesData()
    {
        // Arrange
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object>
            {
                "https://www.w3.org/ns/credentials/v2",
                new Dictionary<string, object>
                {
                    { "@vocab", "https://www.w3.org/ns/credentials/issuer-dependent#" }
                }
            },
            Type = new Collection<string> { "VerifiableCredential", "OpenBadgeCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Context.Count, deserialized.Context.Count);
        Assert.Equal(json, reserializedJson);
    }

    [Fact]
    public void Context_FirstItemMustBeV2Url_ValidationPasses()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object>
            {
                "https://www.w3.org/ns/credentials/v2",
                "https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json"
            },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act & Assert
        Assert.Equal("https://www.w3.org/ns/credentials/v2", credential.Context.First().ToString());
    }

    [Fact]
    public void Context_EmptyCollection_FailsRequiredValidation()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object>(),
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credential);
        var isValid = Validator.TryValidateObject(
            credential, validationContext, validationResults, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Context"));
    }

    [Fact]
    public void Context_WithNestedContextObject_SerializesCorrectly()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object>
            {
                "https://www.w3.org/ns/credentials/v2",
                new Dictionary<string, object>
                {
                    { "name", "http://schema.org/name" },
                    { "description", "http://schema.org/description" },
                    { 
                        "achievement", 
                        new Dictionary<string, object>
                        {
                            { "@id", "http://example.org/achievement" },
                            { "@type", "@id" }
                        }
                    }
                }
            },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Context.Count);
        Assert.Contains("http://schema.org/name", json);
        Assert.Contains("@id", json);
    }

    [Fact]
    public void Context_WithSingleString_DeserializesAsArray()
    {
        // Arrange - Some systems might serialize a single context as a string rather than array
        var json = @"{
            ""@context"": ""https://www.w3.org/ns/credentials/v2"",
            ""type"": [""VerifiableCredential""],
            ""issuer"": {
                ""id"": ""https://example.com/issuer""
            },
            ""credentialSubject"": {},
            ""validFrom"": ""2024-01-01T00:00:00Z""
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Context);
        Assert.Single(credential.Context);
        Assert.Equal("https://www.w3.org/ns/credentials/v2", credential.Context.First().ToString());
    }

    [Fact]
    public void Context_PreservesOrder_InSerialization()
    {
        // Arrange
        var contexts = new Collection<object>
        {
            "https://www.w3.org/ns/credentials/v2",
            "https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json",
            "https://example.org/context/v1",
            new Dictionary<string, object> { { "ex", "https://example.org#" } }
        };

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = contexts,
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(4, deserialized.Context.Count);
        
        // Verify order is preserved
        var contextArray = deserialized.Context.ToArray();
        Assert.Equal("https://www.w3.org/ns/credentials/v2", contextArray[0].ToString());
        Assert.Equal("https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json", contextArray[1].ToString());
        Assert.Equal("https://example.org/context/v1", contextArray[2].ToString());
    }

    [Theory]
    [InlineData(1, "https://www.w3.org/ns/credentials/v2")]
    [InlineData(2, "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json")]
    [InlineData(3, "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json", "https://example.org/context")]
    public void Context_WithMultipleContexts_DeserializesCorrectly(int expectedCount, params string[] contextUrls)
    {
        // Arrange
        var contexts = new Collection<object>();
        foreach (var url in contextUrls)
        {
            contexts.Add(url);
        }

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = contexts,
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(expectedCount, deserialized.Context.Count);
        for (int i = 0; i < contextUrls.Length; i++)
        {
            Assert.Equal(contextUrls[i], deserialized.Context.ElementAt(i).ToString());
        }
    }

    [Fact]
    public void Context_NullValue_ThrowsValidationException()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = null!,
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer() { Id = "https://university.example/issuers/14" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credential);
        var isValid = Validator.TryValidateObject(
            credential, validationContext, validationResults, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Context"));
    }
}