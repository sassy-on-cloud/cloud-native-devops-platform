namespace CloudOpsProvisioner.Models;

public class ProvisioningRequest
{
    public string Environment { get; set; } = "Development";

    public string AwsRegion { get; set; } = "ap-south-1";

    public string DeploymentPlatform { get; set; } = "ECS Fargate";

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
