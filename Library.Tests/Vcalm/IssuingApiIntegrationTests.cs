using Library.Vcalm.Client;
using Library.Vcalm.Client.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Moq;
using System.Text;
using System.Text.Json;

namespace Library.Tests.Vcalm;

/// <summary>
/// Integration tests for the VCALM Issuing API endpoints (https://w3c.github.io/vcalm/#issuing).
/// Tests exercise the full Kiota client stack using a mocked IRequestAdapter.
/// Covered endpoints:
///   POST   /credentials/issue
///   GET    /credentials/{id}
///   DELETE /credentials/{id}
///   POST   /credentials/status
///   POST   /status-lists
///   GET    /status-lists/{id}
/// </summary>
public class IssuingApiIntegrationTests
{
    private readonly ITestOutputHelper _output;

    private const string BaseUrl = "https://issuer.example.com";

    public IssuingApiIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a VcalmClient backed by a mocked IRequestAdapter.
    /// The client constructor registers Kiota serializers in the global registry,
    /// which are then returned by the mock's GetSerializationWriterFactory().
    /// </summary>
    private static (VcalmClient client, Mock<IRequestAdapter> mock) CreateClientWithMock()
    {
        var mock = new Mock<IRequestAdapter>();
        mock.SetupProperty(a => a.BaseUrl, BaseUrl);
        var client = new VcalmClient(mock.Object);
        mock.SetupGet(a => a.SerializationWriterFactory)
            .Returns(SerializationWriterFactoryRegistry.DefaultInstance);
        return (client, mock);
    }

    private static string ReadRequestBody(RequestInformation requestInfo)
    {
        if (requestInfo.Content is null) return string.Empty;
        requestInfo.Content.Position = 0;
        return new StreamReader(requestInfo.Content, Encoding.UTF8, leaveOpen: true).ReadToEnd();
    }

    private static UnsecuredCredential BuildUniversityDegreeCredential() => new()
    {
        Context = ["https://www.w3.org/ns/credentials/v2", "https://www.w3.org/ns/credentials/examples/v2"],
        Id = "http://example.gov/credentials/3732",
        Type = ["VerifiableCredential", "UniversityDegreeCredential"],
        ValidFrom = "2020-03-16T22:37:26.544Z"
    };

    private static VerifiableCredential BuildIssuedVerifiableCredential() => new()
    {
        Context = ["https://www.w3.org/ns/credentials/v2", "https://www.w3.org/ns/credentials/examples/v2"],
        Id = "http://example.gov/credentials/3732",
        Type = ["VerifiableCredential", "UniversityDegreeCredential"],
        IssuanceDate = "2020-03-16T22:37:26.544Z",
        Proof = new DataIntegrityProof
        {
            Type = "DataIntegrityProof",
            Cryptosuite = "ecdsa-rdfc-2019",
            Created = "2020-04-02T18:28:08Z",
            VerificationMethod = "did:example:123#z6MksHh7qHWvybLg5QTPPdG2DgEjjduBDArV9EF9mRiRzMBN",
            ProofPurpose = "assertionMethod",
            ProofValue = "zaHXrr7AQdydBk3ahpCDpWbxfLokDqmCToYm2dyWvpcFVyWooC2he63w1f7UNQoAMKdhaRtcnaE2KTo5o5vTCcfw"
        }
    };

    // =========================================================================
    // POST /credentials/issue
    // =========================================================================

    [Fact]
    public async Task IssueCredential_SendsPostToCorrectEndpoint()
    {
        var (client, mock) = CreateClientWithMock();

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<IssueCredentialResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IssueCredentialResponse());

        await client.Credentials.Issue.PostAsync(new IssueCredentialRequest
        {
            Credential = BuildUniversityDegreeCredential()
        }, cancellationToken: TestContext.Current.CancellationToken);

