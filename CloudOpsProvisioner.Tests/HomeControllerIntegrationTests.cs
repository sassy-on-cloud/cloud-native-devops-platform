using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CloudOpsProvisioner.Tests;

public class HomeControllerIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HomeControllerIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HomePage_ContainsSecurityHeaders()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal("nosniff",
            response.Headers.GetValues("X-Content-Type-Options").Single());

        Assert.Equal("strict-origin-when-cross-origin",
            response.Headers.GetValues("Referrer-Policy").Single());

        Assert.Equal("camera=(), microphone=(), geolocation=()",
            response.Headers.GetValues("Permissions-Policy").Single());

        Assert.Equal("SAMEORIGIN",
            response.Headers.GetValues("X-Frame-Options").Single());

        Assert.Contains(
            "default-src 'self'",
            response.Headers.GetValues("Content-Security-Policy").Single());
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ProvisionPost_WithoutAntiForgeryToken_IsRejected()
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Environment"] = "Development",
            ["AwsRegion"] = "ap-south-1",
            ["DeploymentPlatform"] = "ECS Fargate"
        });

        var response = await _client.PostAsync("/Home/Provision", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProvisionPost_WithInvalidValues_ShowsValidationErrors()
    {
        // GET the form first so the client receives the anti-forgery cookie.
        var pageResponse = await _client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, pageResponse.StatusCode);

        var html = await pageResponse.Content.ReadAsStringAsync();

        var tokenTag = Regex.Match(
            html,
            "<input\\b[^>]*name=\"__RequestVerificationToken\"[^>]*>",
            RegexOptions.IgnoreCase);

        Assert.True(tokenTag.Success,
            "The form should contain an anti-forgery token.");

        var tokenValue = Regex.Match(
            tokenTag.Value,
            "value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);

        Assert.True(tokenValue.Success,
            "The anti-forgery token should have a value.");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(
                tokenValue.Groups[1].Value),
            ["Environment"] = "InvalidEnvironment",
            ["AwsRegion"] = "invalid-region",
            ["DeploymentPlatform"] = "InvalidPlatform"
        });

        var response = await _client.PostAsync("/Home/Provision", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseHtml = await response.Content.ReadAsStringAsync();

        Assert.Contains("Choose Development, Staging, or Production.",
            responseHtml);
        Assert.Contains("Choose one of the supported AWS regions.",
            responseHtml);
        Assert.Contains("Choose ECS Fargate or Amazon EKS.",
            responseHtml);
    }
}
