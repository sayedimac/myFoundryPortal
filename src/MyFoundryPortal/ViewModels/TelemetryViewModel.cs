namespace MyFoundryPortal.ViewModels;

public class TelemetryViewModel
{
    public bool IsAzureMonitorEnabled { get; set; }
    public bool IsConsoleExporterEnabled { get; set; }
    public string ActivitySourceName { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public IReadOnlyList<TelemetrySpanExample> SpanExamples { get; set; } = [];
}

public class TelemetrySpanExample
{
    public string OperationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> Tags { get; set; } = [];
}