        mock.Verify(a => a.SendAsync(
            It.Is<RequestInformation>(r =>
                r.HttpMethod == Method.POST &&
                r.UrlTemplate == "{+baseurl}/credentials/issue"),
            It.IsAny<ParsableFactory<IssueCredentialResponse>>(),
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IssueCredential_SerializesCredentialInRequestBody()
    {
        var (client, mock) = CreateClientWithMock();
        RequestInformation? captured = null;

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<IssueCredentialResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RequestInformation, ParsableFactory<IssueCredentialResponse>,
                Dictionary<string, ParsableFactory<IParsable>>, CancellationToken>(
                (req, _, __, ___) => captured = req)
            .ReturnsAsync(new IssueCredentialResponse());

        await client.Credentials.Issue.PostAsync(new IssueCredentialRequest
        {
            Credential = BuildUniversityDegreeCredential()
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        var body = ReadRequestBody(captured);
        _output.WriteLine(body);

        using var doc = JsonDocument.Parse(body);
        var credential = doc.RootElement.GetProperty("credential");
        Assert.Equal("http://example.gov/credentials/3732", credential.GetProperty("id").GetString());
        Assert.Contains("UniversityDegreeCredential",
            credential.GetProperty("type").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public async Task IssueCredential_WithOptions_SerializesOptionsInRequestBody()
    {
        var (client, mock) = CreateClientWithMock();
        RequestInformation? captured = null;

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<IssueCredentialResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RequestInformation, ParsableFactory<IssueCredentialResponse>,
                Dictionary<string, ParsableFactory<IParsable>>, CancellationToken>(
                (req, _, __, ___) => captured = req)
            .ReturnsAsync(new IssueCredentialResponse());

        await client.Credentials.Issue.PostAsync(new IssueCredentialRequest
        {
            Credential = BuildUniversityDegreeCredential(),
            Options = new IssueCredentialOptions
            {
                CredentialId = "http://example.gov/credentials/3732"
            }
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        var body = ReadRequestBody(captured);
        _output.WriteLine(body);

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(
            "http://example.gov/credentials/3732",
            doc.RootElement.GetProperty("options").GetProperty("credentialId").GetString());
    }

    [Fact]
    public async Task IssueCredential_WithMandatoryPointers_SerializesMandatoryPointersInOptions()
    {
        // Spec: mandatoryPointers are used with selective disclosure (e.g., bbs-2023, ecdsa-sd-2023)
        var (client, mock) = CreateClientWithMock();
        RequestInformation? captured = null;

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<IssueCredentialResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RequestInformation, ParsableFactory<IssueCredentialResponse>,
                Dictionary<string, ParsableFactory<IParsable>>, CancellationToken>(
                (req, _, __, ___) => captured = req)
            .ReturnsAsync(new IssueCredentialResponse());

        await client.Credentials.Issue.PostAsync(new IssueCredentialRequest
        {
            Credential = BuildUniversityDegreeCredential(),
            Options = new IssueCredentialOptions
            {
                MandatoryPointers = ["/credentialSubject/id", "/credentialSubject/name"]
            }
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        var body = ReadRequestBody(captured);
        _output.WriteLine(body);

        using var doc = JsonDocument.Parse(body);
        var pointers = doc.RootElement.GetProperty("options").GetProperty("mandatoryPointers");
        Assert.Equal(JsonValueKind.Array, pointers.ValueKind);
        Assert.Equal(2, pointers.GetArrayLength());
    }

    [Fact]
    public async Task IssueCredential_ReturnsResponseFromAdapter()
    {
        var (client, mock) = CreateClientWithMock();
        var expected = new IssueCredentialResponse();

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<IssueCredentialResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await client.Credentials.Issue.PostAsync(new IssueCredentialRequest
        {
            Credential = BuildUniversityDegreeCredential()
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(expected, response);
    }

    // =========================================================================
    // GET /credentials/{id}
    // =========================================================================

    [Fact]
    public async Task GetCredential_SendsGetToCorrectEndpoint()
    {
        var (client, mock) = CreateClientWithMock();
        const string credentialId = "3732";

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<VerifiableCredentialResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifiableCredentialResponse());

        await client.Credentials[credentialId].GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        mock.Verify(a => a.SendAsync(
            It.Is<RequestInformation>(r =>
                r.HttpMethod == Method.GET &&
                r.UrlTemplate == "{+baseurl}/credentials/{id}"),
            It.IsAny<ParsableFactory<VerifiableCredentialResponse>>(),
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCredential_ReturnsVerifiableCredentialResponse()
    {
        var (client, mock) = CreateClientWithMock();
        var expected = new VerifiableCredentialResponse
        {
            VerifiableCredential = new VerifiableCredentialResponse.VerifiableCredentialResponse_verifiableCredential
            {
                VerifiableCredential = BuildIssuedVerifiableCredential()
            }
        };

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<VerifiableCredentialResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await client.Credentials["3732"].GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.NotNull(response.VerifiableCredential?.VerifiableCredential);
        Assert.Equal("http://example.gov/credentials/3732",
            response.VerifiableCredential.VerifiableCredential.Id);
    }

    // =========================================================================
    // DELETE /credentials/{id}
    // =========================================================================

    [Fact]
    public async Task DeleteCredential_SendsDeleteToCorrectEndpoint()
    {
        var (client, mock) = CreateClientWithMock();
        const string credentialId = "3732";

        mock.Setup(a => a.SendNoContentAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await client.Credentials[credentialId].DeleteAsync(cancellationToken: TestContext.Current.CancellationToken);

        mock.Verify(a => a.SendNoContentAsync(
            It.Is<RequestInformation>(r =>
                r.HttpMethod == Method.DELETE &&
                r.UrlTemplate == "{+baseurl}/credentials/{id}"),
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteCredential_CompletesWithoutException()
    {
        var (client, mock) = CreateClientWithMock();

        mock.Setup(a => a.SendNoContentAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var exception = await Record.ExceptionAsync(() =>
            client.Credentials["3732"].DeleteAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    // =========================================================================
    // POST /credentials/status
    // =========================================================================

    [Fact]
    public async Task UpdateCredentialStatus_SendsPostToCorrectEndpoint()
    {
        var (client, mock) = CreateClientWithMock();

        mock.Setup(a => a.SendPrimitiveAsync<Stream>(
                It.IsAny<RequestInformation>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        await client.Credentials.Status.PostAsync(new UpdateCredentialStatusRequest
        {
            CredentialId = "http://example.gov/credentials/3732",
            Status = true
        }, cancellationToken: TestContext.Current.CancellationToken);

        mock.Verify(a => a.SendPrimitiveAsync<Stream>(
            It.Is<RequestInformation>(r =>
                r.HttpMethod == Method.POST &&
                r.UrlTemplate == "{+baseurl}/credentials/status"),
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCredentialStatus_SerializesStatusUpdateInBody()
    {
        // Spec: credentialId identifies the credential; credentialStatus identifies the status list entry
        var (client, mock) = CreateClientWithMock();
        RequestInformation? captured = null;

        mock.Setup(a => a.SendPrimitiveAsync<Stream>(
                It.IsAny<RequestInformation>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RequestInformation, Dictionary<string, ParsableFactory<IParsable>>, CancellationToken>(
                (req, _, __) => captured = req)
            .ReturnsAsync((Stream?)null);

        await client.Credentials.Status.PostAsync(new UpdateCredentialStatusRequest
        {
            CredentialId = "http://example.gov/credentials/3732",
            Status = true,
            CredentialStatus = new UpdateCredentialStatusRequest_credentialStatus
            {
                Type = "BitstringStatusListEntry",
                StatusPurpose = "revocation",
                StatusListIndex = "94567",
                StatusListCredential = "https://example.com/credentials/status/3"
            }
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        var body = ReadRequestBody(captured);
        _output.WriteLine(body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.Equal("http://example.gov/credentials/3732", root.GetProperty("credentialId").GetString());
        Assert.True(root.GetProperty("status").GetBoolean());
        var cs = root.GetProperty("credentialStatus");
        Assert.Equal("BitstringStatusListEntry", cs.GetProperty("type").GetString());
        Assert.Equal("revocation", cs.GetProperty("statusPurpose").GetString());
        Assert.Equal("94567", cs.GetProperty("statusListIndex").GetString());
    }

    // =========================================================================
    // POST /status-lists
    // =========================================================================

    [Fact]
    public async Task CreateStatusList_SendsPostToCorrectEndpoint()
    {
        var (client, mock) = CreateClientWithMock();

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<CreateStatusListResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateStatusListResponse());

        await client.StatusLists.PostAsync(new CreateStatusListRequest
        {
            StatusPurpose = "revocation"
        }, cancellationToken: TestContext.Current.CancellationToken);

        mock.Verify(a => a.SendAsync(
            It.Is<RequestInformation>(r =>
                r.HttpMethod == Method.POST &&
                r.UrlTemplate == "{+baseurl}/status-lists"),
            It.IsAny<ParsableFactory<CreateStatusListResponse>>(),
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateStatusList_SerializesRequestBody()
    {
        // Spec: statusPurpose is "revocation" or "suspension"
        var (client, mock) = CreateClientWithMock();
        RequestInformation? captured = null;

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<CreateStatusListResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .Callback<RequestInformation, ParsableFactory<CreateStatusListResponse>,
                Dictionary<string, ParsableFactory<IParsable>>, CancellationToken>(
                (req, _, __, ___) => captured = req)
            .ReturnsAsync(new CreateStatusListResponse());

        await client.StatusLists.PostAsync(new CreateStatusListRequest
        {
            Id = "https://example.com/credentials/status/3",
            StatusPurpose = "revocation"
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        var body = ReadRequestBody(captured);
        _output.WriteLine(body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.Equal("https://example.com/credentials/status/3", root.GetProperty("id").GetString());
        Assert.Equal("revocation", root.GetProperty("statusPurpose").GetString());
    }

    [Fact]
    public async Task CreateStatusList_ReturnsStatusListResponse()
    {
        var (client, mock) = CreateClientWithMock();
        var expected = new CreateStatusListResponse
        {
            Id = "https://example.com/credentials/status/3",
            VerifiableCredential = BuildIssuedVerifiableCredential()
        };

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<CreateStatusListResponse>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await client.StatusLists.PostAsync(new CreateStatusListRequest
        {
            StatusPurpose = "revocation"
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal("https://example.com/credentials/status/3", response.Id);
        Assert.NotNull(response.VerifiableCredential);
    }

    // =========================================================================
    // GET /status-lists/{id}
    // =========================================================================

    [Fact]
    public async Task GetStatusList_SendsGetToCorrectEndpoint()
    {
        var (client, mock) = CreateClientWithMock();
        const string statusListId = "3";

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<VerifiableCredential>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerifiableCredential());

        await client.StatusLists[statusListId].GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        mock.Verify(a => a.SendAsync(
            It.Is<RequestInformation>(r =>
                r.HttpMethod == Method.GET &&
                r.UrlTemplate == "{+baseurl}/status-lists/{id}"),
            It.IsAny<ParsableFactory<VerifiableCredential>>(),
            It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetStatusList_ReturnsStatusListCredential()
    {
        // Spec: the status list credential is a VerifiableCredential with BitstringStatusList type
        var (client, mock) = CreateClientWithMock();
        var expected = new VerifiableCredential
        {
            Context = [
                "https://www.w3.org/ns/credentials/v2",
                "https://www.w3.org/ns/credentials/examples/v2"
            ],
            Id = "https://example.com/credentials/status/3",
            Type = ["VerifiableCredential", "BitstringStatusListCredential"],
            Proof = new DataIntegrityProof
            {
                Type = "DataIntegrityProof",
                Cryptosuite = "ecdsa-rdfc-2019",
                ProofPurpose = "assertionMethod"
            }
        };

        mock.Setup(a => a.SendAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<ParsableFactory<VerifiableCredential>>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var response = await client.StatusLists["3"].GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal("https://example.com/credentials/status/3", response.Id);
        Assert.Contains("BitstringStatusListCredential", response.Type!);
        Assert.Equal("ecdsa-rdfc-2019", response.Proof!.Cryptosuite);
    }
}
