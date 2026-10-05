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

        return View(new ProvisioningRequest());
    }

    [HttpPost]
    public IActionResult Provision(ProvisioningRequest request)
    {
        request.RequestedAt = DateTime.UtcNow;

        _store.Add(request);

        TempData["SuccessMessage"] =
            $"Provisioning request received for {request.Environment} using {request.DeploymentPlatform} in {request.AwsRegion}.";

        return RedirectToAction(nameof(Index));
    }
}
