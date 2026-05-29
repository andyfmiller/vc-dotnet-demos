using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialRefreshServiceTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialRefreshServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Single RefreshService Tests

    [Fact]
    public void RefreshService_SingleService_DeserializesFromSpec()
    {
        // Arrange - Example from VC 2.0 spec with single refresh service
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://w3id.org/age/v1""
            ],
            ""type"": [""VerifiableCredential"", ""AgeVerificationCredential""],
            ""issuer"": ""did:key:z6MksFxi8wnHkNq4zgEskSZF45SuWQ4HndWSAVYRRGe9qDks"",
            ""validFrom"": ""2024-04-03T00:00:00.000Z"",
            ""validUntil"": ""2024-12-15T00:00:00.000Z"",
            ""name"": ""Age Verification Credential"",
            ""credentialSubject"": {
                ""overAge"": 21
            },
            ""refreshService"": {
                ""type"": ""VerifiableCredentialRefreshService2021"",
                ""id"": ""https://registration.provider.example/flows/reissue-age-token""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.RefreshService);
        Assert.Single(credential.RefreshService);
        
        var service = credential.RefreshService.First();
        Assert.Equal("https://registration.provider.example/flows/reissue-age-token", service.Id);
        Assert.Equal("VerifiableCredentialRefreshService2021", service.Type);
    }

    [Fact]
    public void RefreshService_SingleService_WithAdditionalProperties_Deserializes()
    {
        // Arrange - Refresh service with additional properties like refreshToken
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""refreshService"": {
                ""type"": ""VerifiableCredentialRefreshService2021"",
                ""id"": ""https://registration.provider.example/flows/reissue"",
                ""refreshToken"": ""z2BJYfNtmWRiouWhDrbDQmC2zicUPBxsPg""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.RefreshService);
        Assert.Single(credential.RefreshService);
        
        var service = credential.RefreshService.First();
        Assert.True(service.AdditionalProperties.ContainsKey("refreshToken"));
        Assert.Equal("z2BJYfNtmWRiouWhDrbDQmC2zicUPBxsPg", service.AdditionalProperties["refreshToken"]?.ToString());
    }

    [Fact]
    public void RefreshService_SingleService_SerializesAsObject()
    {
        // Arrange
        var refreshService = new RefreshService
        {
            Id = "https://registration.provider.example/flows/reissue",
            Type = "VerifiableCredentialRefreshService2021"
        };
        refreshService.AdditionalProperties["refreshToken"] = "z2BJYfNtmWRiouWhDrbDQmC2zicUPBxsPg";

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
            RefreshService = new Collection<RefreshService> { refreshService }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"refreshService\": {", json);
        Assert.DoesNotContain("\"refreshService\": [", json);
        Assert.Contains("\"type\": \"VerifiableCredentialRefreshService2021\"", json);
        Assert.Contains("\"id\": \"https://registration.provider.example/flows/reissue\"", json);
        Assert.Contains("\"refreshToken\"", json);
    }

    [Fact]
    public void RefreshService_SingleService_RoundTrip_PreservesData()
    {
        // Arrange
        var originalService = new RefreshService
        {
            Id = "https://registration.provider.example/flows/reissue-age-token",
            Type = "VerifiableCredentialRefreshService2021"
        };
        originalService.AdditionalProperties["refreshToken"] = "z2BJYfNtmWRiouWhDrbDQmC2zicUPBxsPg";

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 4, 3, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> 
            { 
                new CredentialSubject { Id = "did:example:123" } 
            },
            RefreshService = new Collection<RefreshService> { originalService }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.RefreshService);
        Assert.Single(deserialized.RefreshService);
        Assert.Equal(original.RefreshService.First().Id, deserialized.RefreshService.First().Id);
        Assert.Equal(original.RefreshService.First().Type, deserialized.RefreshService.First().Type);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Multiple RefreshService Tests

    [Fact]
    public void RefreshService_MultipleServices_Deserializes()
    {
        // Arrange - Multiple refresh services as array
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:123""
            },
            ""refreshService"": [{
                ""type"": ""VerifiableCredentialRefreshService2021"",
                ""id"": ""https://example.com/refresh/primary""
            }, {
                ""type"": ""ManualRefreshService2018"",
                ""id"": ""https://example.com/refresh/manual""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.RefreshService);
        Assert.Equal(2, credential.RefreshService.Count);
        
        var services = credential.RefreshService.ToArray();
        
        // First service
        Assert.Equal("https://example.com/refresh/primary", services[0].Id);
        Assert.Equal("VerifiableCredentialRefreshService2021", services[0].Type);
        
        // Second service
        Assert.Equal("https://example.com/refresh/manual", services[1].Id);
        Assert.Equal("ManualRefreshService2018", services[1].Type);
    }

    [Fact]
    public void RefreshService_MultipleServices_SerializesAsArray()
    {
        // Arrange
        var service1 = new RefreshService
        {
            Id = "https://example.com/refresh/primary",
            Type = "VerifiableCredentialRefreshService2021"
        };
        
        var service2 = new RefreshService
        {
            Id = "https://example.com/refresh/manual",
            Type = "ManualRefreshService2018"
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
            RefreshService = new Collection<RefreshService> { service1, service2 }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"refreshService\": [", json);
        Assert.Contains("\"https://example.com/refresh/primary\"", json);
        Assert.Contains("\"https://example.com/refresh/manual\"", json);
    }

    [Fact]
    public void RefreshService_MultipleServices_RoundTrip()
    {
        // Arrange
        var service1 = new RefreshService 
        { 
            Id = "https://example.com/refresh/1", 
            Type = "Type1" 
        };
        var service2 = new RefreshService 
        { 
            Id = "https://example.com/refresh/2", 
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
            RefreshService = new Collection<RefreshService> { service1, service2 }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.RefreshService);
        Assert.Equal(2, deserialized.RefreshService.Count);
        Assert.Equal(json, reserializedJson);
    }

    #endregion

    #region Missing/Null RefreshService Tests

    [Fact]
    public void RefreshService_MissingInJson_IsNull()
    {
        // Arrange - refreshService is optional per spec
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
        Assert.Null(credential.RefreshService); // Optional property should be null when missing
    }

    [Fact]
    public void RefreshService_NullInJson_DeserializesToNull()
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
            ""refreshService"": null
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.Null(credential.RefreshService); // Null should deserialize to null
    }

    [Fact]
    public void RefreshService_EmptyCollection_SerializesAsOmitted()
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
            RefreshService = new Collection<RefreshService>() // Empty collection
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Converter writes null for empty collections, then JsonIgnore omits it
        Assert.DoesNotContain("\"refreshService\"", json);
    }

    [Fact]
    public void RefreshService_NullProperty_SerializesAsOmitted()
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
            RefreshService = null // Explicitly null
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Null should be omitted by JsonIgnore
        Assert.DoesNotContain("\"refreshService\"", json);
    }

    #endregion

    #region Required Properties Tests

    [Fact]
    public void RefreshService_WithoutId_FailsValidation()
    {
        // Arrange - Per spec: id is REQUIRED
        var refreshService = new RefreshService
        {
            Id = null!, // Missing required id
            Type = "VerifiableCredentialRefreshService2021"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(refreshService);
        var isValid = Validator.TryValidateObject(
            refreshService, validationContext, validationResults, validateAllProperties: true);

        // Assert - Id is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Id"));
    }

    [Fact]
    public void RefreshService_WithoutType_FailsValidation()
    {
        // Arrange - Per spec: type is REQUIRED
        var refreshService = new RefreshService
        {
            Id = "https://example.com/refresh",
            Type = null! // Missing required type
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(refreshService);
        var isValid = Validator.TryValidateObject(
            refreshService, validationContext, validationResults, validateAllProperties: true);

        // Assert - Type is required
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("Type"));
    }

    [Theory]
    [InlineData("VerifiableCredentialRefreshService2021")]
    [InlineData("ManualRefreshService2018")]
    [InlineData("CustomRefreshType")]
    public void RefreshService_WithVariousTypes_Deserializes(string serviceType)
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
            ""refreshService"": {{
                ""id"": ""https://example.com/refresh"",
                ""type"": ""{serviceType}""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.RefreshService);
        Assert.Single(credential.RefreshService);
        Assert.Equal(serviceType, credential.RefreshService.First().Type);
    }

    [Theory]
    [InlineData("https://registration.provider.example/flows/reissue-age-token")]
    [InlineData("https://example.com/api/v1/refresh")]
    [InlineData("urn:uuid:3d4f3e8a-9c7b-4f2e-8d1a-5e6f7a8b9c0d")]
    public void RefreshService_WithVariousIdFormats_Deserializes(string serviceId)
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
            ""refreshService"": {{
                ""id"": ""{serviceId}"",
                ""type"": ""VerifiableCredentialRefreshService2021""
            }}
        }}";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.RefreshService);
        Assert.Single(credential.RefreshService);
        Assert.Equal(serviceId, credential.RefreshService.First().Id);
    }

    #endregion

    #region Additional Properties Tests

    [Fact]
    public void RefreshService_WithMultipleAdditionalProperties_PreservesData()
    {
        // Arrange
        var service = new RefreshService
        {
            Id = "https://example.com/refresh",
            Type = "VerifiableCredentialRefreshService2021"
        };
        service.AdditionalProperties["refreshToken"] = "z2BJYfNtmWRiouWhDrbDQmC2zicUPBxsPg";
        service.AdditionalProperties["url"] = "https://example.com/oauth/refresh";
        service.AdditionalProperties["expiresIn"] = 3600;

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
            RefreshService = new Collection<RefreshService> { service }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.RefreshService);
        var deserializedService = deserialized.RefreshService.First();
        
        Assert.True(deserializedService.AdditionalProperties.ContainsKey("refreshToken"));
        Assert.Equal("z2BJYfNtmWRiouWhDrbDQmC2zicUPBxsPg", deserializedService.AdditionalProperties["refreshToken"]?.ToString());
        
        Assert.True(deserializedService.AdditionalProperties.ContainsKey("url"));
        Assert.Equal("https://example.com/oauth/refresh", deserializedService.AdditionalProperties["url"]?.ToString());
        
        Assert.True(deserializedService.AdditionalProperties.ContainsKey("expiresIn"));
    }

    #endregion
}