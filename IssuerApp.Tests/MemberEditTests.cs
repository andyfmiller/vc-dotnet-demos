namespace IssuerApp.Integration.Tests
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public class MembersPageTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public MembersPageTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task EditMember_UpdatesMemberAndRedirects()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // GET the edit page to verify it loads successfully
            var getResponse = await client.GetAsync("/Admin/Members/Edit?key=1", TestContext.Current.CancellationToken);
            getResponse.EnsureSuccessStatusCode();
            var pageContent = await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            var token = ExtractAntiforgeryToken(pageContent);

            // Prepare form data matching the fields rendered by Edit.cshtml
            var formData = new Dictionary<string, string>
            {
                { "Member.MemberKey", "1" },
                { "Member.OrganizationKey", "1" },
                { "Member.Name", "Updated Name" },
                { "__RequestVerificationToken", token }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var postResponse = await client.PostAsync("/Admin/Members/Edit?key=1", content, TestContext.Current.CancellationToken);

            // Assert – the save succeeded and redirects to Edit page with "Saved" message
            Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
            var redirectUrl = postResponse.Headers.Location?.ToString();
            Assert.NotNull(redirectUrl);
            Assert.Contains("message=Saved", redirectUrl);
        }

        [Fact]
        public async Task EditMember_DoesNotAllowEmptyName()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // GET the edit page to verify it loads successfully
            var getResponse = await client.GetAsync("/Admin/Members/Edit?key=1", TestContext.Current.CancellationToken);
            getResponse.EnsureSuccessStatusCode();
            var pageContent = await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            var token = ExtractAntiforgeryToken(pageContent);

            // Prepare form data matching the fields rendered by Edit.cshtml
            var formData = new Dictionary<string, string>
            {
                { "Member.MemberKey", "1" },
                { "Member.OrganizationKey", "1" },
                { "Member.Name", "" }, // Invalid empty Name: "The Name field is required."
                { "__RequestVerificationToken", token }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var postResponse = await client.PostAsync("/Admin/Members/Edit?key=1", content, TestContext.Current.CancellationToken);

            // Assert – The save should fail validation and return the same page with errors (not redirect).
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            var responseContent = await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("The Name field is required.", responseContent);
        }

        // Helper to extract antiforgery token from HTML (if needed)
        private string ExtractAntiforgeryToken(string html)
        {
            var match = System.Text.RegularExpressions.Regex.Match(html, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]*)""");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
