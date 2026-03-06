using Microsoft.AspNetCore.Mvc;
using MyFoundryPortal.Services;
using MyFoundryPortal.ViewModels;

namespace MyFoundryPortal.Controllers;

public class AgentsController : Controller
{
    private readonly FoundryService _foundry;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(FoundryService foundry, ILogger<AgentsController> logger)
    {
        _foundry = foundry;
        _logger = logger;
    }

    // GET /Agents
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var agents = await _foundry.GetAgentsAsync(ct);
            var viewModels = agents.Select(a => new AgentViewModel
            {
                Id = a.Id,
                Name = a.Name ?? string.Empty,
                Description = a.Description ?? string.Empty,
                Model = a.Model,
                Instructions = a.Instructions ?? string.Empty,
                CreatedAt = a.CreatedAt,
            }).ToList();

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve agents");
            ViewData["Error"] = ex.Message;
            return View(new List<AgentViewModel>());
        }
    }

    // GET /Agents/Create
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new CreateAgentViewModel();
        await PopulateAvailableModelsAsync(vm, ct);
        return View(vm);
    }

    // POST /Agents/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAgentViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAvailableModelsAsync(vm, ct);
            return View(vm);
        }

        try
        {
            await _foundry.CreateAgentAsync(
                model: vm.Model,
                name: vm.Name,
                instructions: vm.Instructions,
                description: string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description,
                ct: ct);

            TempData["Success"] = $"Agent '{vm.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create agent '{Name}'", vm.Name);
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateAvailableModelsAsync(vm, ct);
            return View(vm);
        }
    }

    // POST /Agents/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        try
        {
            await _foundry.DeleteAgentAsync(id, ct);
            TempData["Success"] = "Agent deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete agent {Id}", id);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET /Agents/Playground/{id}
    public async Task<IActionResult> Playground(string id, string? threadId, CancellationToken ct)
    {
        try
        {
            var agent = await _foundry.GetAgentAsync(id, ct);
            if (agent is null)
                return NotFound();

            var vm = new ChatViewModel
            {
                AgentId = agent.Id,
                AgentName = agent.Name ?? agent.Id,
                AgentModel = agent.Model,
                AgentInstructions = agent.Instructions ?? string.Empty,
                ThreadId = threadId,
                Messages = [],
            };

            // Load previous thread messages when a threadId is provided
            if (!string.IsNullOrWhiteSpace(threadId))
            {
                try
                {
                    var history = await _foundry.GetThreadMessagesAsync(threadId, ct);
                    vm.Messages = history
                        .Select(m => new ChatMessageViewModel { Role = m.Role, Text = m.Text })
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load thread history for thread {ThreadId}", threadId);
                }
            }

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load playground for agent {Id}", id);
            ViewData["Error"] = ex.Message;
            return View(new ChatViewModel { AgentId = id });
        }
    }

    // POST /Agents/Chat
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Chat(string agentId, string userMessage, string? threadId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return RedirectToAction(nameof(Playground), new { id = agentId, threadId });

        try
        {
            var (newThreadId, _) = await _foundry.ChatWithThreadAsync(agentId, userMessage, threadId, ct);
            return RedirectToAction(nameof(Playground), new { id = agentId, threadId = newThreadId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to chat with agent {AgentId}", agentId);
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Playground), new { id = agentId, threadId });
        }
    }

    private async Task PopulateAvailableModelsAsync(CreateAgentViewModel vm, CancellationToken ct)
    {
        try
        {
            var deployments = await _foundry.GetDeploymentsAsync(ct);
            vm.AvailableModels = deployments.Select(d => d.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not enumerate deployments for model list");
            vm.AvailableModels = [];
        }
    }
}
