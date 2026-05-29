using Library.OpenBadges.Client;
using Library.OpenBadges.Client.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Library.Tests.OpenBadges;

public class OpenBadgesClientTests
{
    private const string BaseUrl = "https://example.org/ims/ob/v3p0";

    [Fact]
    public async Task GetAsync_WithValidResponse_ReturnsCredentials()
    {
        var response = new GetOpenBadgeCredentialsResponse
        {
            Credential = new List<AchievementCredential>
            {
                CreateSampleCredential("cred-1"),
                CreateSampleCredential("cred-2")
            }
        };
        var client = CreateClient(Serialize(response), HttpStatusCode.OK);

        var result = await client.Credentials.GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Credential!.Count);
        Assert.Equal("https://example.org/credentials/cred-1", result.Credential.First().Id);
    }

    [Fact]
    public async Task GetAsync_WithPaginationParams_IncludesQueryParameters()
    {
        var response = new GetOpenBadgeCredentialsResponse
        {
            Credential = new List<AchievementCredential> { CreateSampleCredential("cred-1") }
        };
        HttpRequestMessage? captured = null;
        var client = CreateClient(Serialize(response), HttpStatusCode.OK, req => captured = req);

        await client.Credentials.GetAsync(cfg =>
        {
            cfg.QueryParameters.Limit = 25;
            cfg.QueryParameters.Offset = 50;
            cfg.QueryParameters.Since = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        var query = captured.RequestUri?.Query;
        Assert.Contains("limit=25", query);
        Assert.Contains("offset=50", query);
        Assert.Contains("since=", query);
        Assert.Contains("2024-01-01", query);
    }

    [Fact]
    public async Task GetAsync_WithJwtCredentials_ReturnsCompactJwsStrings()
    {
        var response = new GetOpenBadgeCredentialsResponse
        {
            CompactJwsString = new List<string> { "token1", "token2" }
        };
        var client = CreateClient(Serialize(response), HttpStatusCode.OK);

        var result = await client.Credentials.GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.CompactJwsString!.Count);
    }

    [Fact]
    public async Task GetAsync_With400BadRequest_ThrowsImsx_StatusInfo()
    {
        var error = new { imsx_codeMajor = "failure", imsx_severity = "error", imsx_description = "Bad request" };
        var client = CreateClient(JsonSerializer.Serialize(error), HttpStatusCode.BadRequest);

        var ex = await Assert.ThrowsAsync<Imsx_StatusInfo>(() => client.Credentials.GetAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(400, ex.ResponseStatusCode);
    }

    [Fact]
    public async Task GetAsync_With401Unauthorized_ThrowsImsx_StatusInfo()
    {
        var error = new { imsx_codeMajor = "failure", imsx_severity = "error", imsx_description = "Unauthorized" };
        var client = CreateClient(JsonSerializer.Serialize(error), HttpStatusCode.Unauthorized);

        var ex = await Assert.ThrowsAsync<Imsx_StatusInfo>(() => client.Credentials.GetAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(401, ex.ResponseStatusCode);
    }

    [Fact]
    public async Task GetAsync_With500ServerError_ThrowsImsx_StatusInfo()
    {
        var error = new { imsx_codeMajor = "failure", imsx_severity = "error", imsx_description = "Server error" };
        var client = CreateClient(JsonSerializer.Serialize(error), HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<Imsx_StatusInfo>(() => client.Credentials.GetAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(500, ex.ResponseStatusCode);
    }

    [Fact]
    public async Task PostAsync_WithNewCredential_Returns201Created()
    {
        var credential = CreateSampleCredential("new-cred");
        var client = CreateClient(Serialize(credential), HttpStatusCode.Created);

        var result = await client.Credentials.PostAsync(credential, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("https://example.org/credentials/new-cred", result.Id);
    }

    [Fact]
    public async Task PostAsync_WithExistingCredential_Returns200OK()
    {
        var credential = CreateSampleCredential("existing-cred");
        var client = CreateClient(Serialize(credential), HttpStatusCode.OK);

        var result = await client.Credentials.PostAsync(credential, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("https://example.org/credentials/existing-cred", result.Id);
    }

    [Fact]
    public async Task PostAsync_WithNullBody_ThrowsArgumentNullException()
    {
        var client = CreateClient("{}", HttpStatusCode.OK);

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.Credentials.PostAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PostAsync_SerializesCredentialAsJson()
    {
        var credential = CreateSampleCredential("cred-1");
        string? capturedBody = null;
        var client = CreateClient(Serialize(credential), HttpStatusCode.Created, async req =>
        {
            if (req.Content != null)
                capturedBody = await req.Content.ReadAsStringAsync();
        });

        await client.Credentials.PostAsync(credential, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(capturedBody);
        Assert.Contains("credentialSubject", capturedBody);
    }

    [Fact]
    public async Task GetProfileAsync_WithValidResponse_ReturnsProfile()
    {
        var profile = CreateSampleProfile();
        var client = CreateClient(Serialize(profile), HttpStatusCode.OK);

        var result = await client.Profile.GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("https://example.org/issuers/123", result.Id);
        Assert.Equal("Example Issuer", result.Name);
    }

    [Fact]
    public async Task GetProfileAsync_With404NotFound_ThrowsImsx_StatusInfo()
    {
        var error = new { imsx_codeMajor = "failure", imsx_severity = "error", imsx_description = "Not found" };
        var client = CreateClient(JsonSerializer.Serialize(error), HttpStatusCode.NotFound);

        var ex = await Assert.ThrowsAsync<Imsx_StatusInfo>(() => client.Profile.GetAsync(cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(404, ex.ResponseStatusCode);
    }

    [Fact]
    public async Task PutProfileAsync_WithValidProfile_ReturnsUpdatedProfile()
    {
        var profile = CreateSampleProfile();
        var client = CreateClient(Serialize(profile), HttpStatusCode.OK);

        var result = await client.Profile.PutAsync(profile, cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(profile.Id, result.Id);
    }

    [Fact]
    public async Task PutProfileAsync_WithNullBody_ThrowsArgumentNullException()
    {
        var client = CreateClient("{}", HttpStatusCode.OK);

        await Assert.ThrowsAsync<ArgumentNullException>(() => client.Profile.PutAsync(null!, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetServiceDescriptionAsync_WithValidResponse_ReturnsDocument()
    {
        var doc = new ServiceDescriptionDocument { Openapi = "3.0" };
        var client = CreateClient(Serialize(doc), HttpStatusCode.OK);

        var result = await client.Discovery.GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("3.0", result.Openapi);
    }

    [Fact]
    public async Task GetAsync_IncludesAuthorizationHeader_WhenSetOnHttpClient()
    {
        var response = new GetOpenBadgeCredentialsResponse { Credential = new List<AchievementCredential>() };
        HttpRequestMessage? captured = null;
        var http = new HttpClient(new MockHttpHandler(Serialize(response), HttpStatusCode.OK, req => captured = req))
        {
            BaseAddress = new Uri(BaseUrl)
        };
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token-123");
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: http);
        adapter.BaseUrl = BaseUrl;
        var client = new OpenBadgesClient(adapter);

        await client.Credentials.GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        var auth = captured.Headers.Authorization ?? http.DefaultRequestHeaders.Authorization;
        Assert.NotNull(auth);
        Assert.Equal("Bearer", auth.Scheme);
    }

    [Fact]
    public async Task GetAsync_DeserializesIso8601ValidFrom()
    {
        var json = @"{""credential"":[{""@context"":[""https://www.w3.org/ns/credentials/v2""],""type"":[""VerifiableCredential"",""OpenBadgeCredential""],""id"":""https://example.org/credentials/123"",""issuer"":""https://example.org/issuers/1"",""validFrom"":""2024-06-15T14:30:00Z"",""credentialSubject"":{""type"":[""AchievementSubject""],""achievement"":{""id"":""https://example.org/achievements/1"",""type"":[""Achievement""],""name"":""Test"",""description"":""Test"",""criteria"":{}}}}]}";
        var client = CreateClient(json, HttpStatusCode.OK);

        var result = await client.Credentials.GetAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Single(result.Credential!);
        Assert.Equal(new DateTimeOffset(2024, 6, 15, 14, 30, 0, TimeSpan.Zero), result.Credential!.First().ValidFrom);
    }

    private static OpenBadgesClient CreateClient(string json, HttpStatusCode status, Action<HttpRequestMessage>? capture = null)
    {
        var http = new HttpClient(new MockHttpHandler(json, status, capture)) { BaseAddress = new Uri(BaseUrl) };
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: http);
        adapter.BaseUrl = BaseUrl;
        return new OpenBadgesClient(adapter);
    }

    private static OpenBadgesClient CreateClient(string json, HttpStatusCode status, Func<HttpRequestMessage, Task> asyncCapture)
    {
        var http = new HttpClient(new MockHttpHandler(json, status, asyncCapture)) { BaseAddress = new Uri(BaseUrl) };
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: http);
        adapter.BaseUrl = BaseUrl;
        return new OpenBadgesClient(adapter);
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

    private static AchievementCredential CreateSampleCredential(string id) => new()
    {
        Context = new List<string> { "https://www.w3.org/ns/credentials/v2", "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json" },
        Type = new List<string> { "VerifiableCredential", "OpenBadgeCredential" },
        Id = $"https://example.org/credentials/{id}",
        ValidFrom = DateTimeOffset.UtcNow,
        CredentialSubject = new AchievementSubject
        {
            Type = new List<string> { "AchievementSubject" },
            Achievement = new Achievement
            {
                Id = "https://example.org/achievements/1",
                Type = new List<string> { "Achievement" },
                Name = "Test Achievement",
                Description = "A test achievement",
                Criteria = new Criteria()
            }
        }
    };

    private static Profile CreateSampleProfile() => new()
    {
        Id = "https://example.org/issuers/123",
        Type = new List<string> { "Profile" },
        Name = "Example Issuer",
        Description = "An example issuer for testing",
        Email = "contact@example.org"
    };
}

public class MockHttpHandler : HttpMessageHandler
{
    private readonly string _json;
    private readonly HttpStatusCode _statusCode;
    private readonly Action<HttpRequestMessage>? _capture;
    private readonly Func<HttpRequestMessage, Task>? _asyncCapture;

    public MockHttpHandler(string json, HttpStatusCode statusCode, Action<HttpRequestMessage>? capture = null)
    {
        _json = json;
        _statusCode = statusCode;
        _capture = capture;
    }

    public MockHttpHandler(string json, HttpStatusCode statusCode, Func<HttpRequestMessage, Task> asyncCapture)
    {
        _json = json;
        _statusCode = statusCode;
        _asyncCapture = asyncCapture;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _capture?.Invoke(request);
        if (_asyncCapture != null)
            await _asyncCapture(request);

        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_json, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };
    }
}
