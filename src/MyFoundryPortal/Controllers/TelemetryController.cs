using Microsoft.AspNetCore.Mvc;
using MyFoundryPortal.Services;
using MyFoundryPortal.ViewModels;

namespace MyFoundryPortal.Controllers;

public class TelemetryController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(
        IConfiguration configuration,
        IWebHostEnvironment env,
        ILogger<TelemetryController> logger)
    {
        _configuration = configuration;
        _env = env;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var appInsightsConnectionString =
            _configuration["ApplicationInsights:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

        var isAzureMonitorEnabled = !string.IsNullOrWhiteSpace(appInsightsConnectionString);

        var vm = new TelemetryViewModel
        {
            IsAzureMonitorEnabled = isAzureMonitorEnabled,
            IsConsoleExporterEnabled = !isAzureMonitorEnabled || _env.IsDevelopment(),
            ActivitySourceName = FoundryTelemetry.ActivitySourceName,
            TargetFramework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            SpanExamples =
            [
                new TelemetrySpanExample
                {
                    OperationName = "foundry.deployments.list",
                    Description = "Emitted every time the portal enumerates deployed models from AI Foundry.",
                    Tags = ["foundry.deployments.count"],
                },
                new TelemetrySpanExample
                {
                    OperationName = "foundry.deployments.get",
                    Description = "Emitted when the portal fetches a single deployment by name.",
                    Tags = ["foundry.deployment.name"],
                },
                new TelemetrySpanExample
                {
                    OperationName = "foundry.agents.list",
                    Description = "Emitted every time the agent list page is loaded.",
                    Tags = ["foundry.agents.count"],
                },
                new TelemetrySpanExample
                {
                    OperationName = "foundry.agents.get",
                    Description = "Emitted when a specific agent is fetched (e.g., opening the Playground).",
                    Tags = ["foundry.agent.id"],
                },
                new TelemetrySpanExample
                {
                    OperationName = "foundry.agents.create",
                    Description = "Emitted when a new agent is created.",
                    Tags = ["foundry.agent.model", "foundry.agent.name", "foundry.agent.id"],
                },
                new TelemetrySpanExample
                {
                    OperationName = "foundry.agents.delete",
                    Description = "Emitted when an agent is deleted.",
                    Tags = ["foundry.agent.id"],
                },
                new TelemetrySpanExample
                {
                    OperationName = "foundry.chat.with_thread",
                    Description = "Emitted for every user message sent to an agent in the Playground.",
                    Tags = ["foundry.agent.id", "foundry.thread.id", "foundry.chat.has_existing_thread"],
                },
                new TelemetrySpanExample
                {
                    OperationName = "foundry.connections.list",
                    Description = "Emitted when the dashboard lists project connections.",
                    Tags = ["foundry.connections.count"],
                },
            ],
        };

        _logger.LogInformation(
            "Telemetry page loaded. AzureMonitor={AzureMonitorEnabled}, Console={ConsoleEnabled}",
            vm.IsAzureMonitorEnabled,
            vm.IsConsoleExporterEnabled);

        return View(vm);
    }
}
