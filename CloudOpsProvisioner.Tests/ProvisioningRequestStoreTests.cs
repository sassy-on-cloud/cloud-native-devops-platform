using CloudOpsProvisioner.Data;
using CloudOpsProvisioner.Models;
using CloudOpsProvisioner.Services;
using Microsoft.EntityFrameworkCore;

namespace CloudOpsProvisioner.Tests;

public class ProvisioningRequestStoreTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldStoreProvisioningRequest()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);
        var request = new ProvisioningRequest
        {
            Environment = "Development",
            AwsRegion = "ap-south-1",
            DeploymentPlatform = "ECS Fargate"
        };

        await store.AddAsync(request);

        var requests = await store.GetAllAsync();

        Assert.Single(requests);
        Assert.Equal("Development", requests[0].Environment);
        Assert.Equal("ap-south-1", requests[0].AwsRegion);
        Assert.Equal("ECS Fargate", requests[0].DeploymentPlatform);
    }

    [Fact]
    public async Task GetAllAsync_ShouldPlaceNewestRequestFirst()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);

        var first = new ProvisioningRequest
        {
            Environment = "Development",
            RequestedAt = DateTime.UtcNow.AddMinutes(-1)
        };
        var second = new ProvisioningRequest
        {
            Environment = "Staging",
            RequestedAt = DateTime.UtcNow
        };

        await store.AddAsync(first);
        await store.AddAsync(second);

        var requests = await store.GetAllAsync();

        Assert.Equal("Staging", requests[0].Environment);
        Assert.Equal("Development", requests[1].Environment);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnIndependentListSnapshot()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);

        await store.AddAsync(new ProvisioningRequest
        {
            Environment = "Development"
        });

        var snapshot = await store.GetAllAsync();

        await store.AddAsync(new ProvisioningRequest
        {
            Environment = "Staging"
        });

        Assert.Single(snapshot);
        Assert.Equal(2, (await store.GetAllAsync()).Count);
    }

    [Fact]
    public async Task AddAsync_ShouldRejectNullRequest()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => store.AddAsync(null!));
    }

    [Fact]
    public async Task AddAsync_ShouldStoreManyUniqueRequests()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);

        for (var i = 0; i < 100; i++)
        {
            await store.AddAsync(new ProvisioningRequest
            {
                Environment = $"Request-{i}"
            });
        }

        var requests = await store.GetAllAsync();

        Assert.Equal(100, requests.Count);
        Assert.Equal(
            100,
            requests.Select(request => request.Environment)
                .Distinct()
                .Count());
    }

    [Fact]
    public void ProvisioningRequest_ShouldGenerateUniqueIds()
    {
        var first = new ProvisioningRequest();
        var second = new ProvisioningRequest();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void ProvisioningRequest_ShouldStartWithPendingStatus()
    {
        var request = new ProvisioningRequest();

        Assert.Equal(ProvisioningRequestStatuses.Pending, request.Status);
    }

    [Fact]
    public async Task TryUpdateStatusAsync_ShouldAllowPendingToProcessing()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);
        var request = new ProvisioningRequest();

        await store.AddAsync(request);

        var updated = await store.TryUpdateStatusAsync(
            request.Id,
            ProvisioningRequestStatuses.Processing);

        Assert.True(updated);
        Assert.Equal(
            ProvisioningRequestStatuses.Processing,
            request.Status);
    }

    [Theory]
    [InlineData(ProvisioningRequestStatuses.Completed)]
    [InlineData(ProvisioningRequestStatuses.Failed)]
    public async Task TryUpdateStatusAsync_ShouldAllowProcessingToTerminalStatus(
        string terminalStatus)
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);
        var request = new ProvisioningRequest();

        await store.AddAsync(request);

        Assert.True(await store.TryUpdateStatusAsync(
            request.Id,
            ProvisioningRequestStatuses.Processing));

        Assert.True(await store.TryUpdateStatusAsync(
            request.Id,
            terminalStatus));

        Assert.Equal(terminalStatus, request.Status);
    }

    [Fact]
    public async Task TryUpdateStatusAsync_ShouldRejectPendingToCompleted()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);
        var request = new ProvisioningRequest();

        await store.AddAsync(request);

        var updated = await store.TryUpdateStatusAsync(
            request.Id,
            ProvisioningRequestStatuses.Completed);

        Assert.False(updated);
        Assert.Equal(ProvisioningRequestStatuses.Pending, request.Status);
    }

    [Fact]
    public async Task TryUpdateStatusAsync_ShouldRejectUnknownRequest()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);

        var updated = await store.TryUpdateStatusAsync(
            Guid.NewGuid(),
            ProvisioningRequestStatuses.Processing);

        Assert.False(updated);
    }

    [Fact]
    public async Task TryUpdateStatusAsync_ShouldRejectTransitionFromCompleted()
    {
        using var context = CreateContext();
        var store = new ProvisioningRequestStore(context);
        var request = new ProvisioningRequest();

        await store.AddAsync(request);

        Assert.True(await store.TryUpdateStatusAsync(
            request.Id,
            ProvisioningRequestStatuses.Processing));

        Assert.True(await store.TryUpdateStatusAsync(
            request.Id,
            ProvisioningRequestStatuses.Completed));

        var updated = await store.TryUpdateStatusAsync(
            request.Id,
            ProvisioningRequestStatuses.Processing);

        Assert.False(updated);
        Assert.Equal(ProvisioningRequestStatuses.Completed, request.Status);
    }
}
