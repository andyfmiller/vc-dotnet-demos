using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialSubjectTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialSubjectTests(ITestOutputHelper output)
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
    public void CredentialSubject_DeserializesSingleObject_FromSpec()
    {
        // Arrange - Example from VC 2.0 spec with single subject
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
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21"",
                ""type"": [""CredentialSubject""],
                ""degree"": {
                    ""type"": ""ExampleBachelorDegree"",
                    ""name"": ""Bachelor of Science and Arts""
                }
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSubject);
        Assert.Single(credential.CredentialSubject);
        
        var subject = credential.CredentialSubject.First();
        Assert.NotNull(subject);
        Assert.Equal("did:example:ebfeb1f712ebc6f1c276e12ec21", subject.Id);
        Assert.NotNull(subject.Type);
        Assert.Contains("CredentialSubject", subject.Type);
        Assert.True(subject.AdditionalProperties.ContainsKey("degree"));
    }

    [Fact]
    public void CredentialSubject_DeserializesMultipleSubjects_FromSpec()
    {
        // Arrange - Example from VC 2.0 spec with multiple subjects (spouses)
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/examples/v2""
            ],
            ""id"": ""http://university.example/credentials/3732"",
            ""type"": [""VerifiableCredential"", ""RelationshipCredential""],
            ""issuer"": ""https://issuer.example/issuer/123"",
            ""validFrom"": ""2010-01-01T00:00:00Z"",
            ""credentialSubject"": [{
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21"",
                ""type"": [""CredentialSubject""],
                ""name"": ""Jayden Doe"",
                ""spouse"": ""did:example:c276e12ec21ebfeb1f712ebc6f1""
            }, {
                ""id"": ""https://subject.example/subject/8675"",
                ""type"": [""CredentialSubject""],
                ""name"": ""Morgan Doe"",
                ""spouse"": ""https://subject.example/subject/7421""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSubject);
        Assert.Equal(2, credential.CredentialSubject.Count);
        
        var subjects = credential.CredentialSubject.ToArray();
        
        // First subject
        Assert.Equal("did:example:ebfeb1f712ebc6f1c276e12ec21", subjects[0].Id);
        Assert.True(subjects[0].AdditionalProperties.ContainsKey("name"));
        Assert.True(subjects[0].AdditionalProperties.ContainsKey("spouse"));
        
        // Second subject
        Assert.Equal("https://subject.example/subject/8675", subjects[1].Id);
        Assert.True(subjects[1].AdditionalProperties.ContainsKey("name"));
        Assert.True(subjects[1].AdditionalProperties.ContainsKey("spouse"));
    }

    [Fact]
    public void CredentialSubject_SingleSubject_SerializesAsObject()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> 
            { 
                "https://www.w3.org/ns/credentials/v2",
                "https://www.w3.org/ns/credentials/examples/v2"
            },
            Type = new Collection<string> { "VerifiableCredential", "ExampleDegreeCredential" },
            Issuer = new Issuer { Id = "https://university.example/issuers/565049" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject>
            {
                new CredentialSubject
                {
                    Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
                    Type = new Collection<string> { "CredentialSubject" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialSubject\": {", json);
        Assert.DoesNotContain("\"credentialSubject\": [", json);
        Assert.Contains("\"id\": \"did:example:ebfeb1f712ebc6f1c276e12ec21\"", json);
    }

    [Fact]
    public void CredentialSubject_MultipleSubjects_SerializesAsArray()
    {
        // Arrange
        var subject1 = new CredentialSubject
        {
            Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
            Type = new Collection<string> { "CredentialSubject" }
        };
        subject1.AdditionalProperties["name"] = "Jayden Doe";
        subject1.AdditionalProperties["spouse"] = "did:example:c276e12ec21ebfeb1f712ebc6f1";

        var subject2 = new CredentialSubject
        {
            Id = "https://subject.example/subject/8675",
            Type = new Collection<string> { "CredentialSubject" }
        };
        subject2.AdditionalProperties["name"] = "Morgan Doe";
        subject2.AdditionalProperties["spouse"] = "https://subject.example/subject/7421";

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> 
            { 
                "https://www.w3.org/ns/credentials/v2",
                "https://www.w3.org/ns/credentials/examples/v2"
            },
            Type = new Collection<string> { "VerifiableCredential", "RelationshipCredential" },
            Issuer = new Issuer { Id = "https://issuer.example/issuer/123" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { subject1, subject2 }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialSubject\": [", json);
        Assert.Contains("\"Jayden Doe\"", json);
        Assert.Contains("\"Morgan Doe\"", json);
    }

    [Fact]
    public void CredentialSubject_SingleSubject_RoundTrip_PreservesData()
    {
        // Arrange
        var originalSubject = new CredentialSubject
        {
            Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
            Type = new Collection<string> { "CredentialSubject" }
        };
        originalSubject.AdditionalProperties["degree"] = new Dictionary<string, object>
        {
            { "type", "ExampleBachelorDegree" },
            { "name", "Bachelor of Science and Arts" }
        };

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> 
            { 
                "https://www.w3.org/ns/credentials/v2",
                "https://www.w3.org/ns/credentials/examples/v2"
            },
            Type = new Collection<string> { "VerifiableCredential", "ExampleDegreeCredential" },
            Issuer = new Issuer { Id = "https://university.example/issuers/565049" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { originalSubject }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.CredentialSubject);
        Assert.Equal(original.CredentialSubject.First().Id, deserialized.CredentialSubject.First().Id);
        Assert.Equal(json, reserializedJson);
    }

    [Fact]
    public void CredentialSubject_MultipleSubjects_RoundTrip_PreservesData()
    {
        // Arrange
        var subject1 = new CredentialSubject
        {
            Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
            Type = new Collection<string> { "CredentialSubject" }
        };
        subject1.AdditionalProperties["name"] = "Jayden Doe";

        var subject2 = new CredentialSubject
        {
            Id = "https://subject.example/subject/8675",
            Type = new Collection<string> { "CredentialSubject" }
        };
        subject2.AdditionalProperties["name"] = "Morgan Doe";

        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential", "RelationshipCredential" },
            Issuer = new Issuer { Id = "https://issuer.example/issuer/123" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { subject1, subject2 }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        var reserializedJson = JsonSerializer.Serialize(deserialized, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.CredentialSubject.Count);
        Assert.Equal(json, reserializedJson);
    }

    [Fact]
    public void CredentialSubject_MustBeRequired_ValidationFails()
    {
        // Arrange - credentialSubject is REQUIRED per spec
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = null!
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credential);
        var isValid = Validator.TryValidateObject(
            credential, validationContext, validationResults, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("CredentialSubject"));
    }

    [Fact]
    public void CredentialSubject_EmptyCollection_FailsValidation()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject>()
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credential);
        var isValid = Validator.TryValidateObject(
            credential, validationContext, validationResults, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("CredentialSubject"));
    }

    [Fact]
    public void CredentialSubject_WithAdditionalClaims_SingleSubject_SerializesCorrectly()
    {
        // Arrange
        var credentialSubject = new CredentialSubject
        {
            Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
            Type = new Collection<string> { "CredentialSubject" }
        };
        credentialSubject.AdditionalProperties["degree"] = new Dictionary<string, object>
        {
            { "type", "ExampleBachelorDegree" },
            { "name", "Bachelor of Science and Arts" }
        };

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> 
            { 
                "https://www.w3.org/ns/credentials/v2",
                "https://www.w3.org/ns/credentials/examples/v2"
            },
            Type = new Collection<string> { "VerifiableCredential", "ExampleDegreeCredential" },
            Issuer = new Issuer { Id = "https://university.example/issuers/565049" },
            ValidFrom = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { credentialSubject }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"degree\"", json);
        Assert.Contains("\"ExampleBachelorDegree\"", json);
        Assert.Contains("\"Bachelor of Science and Arts\"", json);
    }

    [Fact]
    public void CredentialSubject_WithoutId_SerializesCorrectly()
    {
        // Arrange - id is optional per spec (MAY contain)
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject>
            {
                new CredentialSubject
                {
                    Type = new Collection<string> { "CredentialSubject" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialSubject\"", json);
        Assert.Contains("\"type\":", json);
    }

    [Fact]
    public void CredentialSubject_WithNestedObjects_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [
                ""https://www.w3.org/ns/credentials/v2"",
                ""https://www.w3.org/ns/credentials/examples/v2""
            ],
            ""type"": [""VerifiableCredential"", ""ExampleDegreeCredential""],
            ""issuer"": ""https://university.example/issuers/565049"",
            ""validFrom"": ""2010-01-01T00:00:00Z"",
            ""credentialSubject"": {
                ""id"": ""did:example:ebfeb1f712ebc6f1c276e12ec21"",
                ""type"": [""CredentialSubject""],
                ""degree"": {
                    ""type"": ""ExampleBachelorDegree"",
                    ""name"": ""Bachelor of Science and Arts"",
                    ""institution"": {
                        ""name"": ""Example University"",
                        ""location"": ""City, State""
                    }
                }
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSubject);
        Assert.Single(credential.CredentialSubject);
        Assert.True(credential.CredentialSubject.First().AdditionalProperties.ContainsKey("degree"));
    }

    [Theory]
    [InlineData("did:example:ebfeb1f712ebc6f1c276e12ec21")]
    [InlineData("https://subject.example/subject/123")]
    [InlineData("urn:uuid:3d4f3e8a-9c7b-4f2e-8d1a-5e6f7a8b9c0d")]
    public void CredentialSubject_WithVariousIdFormats_DeserializesCorrectly(string subjectId)
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject>
            {
                new CredentialSubject
                {
                    Id = subjectId,
                    Type = new Collection<string> { "CredentialSubject" }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(subjectId, deserialized.CredentialSubject.First().Id);
    }

    [Fact]
    public void CredentialSubject_ThreeSubjects_SerializesAsArray()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject>
            {
                new CredentialSubject { Id = "did:example:1", Type = new Collection<string> { "CredentialSubject" } },
                new CredentialSubject { Id = "did:example:2", Type = new Collection<string> { "CredentialSubject" } },
                new CredentialSubject { Id = "did:example:3", Type = new Collection<string> { "CredentialSubject" } }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.Contains("\"credentialSubject\": [", json);
        // Use null-conditional operator to avoid possible null dereference
        Assert.NotNull(deserialized?.CredentialSubject); // Ensure not null before dereferencing
        Assert.Equal(3, deserialized!.CredentialSubject.Count);
    }

    [Fact]
    public void CredentialSubject_WithBooleanAndNumericClaims_RoundTrips()
    {
        // Arrange
        var credentialSubject = new CredentialSubject
        {
            Id = "did:example:123",
            Type = new Collection<string> { "CredentialSubject" }
        };
        credentialSubject.AdditionalProperties["isActive"] = true;
        credentialSubject.AdditionalProperties["age"] = 25;
        credentialSubject.AdditionalProperties["gpa"] = 3.75;

        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = new Collection<CredentialSubject> { credentialSubject }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Single(deserialized.CredentialSubject);
        Assert.True(deserialized.CredentialSubject.First().AdditionalProperties.ContainsKey("isActive"));
        Assert.True(deserialized.CredentialSubject.First().AdditionalProperties.ContainsKey("age"));
        Assert.True(deserialized.CredentialSubject.First().AdditionalProperties.ContainsKey("gpa"));
    }

    [Fact]
    public void CredentialSubject_NullValue_SerializesAsNull()
    {
        // Arrange - Setting CredentialSubject to null
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = DateTimeOffset.UtcNow,
            CredentialSubject = null!
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        
        // Assert - With DefaultIgnoreCondition.WhenWritingNull, null properties are OMITTED
        Assert.DoesNotContain("\"credentialSubject\"", json);

        // Validation should fail
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credential);
        var isValid = Validator.TryValidateObject(
            credential, validationContext, validationResults, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("CredentialSubject"));
    }

    [Fact]
    public void CredentialSubject_NullInJson_DeserializesToEmptyCollection()
    {
        // Arrange - credentialSubject is null in JSON
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": null
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);
        
        // Assert - Deserializes null to empty collection
        Assert.NotNull(credential);
        Assert.NotNull(credential.CredentialSubject);
        Assert.Empty(credential.CredentialSubject);

        // Validation should fail
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(credential);
        var isValid = Validator.TryValidateObject(
            credential, validationContext, validationResults, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("CredentialSubject"));
    }
}