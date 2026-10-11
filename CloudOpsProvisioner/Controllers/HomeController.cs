using CloudOpsProvisioner.Models;
using CloudOpsProvisioner.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudOpsProvisioner.Controllers;

public class HomeController : Controller
{
    private readonly ProvisioningRequestStore _store;

    public HomeController(ProvisioningRequestStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        ViewBag.Requests = await _store.GetAllAsync(cancellationToken);

        return View(new ProvisioningRequest
        {
            Environment = "Development",
            AwsRegion = "ap-south-1",
            DeploymentPlatform = "ECS Fargate"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Provision(
        ProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Requests = await _store.GetAllAsync(cancellationToken);

            return View("Index", request);
        }

        request.RequestedAt = DateTime.UtcNow;

        await _store.AddAsync(request, cancellationToken);

        TempData["SuccessMessage"] =
            $"Provisioning request received for {request.Environment} using {request.DeploymentPlatform} in {request.AwsRegion}.";

        return RedirectToAction(nameof(Index));
    }
}
