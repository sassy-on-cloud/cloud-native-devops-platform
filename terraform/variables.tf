variable "aws_region" {
  description = "AWS region where the CloudOps platform will be deployed"
  type        = string
  default     = "ap-south-1"
}

variable "project_name" {
  description = "Project name used for AWS resource naming and tagging"
  type        = string
  default     = "cloud-native-devops-platform"
}

variable "environment" {
  description = "Deployment environment"
  type        = string
  default     = "learning"
}
