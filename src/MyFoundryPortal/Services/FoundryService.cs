using System.Diagnostics;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace MyFoundryPortal.Services;

public class FoundryServiceOptions
{
    public string ProjectEndpoint { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class FoundryService
{
    private readonly AIProjectClient _projectClient;
    private readonly PersistentAgentsClient _agentsClient;
    private readonly ILogger<FoundryService> _logger;
    private readonly bool _configured;

    private void EnsureConfigured()
    {
        if (!_configured)
            throw new InvalidOperationException(
                "Azure AI Foundry is not configured. " +
                "Set AZURE_AI_PROJECT_ENDPOINT (and optionally AZURE_TENANT_ID / AZURE_CLIENT_ID / AZURE_CLIENT_SECRET).");
    }

    public FoundryService(IOptions<FoundryServiceOptions> options, ILogger<FoundryService> logger)
    {
        _logger = logger;
        var opts = options.Value;

        if (string.IsNullOrWhiteSpace(opts.ProjectEndpoint))
        {
            _logger.LogWarning(
                "FoundryPortal:ProjectEndpoint is not configured. " +
                "Set the AZURE_AI_PROJECT_ENDPOINT environment variable or the FoundryPortal:ProjectEndpoint config value.");
            // Clients remain null; every public method will throw NotConfiguredException.
            _projectClient = null!;
            _agentsClient = null!;
            _configured = false;
            return;
        }

        TokenCredential credential = BuildCredential(opts);

        _projectClient = new AIProjectClient(new Uri(opts.ProjectEndpoint), credential);
        _agentsClient = new PersistentAgentsClient(opts.ProjectEndpoint, credential);
        _configured = true;
    }

    private static TokenCredential BuildCredential(FoundryServiceOptions opts)
    {
        // Use ClientSecretCredential when all three app-registration values are provided;
        // otherwise fall back to DefaultAzureCredential (managed identity / CLI / env vars).
        if (!string.IsNullOrWhiteSpace(opts.TenantId) &&
            !string.IsNullOrWhiteSpace(opts.ClientId) &&
            !string.IsNullOrWhiteSpace(opts.ClientSecret))
        {
            return new ClientSecretCredential(opts.TenantId, opts.ClientId, opts.ClientSecret);
        }

        return new DefaultAzureCredential();
    }

    // ── Deployments ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ModelDeployment>> GetDeploymentsAsync(CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.deployments.list");
        var results = new List<ModelDeployment>();
        await foreach (var item in _projectClient.Deployments.GetDeploymentsAsync(
            modelPublisher: null,
            modelName: null,
            deploymentType: null,
            cancellationToken: ct))
        {
            if (item is ModelDeployment modelDep)
            {
                results.Add(modelDep);
            }
        }
        activity?.SetTag("foundry.deployments.count", results.Count);
        return results;
    }

    public async Task<AIProjectDeployment?> GetDeploymentAsync(string name, CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.deployments.get");
        activity?.SetTag("foundry.deployment.name", name);
        try
        {
            var response = await _projectClient.Deployments.GetDeploymentAsync(name, ct);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Not found");
            return null;
        }
    }

    // ── Agents ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<PersistentAgent>> GetAgentsAsync(CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.agents.list");
        var results = new List<PersistentAgent>();
        await foreach (var agent in _agentsClient.Administration.GetAgentsAsync(
            limit: null, order: null, after: null, before: null, cancellationToken: ct))
        {
            results.Add(agent);
        }
        activity?.SetTag("foundry.agents.count", results.Count);
        return results;
    }

    public async Task<PersistentAgent?> GetAgentAsync(string agentId, CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.agents.get");
        activity?.SetTag("foundry.agent.id", agentId);
        try
        {
            var response = await _agentsClient.Administration.GetAgentAsync(agentId, ct);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Not found");
            return null;
        }
    }

    public async Task<PersistentAgent> CreateAgentAsync(
        string model,
        string name,
        string instructions,
        string? description = null,
        CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.agents.create");
        activity?.SetTag("foundry.agent.model", model);
        activity?.SetTag("foundry.agent.name", name);
        var response = await _agentsClient.Administration.CreateAgentAsync(
            model: model,
            name: name,
            description: description,
            instructions: instructions,
            tools: null,
            toolResources: null,
            temperature: null,
            topP: null,
            responseFormat: null,
            metadata: null,
            cancellationToken: ct);
        activity?.SetTag("foundry.agent.id", response.Value.Id);
        return response.Value;
    }

    public async Task<bool> DeleteAgentAsync(string agentId, CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.agents.delete");
        activity?.SetTag("foundry.agent.id", agentId);
        try
        {
            var response = await _agentsClient.Administration.DeleteAgentAsync(agentId, ct);
            return response.Value;
        }
        catch (RequestFailedException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Delete failed");
            return false;
        }
    }

    // ── Chat (Thread → Message → Run → Poll → Retrieve) ─────────────────────

