# Cloud-Native DevOps Platform

A hands-on cloud-native platform built to demonstrate how a containerized application can be developed, provisioned, secured, and deployed using modern DevOps practices.

The project is being developed incrementally, with each stage solving a practical deployment or infrastructure problem rather than simply adding technologies.

---

## Project Goal

The goal is to build a repeatable deployment platform where application code can move from a developer commit to a running AWS workload through an automated and secure delivery pipeline.

The project focuses on:

- Infrastructure as Code
- Containerization
- Automated CI/CD
- Secure AWS authentication
- Container orchestration
- Application availability
- Monitoring and observability
- Cloud security

---

## Current Architecture

```text
Developer
    |
    | git push
    v
GitHub Repository
    |
    v
GitHub Actions
    |
    | OIDC
    v
AWS IAM Role
    |
    +-------------------+
    |                   |
    v                   v
Docker Build        Amazon ECR
    |                   |
    +--------->---------+
              |
              v
       Amazon ECS Fargate
              |
              v
   Application Load Balancer
              |
              v
      CloudOps Provisioner
```
## Application

The current application is CloudOps Provisioner, an ASP.NET Core MVC application that provides a simple interface for submitting infrastructure provisioning requests.
Current Functionality
- Provisioning request form
- Environment selection
- AWS region selection
- Deployment platform selection
- Request history
- Platform status information
- In-memory request storage
- Automated unit tests

## Application Stack
- ASP.NET Core MVC
- .NET 10
- C#
- Docker

## AWS Infrastructure
The AWS infrastructure is provisioned using Terraform.

## Current Resources
- Custom VPC: 10.20.0.0/16
- Two public subnets across two Availability Zones
- Internet Gateway
- Public route table
- Amazon ECR repository
- Application Load Balancer
- ALB security group
- ECS security group
- ECS Fargate cluster
- ECS Fargate service
- ECS task definition
- CloudWatch log group

The current Fargate deployment uses public subnets with public IP assignment to avoid introducing a NAT Gateway and its additional cost during the learning project.

## Infrastructure as Code

Terraform manages the AWS infrastructure so that the environment can be reproduced consistently rather than created manually through the AWS Console.

Terraform is currently responsible for:

- Networking
- Amazon ECR
- Load balancing
- Amazon ECS
- Security groups
- CloudWatch logging
- Resource naming and tagging

## CI/CD Pipeline

The project includes an automated GitHub Actions deployment pipeline.

A push to the main branch triggers the deployment workflow.

## Pipeline Flow

```text
Git Push
   |
   v
GitHub Actions
   |
   v
OIDC Authentication
   |
   v
AWS IAM Role
   |
   v
Docker Image Build
   |
   v
Amazon ECR
   |
   | Image tagged with Git commit SHA
   v
ECS Task Definition Revision
   |
   v
ECS Fargate Service
   |
   v
ALB Health Check
   |
   v
Running Application
```

## Secure AWS Authentication

The workflow does not store long-lived AWS access keys in GitHub.

Instead, GitHub Actions uses OpenID Connect (OIDC) to assume a dedicated AWS IAM role.

The IAM trust relationship is restricted to:

- This GitHub repository
- The main branch
- GitHub's OIDC provider
The deployment role follows a least-privilege approach, granting only the AWS permissions required by the deployment pipeline.

## CI/CD Deployment Process
The deployment pipeline performs the following steps:
1. Checks out the application source code.
2. Authenticates to AWS using GitHub Actions OIDC.
3. Logs in to Amazon ECR.
4. Builds the Docker image.
5. Tags the image using the Git commit SHA.
6. Pushes the image to Amazon ECR.
7. Retrieves the current ECS task definition.
8. Updates the task definition with the new container image.
9. Registers a new ECS task definition revision.
10. Deploys the revision to the ECS Fargate service.
11. Waits for ECS service stability.
12. Verifies that the application is healthy behind the Application Load Balancer.

## Deployment Verification
The first automated deployment successfully completed through GitHub Actions.

## Verified Results
- GitHub Actions deployment: successful
- AWS OIDC authentication: successful
- Docker image pushed to ECR: successful
- ECS task definition revision: 2
- Desired ECS tasks: 1
- Running ECS tasks: 1
- Pending ECS tasks: 0
- ALB target health: healthy
- Public application response: HTTP 200
This demonstrates an end-to-end automated deployment from a Git commit to a healthy ECS workload.

## CI/CD Troubleshooting
During implementation, the deployment pipeline encountered and resolved several real-world AWS and GitHub Actions integration issues.

## OIDC Trust Policy
The initial GitHub Actions authentication failed because the repository used GitHub's immutable OIDC subject format for newer repositories.

The IAM trust policy was updated to use the repository and immutable repository/owner identifiers, after which OIDC authentication succeeded.

## ECR Permissions
The pipeline initially lacked permission to describe the ECR repository.

The deployment role was updated with the required ecr:DescribeRepositories permission.

## ECS Permissions
The pipeline also encountered an ECS permission issue when retrieving the task definition.

The IAM policy was refined by separating ECS read permissions from the service update permission.

These troubleshooting steps demonstrated practical IAM debugging and least-privilege policy design rather than relying on broad administrator permissions.

## Security
Security is being incorporated throughout the project rather than added only at the end.

## Current Security Practices
- GitHub Actions OIDC authentication
- No long-lived AWS credentials stored in GitHub
- Dedicated IAM role for CI/CD
- Least-privilege deployment permissions
- Separate ALB and ECS security groups
- ECS application traffic restricted to the ALB security group
- ECR image scanning on push
- Sensitive files excluded through .gitignore
Future security improvements will include AWS WAF and additional application-level security controls.

