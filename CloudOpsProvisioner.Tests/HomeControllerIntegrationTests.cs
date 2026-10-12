using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using CloudOpsProvisioner.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CloudOpsProvisioner.Tests;

public class HomeControllerIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HomeControllerIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<AppDbContext>();
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.RemoveAll<
                        IDbContextOptionsConfiguration<AppDbContext>>();

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(
                            Guid.NewGuid().ToString()));
                });
            });

        _client = _factory.CreateClient();
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
        Assert.Equal("application/json",
            response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());

        var databaseCheck = root.GetProperty("checks")
            .EnumerateArray()
            .Single(check =>
                check.GetProperty("name").GetString() == "postgresql");

        Assert.Equal("Healthy",
            databaseCheck.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsUnhealthy_WhenDatabaseIsUnavailable()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:DefaultConnection"] =
                                "Host=127.0.0.1;Port=1;" +
                                "Database=cloudops_test;" +
                                "Username=test;Password=test;" +
                                "Timeout=1;Command Timeout=1;Pooling=false"
                        });
                });
            });

        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal("Unhealthy", root.GetProperty("status").GetString());

        var databaseCheck = root.GetProperty("checks")
            .EnumerateArray()
            .Single(check =>
                check.GetProperty("name").GetString() == "postgresql");

        Assert.Equal("Unhealthy",
            databaseCheck.GetProperty("status").GetString());
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
    using (var scope = _factory.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<CloudOpsProvisioner.Data.AppDbContext>();

        Assert.Empty(dbContext.ProvisioningRequests);
    }    
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
