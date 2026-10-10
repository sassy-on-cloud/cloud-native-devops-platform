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
    public IActionResult Index()
    {
        ViewBag.Requests = _store.GetAll();

        return View(new ProvisioningRequest
        {
            Environment = "Development",
            AwsRegion = "ap-south-1",
            DeploymentPlatform = "ECS Fargate"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Provision(ProvisioningRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Requests = _store.GetAll();

            return View("Index", request);
        }

        request.RequestedAt = DateTime.UtcNow;

        _store.Add(request);

        TempData["SuccessMessage"] =
            $"Provisioning request received for {request.Environment} using {request.DeploymentPlatform} in {request.AwsRegion}.";

        return RedirectToAction(nameof(Index));
    }
}
