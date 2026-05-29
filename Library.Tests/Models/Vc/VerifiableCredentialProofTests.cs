using Library.Models.Vc;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Library.Tests.Models2.Vc;

public class VerifiableCredentialProofTests
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITestOutputHelper _output;

    public VerifiableCredentialProofTests(ITestOutputHelper output)
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
    public void Proof_WithSingleProof_DeserializesCorrectly()
    {
        // Arrange - Example 1: Simple signed JSON-LD data document
        var json = @"{
            ""@context"": [
                {""myWebsite"": ""https://vocabulary.example/myWebsite""},
                ""https://w3id.org/security/data-integrity/v2""
            ],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://ldi.example/issuer"",
            ""validFrom"": ""2020-06-11T19:14:04Z"",
            ""credentialSubject"": {""myWebsite"": ""https://hello.world.example/""},
            ""proof"": {
                ""type"": ""DataIntegrityProof"",
                ""cryptosuite"": ""ecdsa-rdfc-2019"",
                ""created"": ""2020-06-11T19:14:04Z"",
                ""verificationMethod"": ""https://ldi.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP"",
                ""proofPurpose"": ""assertionMethod"",
                ""proofValue"": ""zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Proof);
        Assert.Single(credential.Proof);
        
        var proof = credential.Proof.First();
        Assert.Equal("DataIntegrityProof", proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", proof.Cryptosuite);
        Assert.Equal("assertionMethod", proof.ProofPurpose);
        Assert.Equal("https://ldi.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP", proof.VerificationMethod);
        Assert.Equal("zXb23ZkdakfJNUhiTEdwyE598X7RLrkjnXEADLQZ7vZyUGXX8cyJZRBkNw813SGsJHWrcpo4Y8hRJ7adYn35Eetq", proof.ProofValue);
        Assert.Equal(new DateTimeOffset(2020, 6, 11, 19, 14, 4, TimeSpan.Zero), proof.Created);
    }

    [Fact]
    public void Proof_WithExpiresProperty_DeserializesCorrectly()
    {
        // Arrange - Example 2: Data document with expires property
        var json = @"{
            ""@context"": [
                {""myWebsite"": ""https://vocabulary.example/myWebsite""},
                ""https://w3id.org/security/data-integrity/v2""
            ],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://ldi.example/issuer"",
            ""validFrom"": ""2020-06-11T19:14:04Z"",
            ""credentialSubject"": {""myWebsite"": ""https://hello.world.example/""},
            ""proof"": {
                ""type"": ""DataIntegrityProof"",
                ""cryptosuite"": ""ecdsa-rdfc-2019"",
                ""created"": ""2020-06-11T19:14:04Z"",
                ""expires"": ""2020-07-11T19:14:04Z"",
                ""verificationMethod"": ""https://ldi.example/issuer#zDnaepBuvsQ8cpsWrVKw8fbpGpvPeNSjVPTWoq6cRqaYzBKVP"",
                ""proofPurpose"": ""assertionMethod"",
                ""proofValue"": ""z98X7RLrkjnXEADJNUhiTEdwyE5GXX8cyJZRLQZ7vZyUXb23ZkdakfRJ7adYY8hn35EetqBkNw813SGsJHWrcpo4""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Proof);
        Assert.Single(credential.Proof);
        
        var proof = credential.Proof.First();
        Assert.Equal("DataIntegrityProof", proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", proof.Cryptosuite);
        Assert.Equal(new DateTimeOffset(2020, 6, 11, 19, 14, 4, TimeSpan.Zero), proof.Created);
        Assert.Equal(new DateTimeOffset(2020, 7, 11, 19, 14, 4, TimeSpan.Zero), proof.Expires);
    }

    [Fact]
    public void Proof_WithProofSet_DeserializesCorrectly()
    {
        // Arrange - Example 3: Proof set in a data document
        var json = @"{
            ""@context"": [
                {""myWebsite"": ""https://vocabulary.example/myWebsite""},
                ""https://w3id.org/security/data-integrity/v2""
            ],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://ldi.example/issuer"",
            ""validFrom"": ""2020-11-05T19:23:24Z"",
            ""credentialSubject"": {""myWebsite"": ""https://hello.world.example/""},
            ""proof"": [{
                ""type"": ""DataIntegrityProof"",
                ""cryptosuite"": ""eddsa-rdfc-2022"",
                ""created"": ""2020-11-05T19:23:24Z"",
                ""verificationMethod"": ""https://ldi.example/issuer/1#z6MkjLrk3gKS2nnkeWcmcxiZPGskmesDpuwRBorgHxUXfxnG"",
                ""proofPurpose"": ""assertionMethod"",
                ""proofValue"": ""z4oey5q2M3XKaxup3tmzN4DRFTLVqpLMweBrSxMY2xHX5XTYVQeVbY8nQAVHMrXFkXJpmEcqdoDwLWxaqA3Q1geV6""
            }, {
                ""type"": ""DataIntegrityProof"",
                ""cryptosuite"": ""eddsa-rdfc-2022"",
                ""created"": ""2020-11-05T13:08:49Z"",
                ""verificationMethod"": ""https://pfps.example/issuer/2#z6MkGskxnGjLrk3gKS2mesDpuwRBokeWcmrgHxUXfnncxiZP"",
                ""proofPurpose"": ""assertionMethod"",
                ""proofValue"": ""z5QLBrp19KiWXerb8ByPnAZ9wujVFN8PDsxxXeMoyvDqhZ6Qnzr5CG9876zNht8BpStWi8H2Mi7XCY3inbLrZrm95""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Proof);
        Assert.Equal(2, credential.Proof.Count);

        var proofArray = credential.Proof.ToArray();
        
        // First proof
        Assert.Equal("DataIntegrityProof", proofArray[0].Type);
        Assert.Equal("eddsa-rdfc-2022", proofArray[0].Cryptosuite);
        Assert.Equal(new DateTimeOffset(2020, 11, 5, 19, 23, 24, TimeSpan.Zero), proofArray[0].Created);
        Assert.Equal("https://ldi.example/issuer/1#z6MkjLrk3gKS2nnkeWcmcxiZPGskmesDpuwRBorgHxUXfxnG", proofArray[0].VerificationMethod);
        
        // Second proof
        Assert.Equal("DataIntegrityProof", proofArray[1].Type);
        Assert.Equal("eddsa-rdfc-2022", proofArray[1].Cryptosuite);
        Assert.Equal(new DateTimeOffset(2020, 11, 5, 13, 8, 49, TimeSpan.Zero), proofArray[1].Created);
        Assert.Equal("https://pfps.example/issuer/2#z6MkGskxnGjLrk3gKS2mesDpuwRBokeWcmrgHxUXfnncxiZP", proofArray[1].VerificationMethod);
    }

    [Fact]
    public void Proof_WithProofChain_DeserializesCorrectly()
    {
        // Arrange - Example 4: Proof chain in a data document
        var json = @"{
            ""@context"": [
                {""myWebsite"": ""https://vocabulary.example/myWebsite""},
                ""https://w3id.org/security/data-integrity/v2""
            ],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://ldi.example/issuer"",
            ""validFrom"": ""2020-11-05T19:23:42Z"",
            ""credentialSubject"": {""myWebsite"": ""https://hello.world.example/""},
            ""proof"": [{
                ""id"": ""urn:uuid:60102d04-b51e-11ed-acfe-2fcd717666a7"",
                ""type"": ""DataIntegrityProof"",
                ""cryptosuite"": ""eddsa-rdfc-2022"",
                ""created"": ""2020-11-05T19:23:42Z"",
                ""verificationMethod"": ""https://ldi.example/issuer/1#z6MkjLrk3gKS2nnkeWcmcxiZPGskmesDpuwRBorgHxUXfxnG"",
                ""proofPurpose"": ""assertionMethod"",
                ""proofValue"": ""zVbY8nQAVHMrXFkXJpmEcqdoDwLWxaqA3Q1geV64oey5q2M3XKaxup3tmzN4DRFTLVqpLMweBrSxMY2xHX5XTYVQe""
            }, {
                ""type"": ""DataIntegrityProof"",
                ""cryptosuite"": ""eddsa-rdfc-2022"",
                ""created"": ""2020-11-05T21:28:14Z"",
                ""verificationMethod"": ""https://pfps.example/issuer/2#z6MkGskxnGjLrk3gKS2mesDpuwRBokeWcmrgHxUXfnncxiZP"",
                ""proofPurpose"": ""assertionMethod"",
                ""proofValue"": ""z6Qnzr5CG9876zNht8BpStWi8H2Mi7XCY3inbLrZrm955QLBrp19KiWXerb8ByPnAZ9wujVFN8PDsxxXeMoyvDqhZ"",
                ""previousProof"": ""urn:uuid:60102d04-b51e-11ed-acfe-2fcd717666a7""
            }]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Proof);
        Assert.Equal(2, credential.Proof.Count);

        var proofArray = credential.Proof.ToArray();
        
        // First proof (root of chain)
        Assert.Equal("urn:uuid:60102d04-b51e-11ed-acfe-2fcd717666a7", proofArray[0].Id);
        Assert.Equal("DataIntegrityProof", proofArray[0].Type);
        Assert.Equal("eddsa-rdfc-2022", proofArray[0].Cryptosuite);
        Assert.Null(proofArray[0].PreviousProof);
        
        // Second proof (linked to first via previousProof)
        Assert.Equal("DataIntegrityProof", proofArray[1].Type);
        Assert.Equal("eddsa-rdfc-2022", proofArray[1].Cryptosuite);
        Assert.Equal("urn:uuid:60102d04-b51e-11ed-acfe-2fcd717666a7", proofArray[1].PreviousProof);
        Assert.Equal(new DateTimeOffset(2020, 11, 5, 21, 28, 14, TimeSpan.Zero), proofArray[1].Created);
    }

    [Fact]
    public void Proof_SingleProof_SerializesAsObject()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() },
            Proof = new Collection<DataIntegrityProof>
            {
                new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    Cryptosuite = "ecdsa-rdfc-2019",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z123456",
                    Created = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"proof\":", json);
        Assert.Contains("\"type\": \"DataIntegrityProof\"", json);
        Assert.DoesNotContain("\"proof\": [", json); // Should be object, not array
    }

    [Fact]
    public void Proof_MultipleProofs_SerializesAsArray()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() },
            Proof = new Collection<DataIntegrityProof>
            {
                new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    Cryptosuite = "ecdsa-rdfc-2019",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z123456"
                },
                new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    Cryptosuite = "eddsa-rdfc-2022",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z789012"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.Contains("\"proof\": [", json);
        Assert.Contains("\"ecdsa-rdfc-2019\"", json);
        Assert.Contains("\"eddsa-rdfc-2022\"", json);
    }

    [Fact]
    public void Proof_NullValue_SerializesAsNull()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() },
            Proof = null
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert
        Assert.DoesNotContain("\"proof\"", json); // Should be omitted due to WhenWritingNull
    }

    [Fact]
    public void Proof_EmptyCollection_SerializesAsNull()
    {
        // Arrange
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);

        // Assert - Empty collection should return null from getter, which gets omitted
        Assert.DoesNotContain("\"proof\"", json);
    }

    [Fact]
    public void Proof_SingleProof_RoundTrip_PreservesValues()
    {
        // Arrange
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() },
            Proof = new Collection<DataIntegrityProof>
            {
                new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    Cryptosuite = "ecdsa-rdfc-2019",
                    ProofPurpose = "assertionMethod",
                    VerificationMethod = "https://example.com/key#1",
                    ProofValue = "z123456",
                    Created = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Proof);
        Assert.Single(deserialized.Proof);
        
        var proof = deserialized.Proof.First();
        Assert.Equal("DataIntegrityProof", proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", proof.Cryptosuite);
        Assert.Equal("assertionMethod", proof.ProofPurpose);
        Assert.Equal("https://example.com/key#1", proof.VerificationMethod);
        Assert.Equal("z123456", proof.ProofValue);
    }

    [Fact]
    public void Proof_MultipleProofs_RoundTrip_PreservesOrder()
    {
        // Arrange
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() },
            Proof = new Collection<DataIntegrityProof>
            {
                new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    Cryptosuite = "ecdsa-rdfc-2019",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z123456"
                },
                new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    Cryptosuite = "eddsa-rdfc-2022",
                    ProofPurpose = "authentication",
                    ProofValue = "z789012"
                },
                new DataIntegrityProof
                {
                    Type = "DataIntegrityProof",
                    Cryptosuite = "ecdsa-sd-2023",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z345678"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Proof);
        Assert.Equal(3, deserialized.Proof.Count);
        
        var proofArray = deserialized.Proof.ToArray();
        Assert.Equal("ecdsa-rdfc-2019", proofArray[0].Cryptosuite);
        Assert.Equal("eddsa-rdfc-2022", proofArray[1].Cryptosuite);
        Assert.Equal("ecdsa-sd-2023", proofArray[2].Cryptosuite);
    }

    [Fact]
    public void Proof_WithOptionalProperties_DeserializesCorrectly()
    {
        // Arrange - Proof with optional properties: domain, challenge, nonce
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {},
            ""proof"": {
                ""type"": ""DataIntegrityProof"",
                ""cryptosuite"": ""ecdsa-rdfc-2019"",
                ""created"": ""2024-01-01T00:00:00Z"",
                ""verificationMethod"": ""https://example.com/key#1"",
                ""proofPurpose"": ""assertionMethod"",
                ""proofValue"": ""z123456"",
                ""domain"": ""example.com"",
                ""challenge"": ""1235abcd6789"",
                ""nonce"": ""ruby""
            }
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Proof);
        
        var proof = credential.Proof.First();
        Assert.Equal("example.com", proof.Domain);
        Assert.Equal("1235abcd6789", proof.Challenge);
        Assert.Equal("ruby", proof.Nonce);
    }

    [Fact]
    public void Proof_WithAllProperties_RoundTrips()
    {
        // Arrange - Test all properties of DataIntegrityProof
        var original = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() },
            Proof = new Collection<DataIntegrityProof>
            {
                new DataIntegrityProof
                {
                    Id = "urn:uuid:12345",
                    Type = "DataIntegrityProof",
                    Cryptosuite = "ecdsa-rdfc-2019",
                    ProofPurpose = "assertionMethod",
                    VerificationMethod = "https://example.com/key#1",
                    ProofValue = "z123456",
                    Created = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Expires = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero),
                    Domain = "example.com",
                    Challenge = "challenge123",
                    Nonce = "nonce456",
                    PreviousProof = "urn:uuid:previous"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Proof);
        
        var proof = deserialized.Proof.First();
        Assert.Equal("urn:uuid:12345", proof.Id);
        Assert.Equal("DataIntegrityProof", proof.Type);
        Assert.Equal("ecdsa-rdfc-2019", proof.Cryptosuite);
        Assert.Equal("assertionMethod", proof.ProofPurpose);
        Assert.Equal("https://example.com/key#1", proof.VerificationMethod);
        Assert.Equal("z123456", proof.ProofValue);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), proof.Created);
        Assert.Equal(new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero), proof.Expires);
        Assert.Equal("example.com", proof.Domain);
        Assert.Equal("challenge123", proof.Challenge);
        Assert.Equal("nonce456", proof.Nonce);
        Assert.Equal("urn:uuid:previous", proof.PreviousProof);
    }

    [Fact]
    public void Proof_WithDifferentCryptosuites_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {},
            ""proof"": [
                {
                    ""type"": ""DataIntegrityProof"",
                    ""cryptosuite"": ""ecdsa-rdfc-2019"",
                    ""proofPurpose"": ""assertionMethod"",
                    ""proofValue"": ""z1""
                },
                {
                    ""type"": ""DataIntegrityProof"",
                    ""cryptosuite"": ""eddsa-rdfc-2022"",
                    ""proofPurpose"": ""assertionMethod"",
                    ""proofValue"": ""z2""
                },
                {
                    ""type"": ""DataIntegrityProof"",
                    ""cryptosuite"": ""ecdsa-sd-2023"",
                    ""proofPurpose"": ""assertionMethod"",
                    ""proofValue"": ""z3""
                }
            ]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Proof);
        Assert.Equal(3, credential.Proof.Count);
        
        var cryptosuites = credential.Proof.Select(p => p.Cryptosuite).ToArray();
        Assert.Contains("ecdsa-rdfc-2019", cryptosuites);
        Assert.Contains("eddsa-rdfc-2022", cryptosuites);
        Assert.Contains("ecdsa-sd-2023", cryptosuites);
    }

    [Fact]
    public void Proof_ProofChain_ValidatesCorrectly()
    {
        // Arrange - Create a proof chain
        var credential = new VerifiableCredential<CredentialSubject, Issuer>
        {
            Context = new Collection<object> { "https://www.w3.org/ns/credentials/v2" },
            Type = new Collection<string> { "VerifiableCredential" },
            Issuer = new Issuer { Id = "https://example.com/issuer" },
            ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CredentialSubject = new Collection<CredentialSubject> { new CredentialSubject() },
            Proof = new Collection<DataIntegrityProof>
            {
                new DataIntegrityProof
                {
                    Id = "urn:uuid:proof1",
                    Type = "DataIntegrityProof",
                    Cryptosuite = "eddsa-rdfc-2022",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z1"
                },
                new DataIntegrityProof
                {
                    Id = "urn:uuid:proof2",
                    Type = "DataIntegrityProof",
                    Cryptosuite = "eddsa-rdfc-2022",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z2",
                    PreviousProof = "urn:uuid:proof1"
                },
                new DataIntegrityProof
                {
                    Id = "urn:uuid:proof3",
                    Type = "DataIntegrityProof",
                    Cryptosuite = "eddsa-rdfc-2022",
                    ProofPurpose = "assertionMethod",
                    ProofValue = "z3",
                    PreviousProof = "urn:uuid:proof2"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(credential, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert - Verify the chain is intact
        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.Proof);
        Assert.Equal(3, deserialized.Proof.Count);
        
        var proofArray = deserialized.Proof.ToArray();
        Assert.Null(proofArray[0].PreviousProof); // First in chain
        Assert.Equal("urn:uuid:proof1", proofArray[1].PreviousProof);
        Assert.Equal("urn:uuid:proof2", proofArray[2].PreviousProof);
    }

    [Fact]
    public void Proof_WithDifferentProofPurposes_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""@context"": [""https://www.w3.org/ns/credentials/v2""],
            ""type"": [""VerifiableCredential""],
            ""issuer"": ""https://example.com/issuer"",
            ""validFrom"": ""2024-01-01T00:00:00Z"",
            ""credentialSubject"": {},
            ""proof"": [
                {
                    ""type"": ""DataIntegrityProof"",
                    ""cryptosuite"": ""eddsa-rdfc-2022"",
                    ""proofPurpose"": ""assertionMethod"",
                    ""proofValue"": ""z1""
                },
                {
                    ""type"": ""DataIntegrityProof"",
                    ""cryptosuite"": ""eddsa-rdfc-2022"",
                    ""proofPurpose"": ""authentication"",
                    ""proofValue"": ""z2""
                }
            ]
        }";

        // Act
        var credential = JsonSerializer.Deserialize<VerifiableCredential<CredentialSubject, Issuer>>(json, _jsonOptions);

        // Assert
        Assert.NotNull(credential);
        Assert.NotNull(credential.Proof);
        Assert.Equal(2, credential.Proof.Count);
        
        var purposes = credential.Proof.Select(p => p.ProofPurpose).ToArray();
        Assert.Contains("assertionMethod", purposes);
        Assert.Contains("authentication", purposes);
    }

    [Fact]
    public void Proof_MissingFromJson_DeserializesAsNull()
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
        Assert.Null(credential.Proof);
    }
}