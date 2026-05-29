using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace WalletApp.Integration.Tests;

public class HolderPageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HolderPageTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task IndexPage_ReturnsSuccess()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminHoldersPage_ReturnsSuccess()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Admin/Holders", TestContext.Current.CancellationToken);

        // Expect OK or redirect — not a server error
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Unexpected status: {response.StatusCode}");
    }
}
