using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialStatusTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialStatusTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Single Status Tests

    [Fact]
    public void CredentialStatus_SingleStatus_DeserializesFromSpec()
    {
        // Arrange - Example from VC 2.0 spec with single status
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
            },
            ""credentialStatus"": {
                ""id"": ""https://university.example/credentials/status/3#94567"",
                ""type"": ""BitstringStatusListEntry"",
                ""statusPurpose"": ""revocation"",
                ""statusListIndex"": ""94567"",
                ""statusListCredential"": ""https://university.example/credentials/status/3""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Single(credential.CredentialStatus);
        
        var status = credential.CredentialStatus!.First();
        Assert.Equal("https://university.example/credentials/status/3#94567", status.Id);
        Assert.Equal("BitstringStatusListEntry", status.Type);
    }

    [Fact]
    public void CredentialStatus_SingleStatus_SerializesAsObject()
    {
        // Arrange
        var credentialStatus = new CredentialStatus
        {
            Id = "https://example.com/status/123",
            Type = "BitstringStatusListEntry"
        };
        credentialStatus.AdditionalProperties["statusPurpose"] = "revocation";
        credentialStatus.AdditionalProperties["statusListIndex"] = "123";

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
            CredentialStatus = new Collection<CredentialStatus> { credentialStatus }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialStatus\": {", json);
        Assert.DoesNotContain("\"credentialStatus\": [", json);
        Assert.Contains("\"type\": \"BitstringStatusListEntry\"", json);
        Assert.Contains("\"id\": \"https://example.com/status/123\"", json);
    }

    [Fact]
    public void CredentialStatus_SingleStatus_RoundTrip_PreservesData()
    {
        // Arrange
        var originalStatus = new CredentialStatus
        {
            Id = "https://university.example/credentials/status/3#94567",
            Type = "BitstringStatusListEntry"
        };
        originalStatus.AdditionalProperties["statusPurpose"] = "revocation";
        originalStatus.AdditionalProperties["statusListIndex"] = "94567";
        originalStatus.AdditionalProperties["statusListCredential"] = "https://university.example/credentials/status/3";

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential", "ExampleDegreeCredential" },
            Issuer = new Issuer { Id = "https://university.example/issuers/14" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 19, 23, 24, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:ebfeb1f712ebc6f1c276e12ec21" } 
            },
            CredentialStatus = new Collection<CredentialStatus> { originalStatus }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        Assert.NotNull(deserialized); // Ensure deserialized is not null before dereferencing
        Assert.NotNull(deserialized.CredentialStatus); // Ensure CredentialStatus is not null before dereferencing
        var reserializedJson = JsonSerializer.Serialize(deserialized!, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.CredentialStatus); // Ensure CredentialStatus is not null before dereferencing
        Assert.Single(deserialized.CredentialStatus);
        Assert.Equal(json, reserializedJson);
    }

    [Fact]
    public void CredentialStatus_WithAdditionalProperties_Deserializes()
    {
        // Arrange - BitstringStatusListEntry with additional properties
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""credentialStatus"": {
                ""id"": ""https://example.com/status/3#94567"",
                ""type"": ""BitstringStatusListEntry"",
                ""statusPurpose"": ""revocation"",
                ""statusListIndex"": ""94567"",
                ""statusListCredential"": ""https://example.com/status/3""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Single(credential.CredentialStatus);
        
        var status = credential.CredentialStatus.First();
        Assert.True(status.AdditionalProperties.ContainsKey("statusPurpose"));
        Assert.Equal("revocation", status.AdditionalProperties["statusPurpose"]?.ToString());
        Assert.True(status.AdditionalProperties.ContainsKey("statusListIndex"));
        Assert.Equal("94567", status.AdditionalProperties["statusListIndex"]?.ToString());
        Assert.True(status.AdditionalProperties.ContainsKey("statusListCredential"));
        Assert.Equal("https://example.com/status/3", status.AdditionalProperties["statusListCredential"]?.ToString());
    }

    #endregion

    #region Multiple Status Tests

    [Fact]
    public void CredentialStatus_MultipleStatuses_DeserializesFromSpec()
    {
        // Arrange - Example from spec with multiple statuses (revocation and suspension)
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/examples/v2""
            ],
            ""id"": ""http://license.example/credentials/9837"",
            ""type"": [""VerifiableCredential"", ""ExampleDrivingLicenseCredential""],
            ""issuer"": ""https://license.example/issuers/48"",
            ""validFrom"": ""2020-03-14T12:10:42Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:f1c276e12ec21ebfeb1f712ebc6""
            },
            ""credentialStatus"": [{
                ""id"": ""https://license.example/credentials/status/84#14278"",
                ""type"": ""BitstringStatusListEntry"",
                ""statusPurpose"": ""revocation"",
                ""statusListIndex"": ""14278"",
                ""statusListCredential"": ""https://license.example/credentials/status/84""
            }, {
                ""id"": ""https://license.example/credentials/status/84#82938"",
                ""type"": ""BitstringStatusListEntry"",
                ""statusPurpose"": ""suspension"",
                ""statusListIndex"": ""82938"",
                ""statusListCredential"": ""https://license.example/credentials/status/84""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Equal(2, credential.CredentialStatus.Count);
        
        var statuses = credential.CredentialStatus.ToArray();
        
        // First status (revocation)
        Assert.Equal("https://license.example/credentials/status/84#14278", statuses[0].Id);
        Assert.Equal("BitstringStatusListEntry", statuses[0].Type);
        Assert.Equal("revocation", statuses[0].AdditionalProperties["statusPurpose"]?.ToString());
        
        // Second status (suspension)
        Assert.Equal("https://license.example/credentials/status/84#82938", statuses[1].Id);
        Assert.Equal("BitstringStatusListEntry", statuses[1].Type);
        Assert.Equal("suspension", statuses[1].AdditionalProperties["statusPurpose"]?.ToString());
    }

    [Fact]
    public void CredentialStatus_MultipleStatuses_SerializesAsArray()
    {
        // Arrange
        var status1 = new CredentialStatus
        {
            Id = "https://example.com/status/84#14278",
            Type = "BitstringStatusListEntry"
        };
        status1.AdditionalProperties["statusPurpose"] = "revocation";
        
        var status2 = new CredentialStatus
        {
            Id = "https://example.com/status/84#82938",
            Type = "BitstringStatusListEntry"
        };
        status2.AdditionalProperties["statusPurpose"] = "suspension";

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
            CredentialStatus = new Collection<CredentialStatus> { status1, status2 }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialStatus\": [", json);
        Assert.Contains("\"revocation\"", json);
        Assert.Contains("\"suspension\"", json);
    }

    [Fact]
    public void CredentialStatus_MultipleStatuses_RoundTrip()
    {
        // Arrange
        var status1 = new CredentialStatus { Id = "https://example.com/status/1", Type = "Type1" };
        var status2 = new CredentialStatus { Id = "https://example.com/status/2", Type = "Type2" };

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
            CredentialStatus = new Collection<CredentialStatus> { status1, status2 }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        Assert.NotNull(deserialized); // Ensure deserialized is not null before dereferencing
        Assert.NotNull(deserialized.CredentialStatus); // Ensure CredentialStatus is not null before dereferencing
        var reserializedJson = JsonSerializer.Serialize(deserialized!, _jsonOptions);

        // Assert
        Assert.Equal(2, deserialized.CredentialStatus.Count);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Missing/Null Status Tests

    [Fact]
    public void CredentialStatus_MissingInJson_IsNull()
    {
        // Arrange - credentialStatus is optional per spec
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
        Assert.Null(credential.CredentialStatus); // Optional property should be null when missing
    }

    [Fact]
    public void CredentialStatus_NullInJson_DeserializesToNull()
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
            ""credentialStatus"": null
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Null(credential.CredentialStatus); // Null should deserialize to null
    }

    [Fact]
    public void CredentialStatus_EmptyCollection_SerializesAsOmitted()
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
            CredentialStatus = new Collection<CredentialStatus>() // Empty collection
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Converter writes null for empty collections, then JsonIgnore omits it
        Assert.DoesNotContain("\"credentialStatus\"", json);
    }

    [Fact]
    public void CredentialStatus_NullProperty_SerializesAsOmitted()
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
            CredentialStatus = null // Explicitly null
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Null should be omitted by JsonIgnore
        Assert.DoesNotContain("\"credentialStatus\"", json);
    }

    #endregion

    #region Type Property Tests

    [Fact]
    public void CredentialStatus_WithRequiredType_Deserializes()
    {
        // Arrange - Status with required type property
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""credentialStatus"": {
                ""type"": ""BitstringStatusListEntry""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Single(credential.CredentialStatus);
        Assert.Equal("BitstringStatusListEntry", credential.CredentialStatus.First().Type);
        Assert.Null(credential.CredentialStatus.First().Id); // Id is optional
    }

    [Fact]
    public void CredentialStatus_WithoutType_FailsValidation()
    {
        // Arrange - Per spec: type is REQUIRED
        var credentialStatus = new CredentialStatus
        {
            Id = "https://example.com/status/123",
            Type = null! // Missing required type
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credentialStatus);
        var isValid = Validator.TryValidateObject(
            credentialStatus, validationContext, validationResults, validateAllProperties: true);

        // Assert - Type is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Type"));
    }

    [Theory]
    [InlineData("BitstringStatusListEntry")]
    [InlineData("StatusList2021Entry")]
    [InlineData("CustomStatusType")]
    public void CredentialStatus_WithVariousTypes_Deserializes(string statusType)
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
            ""credentialStatus"": {{
                ""type"": ""{statusType}""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Single(credential.CredentialStatus);
        Assert.Equal(statusType, credential.CredentialStatus.First().Type);
    }

    #endregion

    #region ID Property Tests

    [Fact]
    public void CredentialStatus_WithoutId_IsValid()
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
            ""credentialStatus"": {
                ""type"": ""BitstringStatusListEntry""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Single(credential.CredentialStatus);
        Assert.Null(credential.CredentialStatus.First().Id);
        Assert.Equal("BitstringStatusListEntry", credential.CredentialStatus.First().Type);
    }

    [Theory]
    [InlineData("https://example.com/status/123")]
    [InlineData("https://example.com/credentials/status/3#94567")]
    [InlineData("urn:uuid:3d4f3e8a-9c7b-4f2e-8d1a-5e6f7a8b9c0d")]
    public void CredentialStatus_WithVariousIdFormats_Deserializes(string statusId)
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
            ""credentialStatus"": {{
                ""id"": ""{statusId}"",
                ""type"": ""BitstringStatusListEntry""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Single(credential.CredentialStatus);
        Assert.Equal(statusId, credential.CredentialStatus.First().Id);
    }

    #endregion

    #region Additional Properties Tests

    [Theory]
    [InlineData("revocation")]
    [InlineData("suspension")]
    public void CredentialStatus_WithDifferentStatusPurposes_Deserializes(string purpose)
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
            ""credentialStatus"": {{
                ""type"": ""BitstringStatusListEntry"",
                ""statusPurpose"": ""{purpose}""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialStatus);
        Assert.Single(credential.CredentialStatus);
        Assert.Equal(purpose, credential.CredentialStatus.First().AdditionalProperties["statusPurpose"]?.ToString());
    }

    #endregion
}