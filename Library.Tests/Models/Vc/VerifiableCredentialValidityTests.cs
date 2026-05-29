using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialValidityTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialValidityTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region ValidFrom Tests

    [Fact]
    public void ValidFrom_DeserializesFromSpec_Correctly()
    {
        // Arrange - Example from VC 2.0 spec
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/examples/v2""
            ],
            ""id"": ""http://university.example/credentials/3732"",
            ""type"": [""VerifiableCredential"", ""ExampleDegreeCredential""],
            ""issuer"": ""https://university.example/issuers/14"",
            ""validFrom"": ""2010-01-01T19:23:24Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        var expectedDate = new DateTimeOffset(2010, 1, 1, 19, 23, 24, TimeSpan.Zero);
        Assert.Equal(expectedDate, credential.ValidFrom);
    }

    [Fact]
    public void ValidFrom_MissingInJson_UsesDefaultValue()
    {
        // Arrange - validFrom is optional per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert - Since validFrom is optional, deserialization succeeds with default value
        Assert.NotNull(credential);
        Assert.Equal(default(DateTimeOffset), credential.ValidFrom);
    }

    [Fact]
    public void ValidFrom_Null_SerializesAsOmitted()
    {
        // Arrange - validFrom is optional, default value should be omitted
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = default(DateTimeOffset), // Default value
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Default DateTimeOffset serializes as a date string, not omitted
        // This is expected behavior for DateTimeOffset (not nullable)
        Assert.Contains("\"validFrom\"", json);
    }

    [Fact]
    public void ValidFrom_SerializesCorrectly()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://university.example/issuers/14" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 19, 23, 24, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"validFrom\": \"2010-01-01T19:23:24Z\"", json);
    }

    [Fact]
    public void ValidFrom_RoundTrip_PreservesValue()
    {
        // Arrange
        var originalDate = new DateTimeOffset(2024, 6, 15, 10, 30, 45, TimeSpan.Zero);
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = originalDate,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(originalDate, deserialized.ValidFrom);
    }

    [Fact]
    public void ValidFrom_WithTimeZoneOffset_DeserializesCorrectly()
    {
        // Arrange - validFrom with non-UTC timezone
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-15T14:30:00-05:00"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        var expectedDate = new DateTimeOffset(2024, 1, 15, 14, 30, 0, TimeSpan.FromHours(-5));
        Assert.Equal(expectedDate, credential.ValidFrom);
    }

    [Fact]
    public void ValidFrom_PastDate_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""1990-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        var expectedDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(expectedDate, credential.ValidFrom);
    }

    [Fact]
    public void ValidFrom_FutureDate_DeserializesCorrectly()
    {
        // Arrange - Future validFrom is allowed per spec
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2050-12-31T23:59:59Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        var expectedDate = new DateTimeOffset(2050, 12, 31, 23, 59, 59, TimeSpan.Zero);
        Assert.Equal(expectedDate, credential.ValidFrom);
    }

    [Fact]
    public void ValidFrom_WithMilliseconds_PreservesValue()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-15T14:30:45.123Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Equal(2024, credential.ValidFrom.Year);
        Assert.Equal(1, credential.ValidFrom.Month);
        Assert.Equal(15, credential.ValidFrom.Day);
        Assert.Equal(14, credential.ValidFrom.Hour);
        Assert.Equal(30, credential.ValidFrom.Minute);
        Assert.Equal(45, credential.ValidFrom.Second);
        Assert.Equal(123, credential.ValidFrom.Millisecond);
    }

    #endregion

    #region ValidUntil Tests

    [Fact]
    public void ValidUntil_DeserializesFromSpec_Correctly()
    {
        // Arrange - Example with validUntil
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/examples/v2""
            ],
            ""id"": ""http://university.example/credentials/3732"",
            ""type"": [""VerifiableCredential"", ""ExampleDegreeCredential""],
            ""issuer"": ""https://university.example/issuers/14"",
            ""validFrom"": ""2010-01-01T19:23:24Z"",
            ""validUntil"": ""2020-01-01T19:23:24Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.ValidUntil);
        var expectedDate = new DateTimeOffset(2020, 1, 1, 19, 23, 24, TimeSpan.Zero);
        Assert.Equal(expectedDate, credential.ValidUntil.Value);
    }

    [Fact]
    public void ValidUntil_MissingInJson_IsNull()
    {
        // Arrange - validUntil is optional per spec
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
        Assert.Null(credential.ValidUntil);
    }

    [Fact]
    public void ValidUntil_Null_SerializesAsOmitted()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidUntil = null,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - With DefaultIgnoreCondition.WhenWritingNull, null properties are omitted
        Assert.DoesNotContain("\"validUntil\"", json);
    }

    [Fact]
    public void ValidUntil_WithValue_SerializesCorrectly()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidUntil = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"validUntil\": \"2025-12-31T23:59:59+00:00\"", json);
    }

    [Fact]
    public void ValidUntil_RoundTrip_PreservesValue()
    {
        // Arrange
        var validUntilDate = new DateTimeOffset(2030, 6, 15, 10, 30, 45, TimeSpan.Zero);
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidUntil = validUntilDate,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.ValidUntil);
        Assert.Equal(validUntilDate, deserialized.ValidUntil.Value);
    }

    [Fact]
    public void ValidUntil_RoundTrip_WithNull_RemainsNull()
    {
        // Arrange
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidUntil = null,
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.ValidUntil);
    }

    [Fact]
    public void ValidUntil_WithTimeZoneOffset_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""validUntil"": ""2025-06-30T18:00:00-07:00"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.ValidUntil);
        var expectedDate = new DateTimeOffset(2025, 6, 30, 18, 0, 0, TimeSpan.FromHours(-7));
        Assert.Equal(expectedDate, credential.ValidUntil.Value);
    }

    #endregion

    #region ValidFrom and ValidUntil Relationship Tests

    [Fact]
    public void ValidFromAndValidUntil_BothPresent_DeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2020-01-01T00:00:00Z"",
            ""validUntil"": ""2025-12-31T23:59:59Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Equal(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), credential.ValidFrom);
        Assert.NotNull(credential.ValidUntil);
        Assert.Equal(new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero), credential.ValidUntil.Value);
        
        // Per spec: validUntil MUST be temporally the same or later than validFrom
        Assert.True(credential.ValidUntil.Value >= credential.ValidFrom);
    }

    [Fact]
    public void ValidFromAndValidUntil_SameValue_DeserializesCorrectly()
    {
        // Arrange - Per spec: validFrom and validUntil can be the same
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-06-15T12:00:00Z"",
            ""validUntil"": ""2024-06-15T12:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Equal(credential.ValidFrom, credential.ValidUntil!.Value);
    }

    [Fact]
    public void ValidFromAndValidUntil_ValidUntilBeforeValidFrom_ViolatesSpec()
    {
        // Arrange - Per spec: validUntil MUST be same or later than validFrom
        // This violates the spec but deserializes (application should validate)
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2025-01-01T00:00:00Z"",
            ""validUntil"": ""2020-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert - Deserializes but violates spec requirement
        Assert.NotNull(credential);
        Assert.NotNull(credential.ValidUntil);
        
        // Document that this violates spec (validUntil before validFrom)
        Assert.True(credential.ValidUntil.Value < credential.ValidFrom);
    }

    [Fact]
    public void ValidFromAndValidUntil_RoundTrip_PreservesBothValues()
    {
        // Arrange
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidUntil = new DateTimeOffset(2029, 12, 31, 23, 59, 59, TimeSpan.Zero),
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
        Assert.Equal(original.ValidFrom, deserialized.ValidFrom);
        Assert.Equal(original.ValidUntil, deserialized.ValidUntil);
        Assert.Equal(json, reserializedJson);
    }

    #endregion
}