    public async Task<string> ChatAsync(
        string agentId,
        string userMessage,
        string? existingThreadId = null,
        CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.chat");
        activity?.SetTag("foundry.agent.id", agentId);
        activity?.SetTag("foundry.chat.has_existing_thread", existingThreadId != null);
        // Create or reuse a thread
        PersistentAgentThread thread;
        if (!string.IsNullOrWhiteSpace(existingThreadId))
        {
            var getResp = await _agentsClient.Threads.GetThreadAsync(existingThreadId, ct);
            thread = getResp.Value;
        }
        else
        {
            var createResp = await _agentsClient.Threads.CreateThreadAsync(
                messages: null, toolResources: null, metadata: null, cancellationToken: ct);
            thread = createResp.Value;
        }
        activity?.SetTag("foundry.thread.id", thread.Id);

        // Add the user message
        await _agentsClient.Messages.CreateMessageAsync(
            threadId: thread.Id,
            role: MessageRole.User,
            content: userMessage,
            attachments: null,
            metadata: null,
            cancellationToken: ct);

        // Run the agent
        var runResponse = await _agentsClient.Runs.CreateRunAsync(
            threadId: thread.Id,
            assistantId: agentId,
            cancellationToken: ct);
        var run = runResponse.Value;

        // Poll until the run completes
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.Cancelling)
        {
            await Task.Delay(800, ct);
            var pollResp = await _agentsClient.Runs.GetRunAsync(thread.Id, run.Id, ct);
            run = pollResp.Value;
        }

        if (run.Status == RunStatus.Failed)
        {
            activity?.SetStatus(ActivityStatusCode.Error, run.LastError?.Message ?? "unknown error");
            return $"[Run failed: {run.LastError?.Message ?? "unknown error"}]";
        }

        // Retrieve the assistant's latest message
        var messages = _agentsClient.Messages.GetMessages(
            threadId: thread.Id,
            runId: run.Id,
            limit: null, order: ListSortOrder.Descending, after: null, before: null);

        foreach (var msg in messages)
        {
            if (msg.Role == MessageRole.Agent)
            {
                var textContent = msg.ContentItems.OfType<MessageTextContent>().FirstOrDefault();
                if (textContent != null)
                {
                    return textContent.Text;
                }
            }
        }

        return "[No response received]";
    }

    public async Task<(string ThreadId, string AssistantReply)> ChatWithThreadAsync(
        string agentId,
        string userMessage,
        string? existingThreadId = null,
        CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.chat.with_thread");
        activity?.SetTag("foundry.agent.id", agentId);
        activity?.SetTag("foundry.chat.has_existing_thread", existingThreadId != null);
        // Create or reuse a thread
        string threadId;
        if (!string.IsNullOrWhiteSpace(existingThreadId))
        {
            threadId = existingThreadId;
        }
        else
        {
            var createResp = await _agentsClient.Threads.CreateThreadAsync(
                messages: null, toolResources: null, metadata: null, cancellationToken: ct);
            threadId = createResp.Value.Id;
        }
        activity?.SetTag("foundry.thread.id", threadId);

        // Add the user message
        await _agentsClient.Messages.CreateMessageAsync(
            threadId: threadId,
            role: MessageRole.User,
            content: userMessage,
            attachments: null,
            metadata: null,
            cancellationToken: ct);

        // Run the agent
        var runResponse = await _agentsClient.Runs.CreateRunAsync(
            threadId: threadId,
            assistantId: agentId,
            cancellationToken: ct);
        var run = runResponse.Value;

        // Poll until complete
        while (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress || run.Status == RunStatus.Cancelling)
        {
            await Task.Delay(800, ct);
            var pollResp = await _agentsClient.Runs.GetRunAsync(threadId, run.Id, ct);
            run = pollResp.Value;
        }

        if (run.Status == RunStatus.Failed)
        {
            activity?.SetStatus(ActivityStatusCode.Error, run.LastError?.Message ?? "unknown error");
            return (threadId, $"[Run failed: {run.LastError?.Message ?? "unknown error"}]");
        }

        // Retrieve all messages in the thread for the run
        var messages = _agentsClient.Messages.GetMessages(
            threadId: threadId,
            runId: run.Id,
            limit: null, order: ListSortOrder.Descending, after: null, before: null);

        foreach (var msg in messages)
        {
            if (msg.Role == MessageRole.Agent)
            {
                var textContent = msg.ContentItems.OfType<MessageTextContent>().FirstOrDefault();
                if (textContent != null)
                {
                    return (threadId, textContent.Text);
                }
            }
        }

        return (threadId, "[No response received]");
    }

    public async Task<IReadOnlyList<(string Role, string Text)>> GetThreadMessagesAsync(
        string threadId,
        CancellationToken ct = default)
    {
        EnsureConfigured();
        var results = new List<(string Role, string Text)>();

        var messages = _agentsClient.Messages.GetMessages(
            threadId: threadId,
            runId: null,
            limit: null, order: ListSortOrder.Ascending, after: null, before: null);

        foreach (var msg in messages)
        {
            var textContent = msg.ContentItems.OfType<MessageTextContent>().FirstOrDefault();
            if (textContent != null)
            {
                results.Add((msg.Role == MessageRole.User ? "user" : "assistant", textContent.Text));
            }
        }

        return results;
    }

    // ── Connections ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AIProjectConnection>> GetConnectionsAsync(CancellationToken ct = default)
    {
        EnsureConfigured();
        using var activity = FoundryTelemetry.Source.StartActivity("foundry.connections.list");
        var results = new List<AIProjectConnection>();
        await foreach (var conn in _projectClient.Connections.GetConnectionsAsync(
            connectionType: null, defaultConnection: null, cancellationToken: ct))
        {
            results.Add(conn);
        }
        activity?.SetTag("foundry.connections.count", results.Count);
        return results;
    }
}
