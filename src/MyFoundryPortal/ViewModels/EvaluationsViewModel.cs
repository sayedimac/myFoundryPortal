namespace MyFoundryPortal.ViewModels;

public class EvaluationsViewModel
{
    public int DeploymentCount { get; set; }
    public int AgentCount { get; set; }
    public IReadOnlyList<EvaluationMetricInfo> Metrics { get; set; } = [];
}

public class EvaluationMetricInfo
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ScoreRange { get; set; } = string.Empty;
    /// <summary>Category: Quality | RAG / Grounding | Safety</summary>
    public string Category { get; set; } = string.Empty;
}
