using Azure.AI.Projects;
using Microsoft.AspNetCore.Mvc;
using MyFoundryPortal.Services;
using MyFoundryPortal.ViewModels;

namespace MyFoundryPortal.Controllers;

public class ConnectionsController : Controller
{
    private readonly FoundryService _foundry;
    private readonly ILogger<ConnectionsController> _logger;

    public ConnectionsController(FoundryService foundry, ILogger<ConnectionsController> logger)
    {
        _foundry = foundry;
        _logger = logger;
    }

    // GET /Connections
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var connections = await _foundry.GetConnectionsAsync(ct);
            var viewModels = connections.Select(MapToViewModel).ToList();
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve connections");
            ViewData["Error"] = ex.Message;
            return View(new List<ConnectionViewModel>());
        }
    }

    // GET /Connections/Details/{name}
    public async Task<IActionResult> Details(string name, CancellationToken ct)
    {
        try
        {
            var connection = await _foundry.GetConnectionAsync(name, ct);
            if (connection is null)
                return NotFound();

            return View(MapToViewModel(connection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve connection {Name}", name);
            ViewData["Error"] = ex.Message;
            return View(new ConnectionViewModel { Name = name });
        }
    }

    private static ConnectionViewModel MapToViewModel(AIProjectConnection conn) => new()
    {
        Name = conn.Name,
        ConnectionType = conn.Type.ToString(),
        Target = conn.Target ?? string.Empty,
        IsDefault = conn.IsDefault,
        AuthType = conn.Credentials switch
        {
            AIProjectConnectionApiKeyCredential => "API Key",
            AIProjectConnectionEntraIdCredential => "Entra ID",
            AIProjectConnectionSasCredential => "SAS Token",
            NoAuthenticationCredentials => "None",
            null => "Unknown",
            var c => c.GetType().Name,
        },
        Id = conn.Id ?? string.Empty,
    };
}
