using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyFoundryPortal.Models;
using MyFoundryPortal.Services;

namespace MyFoundryPortal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly FoundryService _foundry;

    public HomeController(ILogger<HomeController> logger, FoundryService foundry)
    {
        _logger = logger;
        _foundry = foundry;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        int deploymentCount = 0;
        int agentCount = 0;
        int connectionCount = 0;

        try { deploymentCount = (await _foundry.GetDeploymentsAsync(ct)).Count; }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not load deployment count"); }

        try { agentCount = (await _foundry.GetAgentsAsync(ct)).Count; }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not load agent count"); }

        try { connectionCount = (await _foundry.GetConnectionsAsync(ct)).Count; }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not load connection count"); }

        ViewData["DeploymentCount"] = deploymentCount;
        ViewData["AgentCount"] = agentCount;
        ViewData["ConnectionCount"] = connectionCount;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