## Evidence
Project screenshots are stored in the screenshots directory.

## Application and Docker
- [Application running](./screenshots/02-application-running.png)
- [CloudOps dashboard](./screenshots/03-cloudops-dashboard.png)
- [Provisioning request](./screenshots/04-provisioning-request.png)
- [Unit tests passing](./screenshots/06-unit-tests-passing.png)
- [Docker image built](./screenshots/07-docker-image-built.png)
- [Containerized application](./screenshots/08-containerized-app.png)

## Terraform and AWS Infrastructure
- [Terraform VPC](./screenshots/09-terraform-vpc-created.png)
- [Terraform ECR](./screenshots/10-terraform-ecr-created.png)
- [Two-AZ network](./screenshots/11-terraform-two-az-network.png)
- [ALB security group](./screenshots/12-alb-security-group.png)
- [Terraform target group](./screenshots/13-terraform-target-group.png)
- [Application Load Balancer](./screenshots/14-terraform-application-load-balancer.png)
- [ALB HTTP listener](./screenshots/15-terraform-alb-http-listener.png)
- [ECS cluster](./screenshots/16-terraform-ecs-cluster.png)
- [ECS security group](./screenshots/17-ecs-security-group.png)
- [ECS Fargate application](./screenshots/18-ecs-fargate-application-running.png)

## CI/CD
- [GitHub Actions deployment success](./screenshots/19-github-actions-deployment-success.png)
- [Application deployed through CI/CD](./screenshots/20-cicd-deployed-application.png)

## Project Roadmap
The project is being developed in stages.

## Completed
- [x] ASP.NET Core application
- [x] Automated unit testing
- [x] Docker containerization
- [x] Terraform AWS networking
- [x] Amazon ECR
- [x] Application Load Balancer
- [x] ECS Fargate deployment
- [x] GitHub Actions CI/CD
- [x] GitHub Actions OIDC authentication
- [x] Least-privilege deployment IAM
- [x] Deployment health verification

## Planned
- [ ] Environment-aware deployments
- [ ] Improved application security
- [ ] AWS WAF
- [ ] Kubernetes / Amazon EKS deployment
- [ ] Prometheus monitoring
- [ ] Grafana dashboards
- [ ] Application and infrastructure observability
- [ ] ECS vs EKS comparison
- [ ] Final production-style cloud-native architecture

## Technology Stack
| Category | Technologies |
|---|---|
| Application | ASP.NET Core MVC, .NET 10, C# |
| Containerization | Docker |
| Source Control | Git, GitHub |
| CI/CD | GitHub Actions |
| Cloud | AWS |
| Infrastructure as Code | Terraform |
| Container Registry | Amazon ECR |
| Container Platform | Amazon ECS Fargate |
| Load Balancing | Application Load Balancer |
| Networking | Amazon VPC, Subnets, Internet Gateway, Route Tables |
| Security | AWS IAM, Security Groups, GitHub OIDC |
| Logging | Amazon CloudWatch |
| Planned Monitoring | Prometheus, Grafana |
| Planned Security | AWS WAF |
| Planned Orchestration | Amazon EKS |

## What This Project Demonstrates
This project is intended as a practical demonstration of:
- AWS cloud infrastructure
- Terraform / Infrastructure as Code
- Docker
- Amazon ECR
- Amazon ECS Fargate
- Application Load Balancing
- GitHub Actions
- OIDC-based AWS authentication
- IAM least privilege
- Linux and cloud deployment concepts
- CI/CD troubleshooting
- Cloud-native architecture
- Security-aware infrastructure design
- Monitoring and observability concepts
The implementation is intentionally incremental so that each stage can be built, tested, documented, and verified before moving to the next architectural capability.

## Project Status
## Current stage: CI/CD with AWS ECS Fargate

The application is currently deployed to AWS ECS Fargate behind an Application Load Balancer, with GitHub Actions handling automated image builds and deployments.

The next stages will extend the platform with stronger security, environment-aware deployments, Kubernetes, and monitoring to create a more complete cloud-native DevOps platform.

## Learning Approach
This project is being developed as a progressive Cloud and DevOps learning project.

Each stage is designed around a practical engineering problem:

```text
Application
     |
     v
Containerization
     |
     v
Infrastructure as Code
     |
     v
Cloud Deployment
     |
     v
CI/CD Automation
     |
     v
Security
     |
     v
Kubernetes
     |
     v
Monitoring & Observability
```

The objective is not simply to demonstrate individual tools, but to understand how they work together to create a repeatable and maintainable cloud-native deployment platform.


## Security Hardening

Implemented and verified application-level HTTP security headers:

- `X-Content-Type-Options: nosniff`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy` restricts camera, microphone, and geolocation access
- `Content-Security-Policy` restricts resource loading and form submission
- `X-Frame-Options: SAMEORIGIN`

### Verification

- Application builds successfully.
- Automated tests: 2 passed, 0 failed.
- Public application returned HTTP 200 OK.
- Security headers verified through the AWS Application Load Balancer.
- Existing health endpoint and ECS deployment pipeline retained.

### Security Notes

The application currently uses HTTP through the public load balancer.
HTTPS with a valid TLS certificate remains a separate implementation task.
Security headers complement, but do not replace, transport encryption,
input validation, authorization, and infrastructure security controls.

### Evidence

- `screenshots/24-public-security-headers.png`
