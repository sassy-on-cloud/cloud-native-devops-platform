using CloudOpsProvisioner.Models;
using CloudOpsProvisioner.Services;

namespace CloudOpsProvisioner.Tests;

public class ProvisioningRequestStoreTests
{
    [Fact]
    public void Add_ShouldStoreProvisioningRequest()
    {
        var store = new ProvisioningRequestStore();

        var request = new ProvisioningRequest
        {
            Environment = "Development",
            AwsRegion = "ap-south-1",
            DeploymentPlatform = "ECS Fargate"
        };

        store.Add(request);

        var requests = store.GetAll();

        Assert.Single(requests);
        Assert.Equal("Development", requests[0].Environment);
        Assert.Equal("ap-south-1", requests[0].AwsRegion);
        Assert.Equal("ECS Fargate", requests[0].DeploymentPlatform);
    }

    [Fact]
    public void Add_ShouldPlaceNewestRequestFirst()
    {
        var store = new ProvisioningRequestStore();

        var firstRequest = new ProvisioningRequest
        {
            Environment = "Development"
        };

        var secondRequest = new ProvisioningRequest
        {
            Environment = "Staging"
        };

        store.Add(firstRequest);
        store.Add(secondRequest);

        var requests = store.GetAll();

        Assert.Equal("Staging", requests[0].Environment);
        Assert.Equal("Development", requests[1].Environment);
    }
    [Fact]
    public void GetAll_ShouldReturnSnapshot()
    {
        var store = new ProvisioningRequestStore();

        store.Add(new ProvisioningRequest
        {
            Environment = "Development"
        });

        var snapshot = store.GetAll();

        store.Add(new ProvisioningRequest
        {
            Environment = "Staging"
        });

        Assert.Single(snapshot);
        Assert.Equal("Development", snapshot[0].Environment);
        Assert.Equal(2, store.GetAll().Count);
    }

    [Fact]
    public void Add_ShouldRejectNullRequest()
    {
        var store = new ProvisioningRequestStore();

        Assert.Throws<ArgumentNullException>(
            () => store.Add(null!));
    }

    [Fact]
    public void Add_ShouldHandleConcurrentRequests()
    {
        var store = new ProvisioningRequestStore();

        Parallel.For(0, 1000, i =>
        {
            store.Add(new ProvisioningRequest
            {
                Environment = $"Request-{i}"
            });
        });

        var requests = store.GetAll();

        Assert.Equal(1000, requests.Count);
        Assert.Equal(
            1000,
            requests.Select(request => request.Environment).Distinct().Count());
    }

    [Fact]
    public void ProvisioningRequest_ShouldGenerateUniqueIds()
    {
        var firstRequest = new ProvisioningRequest();
        var secondRequest = new ProvisioningRequest();

        Assert.NotEqual(firstRequest.Id, secondRequest.Id);
    }

    [Fact]
    public void ProvisioningRequest_ShouldStartWithPendingStatus()
    {
        var request = new ProvisioningRequest();

        Assert.Equal("Pending", request.Status);
    }

    [Fact]
    public void TryUpdateStatus_ShouldAllowPendingToProcessing()
    {
        var store = new ProvisioningRequestStore();
        var request = new ProvisioningRequest();

        store.Add(request);

        var updated = store.TryUpdateStatus(
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
    public void TryUpdateStatus_ShouldAllowProcessingToTerminalStatus(
        string terminalStatus)
    {
        var store = new ProvisioningRequestStore();
        var request = new ProvisioningRequest();

        store.Add(request);

        Assert.True(store.TryUpdateStatus(
            request.Id,
            ProvisioningRequestStatuses.Processing));

        Assert.True(store.TryUpdateStatus(
            request.Id,
            terminalStatus));

        Assert.Equal(terminalStatus, request.Status);
    }

    [Fact]
    public void TryUpdateStatus_ShouldRejectPendingToCompleted()
    {
        var store = new ProvisioningRequestStore();
        var request = new ProvisioningRequest();

        store.Add(request);

        var updated = store.TryUpdateStatus(
            request.Id,
            ProvisioningRequestStatuses.Completed);

        Assert.False(updated);
        Assert.Equal(
            ProvisioningRequestStatuses.Pending,
            request.Status);
    }

    [Fact]
    public void TryUpdateStatus_ShouldRejectUnknownRequest()
    {
        var store = new ProvisioningRequestStore();

        var updated = store.TryUpdateStatus(
            Guid.NewGuid(),
            ProvisioningRequestStatuses.Processing);

        Assert.False(updated);
    }

    [Fact]
    public void TryUpdateStatus_ShouldRejectTransitionFromCompleted()
    {
        var store = new ProvisioningRequestStore();
        var request = new ProvisioningRequest();

        store.Add(request);

        Assert.True(store.TryUpdateStatus(
            request.Id,
            ProvisioningRequestStatuses.Processing));

        Assert.True(store.TryUpdateStatus(
            request.Id,
            ProvisioningRequestStatuses.Completed));

        var updated = store.TryUpdateStatus(
            request.Id,
            ProvisioningRequestStatuses.Processing);

        Assert.False(updated);
        Assert.Equal(
            ProvisioningRequestStatuses.Completed,
            request.Status);
    }
}
