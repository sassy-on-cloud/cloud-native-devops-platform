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
}
