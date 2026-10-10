using System.ComponentModel.DataAnnotations;

namespace CloudOpsProvisioner.Models;

public class ProvisioningRequest
{
    [Required(ErrorMessage = "Please select an environment.")]
    [RegularExpression(
        "^(Development|Staging|Production)$",
        ErrorMessage = "Choose Development, Staging, or Production.")]
    public string Environment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an AWS region.")]
    [RegularExpression(
        "^(ap-south-1|us-east-1|eu-west-1)$",
        ErrorMessage = "Choose one of the supported AWS regions.")]
    public string AwsRegion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a deployment platform.")]
    [RegularExpression(
        "^(ECS Fargate|Amazon EKS)$",
        ErrorMessage = "Choose ECS Fargate or Amazon EKS.")]
    public string DeploymentPlatform { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
