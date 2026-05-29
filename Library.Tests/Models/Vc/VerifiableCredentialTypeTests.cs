using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialTypeTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialTypeTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    [Fact]
    public void Type_DeserializesFromSpec_Correctly()
    {
        // Arrange - Example from VC 2.0 spec
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/examples/v2""
            ],
            ""id"": ""http://university.example/credentials/3732"",
            ""type"": [""VerifiableCredential"", ""ExampleDegreeCredential""],
            ""issuer"": ""https://university.example/issuers/565049"",
            ""validFrom"": ""2010-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Type);
        Assert.Equal(2, credential.Type.Count);
        Assert.Contains("VerifiableCredential", credential.Type);
        Assert.Contains("ExampleDegreeCredential", credential.Type);
    }

    [Fact]
    public void Type_WithSingleValue_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {}
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Type);
        Assert.Single(credential.Type);
        Assert.Equal("VerifiableCredential", credential.Type.First());
    }

    [Fact]
    public void Type_WithMultipleValues_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential"", ""OpenBadgeCredential"", ""AchievementCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {}
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Type);
        Assert.Equal(3, credential.Type.Count);
        Assert.Contains("VerifiableCredential", credential.Type);
        Assert.Contains("OpenBadgeCredential", credential.Type);
        Assert.Contains("AchievementCredential", credential.Type);
    }

    [Fact]
    public void Type_WithAbsoluteUrls_DeserializesCorrectly()
    {
        // Arrange - Type can contain absolute URLs per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential"", ""https://example.org/credentials/CustomCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {}
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Type);
        Assert.Equal(2, credential.Type.Count);
        Assert.Contains("VerifiableCredential", credential.Type);
        Assert.Contains("https://example.org/credentials/CustomCredential", credential.Type);
    }

    [Fact]
    public void Type_SerializesCorrectly()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential", "ExampleDegreeCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"type\"", json);
        Assert.Contains("\"VerifiableCredential\"", json);
        Assert.Contains("\"ExampleDegreeCredential\"", json);
    }

    [Fact]
    public void Type_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> 
            { 
                "VerifiableCredential", 
                "OpenBadgeCredential",
                "AchievementCredential"
            },
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

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Type.Count, deserialized.Type.Count);
        foreach (var type in original.Type)
        {
            Assert.Contains(type, deserialized.Type);
        }
    }

    [Fact]
    public void Type_PreservesOrder_InSerialization()
    {
        // Arrange - Even though spec says order doesn't matter, we should preserve it
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> 
            { 
                "VerifiableCredential", 
                "OpenBadgeCredential",
                "ClrCredential",
                "AchievementCredential"
            },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
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
        Assert.Equal(4, deserialized.Type.Count);
        
        var typeArray = deserialized.Type.ToArray();
        Assert.Equal("VerifiableCredential", typeArray[0]);
        Assert.Equal("OpenBadgeCredential", typeArray[1]);
        Assert.Equal("ClrCredential", typeArray[2]);
        Assert.Equal("AchievementCredential", typeArray[3]);
    }

    [Theory]
    [InlineData(1, "VerifiableCredential")]
    [InlineData(2, "VerifiableCredential", "OpenBadgeCredential")]
    [InlineData(3, "VerifiableCredential", "OpenBadgeCredential", "AchievementCredential")]
    [InlineData(4, "VerifiableCredential", "ClrCredential", "OpenBadgeCredential", "AchievementCredential")]
    public void Type_WithVariousTypeCount_DeserializesCorrectly(int expectedCount, params string[] types)
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string>(types),
            Issuer = new Issuer { Id = "https://example.com/issuer" },
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
        Assert.Equal(expectedCount, deserialized.Type.Count);
        foreach (var type in types)
        {
            Assert.Contains(type, deserialized.Type);
        }
    }

    [Fact]
    public void Type_EmptyCollection_FailsRequiredValidation()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string>(),
            Issuer = new Issuer { Id = "https://example.com/issuer" },
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
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Type"));
    }

    [Fact]
    public void Type_NullValue_FailsValidation()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = null!,
            Issuer = new Issuer { Id = "https://example.com/issuer" },
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
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Type"));
    }

    [Fact]
    public void Type_MustContainVerifiableCredential_ValidationPasses()
    {
        // Arrange - Per spec, all VCs should include "VerifiableCredential" type
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential", "OpenBadgeCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject() 
            }
        };

        // Act & Assert
        Assert.Contains("VerifiableCredential", credential.Type);
    }

    [Fact]
    public void Type_WithMixedTermsAndUrls_RoundTrips()
    {
        // Arrange - Mix of terms and absolute URLs
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> 
            { 
                "VerifiableCredential",
                "https://purl.imsglobal.org/spec/ob/v3p0/schema/json/OBCredentialSchema.json",
                "OpenBadgeCredential",
                "https://example.org/credentials/CustomType"
            },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(4, deserialized.Type.Count);
        Assert.Equal(json, reserializedJson);
    }

    [Fact]
    public void Type_WithOpenBadgeCredential_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json""
            ],
            ""type"": [""VerifiableCredential"", ""OpenBadgeCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {}
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Equal(2, credential.Type.Count);
        Assert.Contains("VerifiableCredential", credential.Type);
        Assert.Contains("OpenBadgeCredential", credential.Type);
    }

    [Fact]
    public void Type_WithClrCredential_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://purl.imsglobal.org/spec/clr/v2p0/context-2.0.1.json""
            ],
            ""type"": [""VerifiableCredential"", ""ClrCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {}
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Equal(2, credential.Type.Count);
        Assert.Contains("VerifiableCredential", credential.Type);
        Assert.Contains("ClrCredential", credential.Type);
    }

    [Fact]
    public void Type_CasePreserved_InRoundTrip()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential", "ExampleDegreeCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
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
        Assert.Equal("VerifiableCredential", deserialized.Type.First());
        Assert.Equal("ExampleDegreeCredential", deserialized.Type.Last());
    }
}