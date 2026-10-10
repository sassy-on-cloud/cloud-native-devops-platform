using System.ComponentModel.DataAnnotations;
using CloudOpsProvisioner.Models;

namespace CloudOpsProvisioner.Tests;

public class ProvisioningRequestValidationTests
{
    [Fact]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = new ProvisioningRequest
        {
            Environment = "Development",
            AwsRegion = "ap-south-1",
            DeploymentPlatform = "ECS Fargate"
        };

        var errors = Validate(request);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("Production;unexpected", "ap-south-1", "ECS Fargate")]
    [InlineData("Development", "invalid-region", "ECS Fargate")]
    [InlineData("Development", "ap-south-1", "Unknown Platform")]
    public void UnsupportedValues_ShouldFailValidation(
        string environment,
        string region,
        string platform)
    {
        var request = new ProvisioningRequest
        {
            Environment = environment,
            AwsRegion = region,
            DeploymentPlatform = platform
        };

        var errors = Validate(request);

        Assert.NotEmpty(errors);
    }

    [Theory]
    [InlineData(null, "ap-south-1", "ECS Fargate")]
    [InlineData("Development", null, "ECS Fargate")]
    [InlineData("Development", "ap-south-1", null)]
    public void MissingValues_ShouldFailValidation(
        string? environment,
        string? region,
        string? platform)
    {
        var request = new ProvisioningRequest
        {
            Environment = environment!,
            AwsRegion = region!,
            DeploymentPlatform = platform!
        };

        var errors = Validate(request);

        Assert.NotEmpty(errors);
    }

    private static List<ValidationResult> Validate(ProvisioningRequest request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);

        Validator.TryValidateObject(
            request,
            context,
            results,
            validateAllProperties: true);

        return results;
    }
}
