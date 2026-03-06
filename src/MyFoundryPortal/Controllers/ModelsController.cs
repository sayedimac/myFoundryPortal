using Azure.AI.Projects;
using Microsoft.AspNetCore.Mvc;
using MyFoundryPortal.Services;
using MyFoundryPortal.ViewModels;

namespace MyFoundryPortal.Controllers;

public class ModelsController : Controller
{
    private readonly FoundryService _foundry;
    private readonly ILogger<ModelsController> _logger;

    public ModelsController(FoundryService foundry, ILogger<ModelsController> logger)
    {
        _foundry = foundry;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var deployments = await _foundry.GetDeploymentsAsync(ct);
            var viewModels = deployments.Select(d => new DeploymentViewModel
            {
                Name = d.Name,
                ModelName = d.ModelName,
                ModelVersion = d.ModelVersion,
                ModelPublisher = d.ModelPublisher,
                ConnectionName = d.ConnectionName,
                SkuName = d.Sku?.Name ?? string.Empty,
                SkuCapacity = d.Sku?.Capacity ?? 0,
                Capabilities = d.Capabilities ?? new Dictionary<string, string>(),
            }).ToList();

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve deployments");
            ViewData["Error"] = ex.Message;
            return View(new List<DeploymentViewModel>());
        }
    }

    public async Task<IActionResult> Details(string name, CancellationToken ct)
    {
        try
        {
            var deployment = await _foundry.GetDeploymentAsync(name, ct);
            if (deployment is null)
                return NotFound();

            if (deployment is ModelDeployment md)
            {
                var vm = new DeploymentViewModel
                {
                    Name = md.Name,
                    ModelName = md.ModelName,
                    ModelVersion = md.ModelVersion,
                    ModelPublisher = md.ModelPublisher,
                    ConnectionName = md.ConnectionName,
                    SkuName = md.Sku?.Name ?? string.Empty,
                    SkuCapacity = md.Sku?.Capacity ?? 0,
                    Capabilities = md.Capabilities ?? new Dictionary<string, string>(),
                };
                return View(vm);
            }

            return View(new DeploymentViewModel { Name = deployment.Name });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve deployment {Name}", name);
            ViewData["Error"] = ex.Message;
            return View(new DeploymentViewModel { Name = name });
        }
    }
}
