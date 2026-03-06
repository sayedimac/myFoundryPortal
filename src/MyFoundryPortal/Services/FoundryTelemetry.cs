using System.Diagnostics;

namespace MyFoundryPortal.Services;

/// <summary>
/// Centralised OpenTelemetry <see cref="ActivitySource"/> used to emit
/// distributed-tracing spans for every Azure AI Foundry operation that the
/// portal performs (agent CRUD, chat, deployment listing, connections, …).
///
/// Register the source name with OpenTelemetry in <c>Program.cs</c> via
/// <c>AddSource(FoundryTelemetry.ActivitySourceName)</c> so that the SDK
/// picks up and exports these activities.
/// </summary>
public static class FoundryTelemetry
{
    /// <summary>Name that must be passed to <c>AddSource()</c> in the OTel builder.</summary>
    public const string ActivitySourceName = "MyFoundryPortal.Foundry";

    internal static readonly ActivitySource Source =
        new(ActivitySourceName, version: "1.0.0");
}
