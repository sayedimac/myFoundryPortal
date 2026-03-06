using Microsoft.AspNetCore.Mvc;
using MyFoundryPortal.Services;
using MyFoundryPortal.ViewModels;

namespace MyFoundryPortal.Controllers;

public class EvaluationsController : Controller
{
    private readonly FoundryService _foundry;
    private readonly ILogger<EvaluationsController> _logger;

    public EvaluationsController(FoundryService foundry, ILogger<EvaluationsController> logger)
    {
        _foundry = foundry;
        _logger = logger;
    }

    // GET /Evaluations
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Gather deployment and agent counts to enrich the page context
        int deploymentCount = 0;
        int agentCount = 0;

        try { deploymentCount = (await _foundry.GetDeploymentsAsync(ct)).Count; }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not load deployment count for evaluations page"); }

        try { agentCount = (await _foundry.GetAgentsAsync(ct)).Count; }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not load agent count for evaluations page"); }

        var vm = new EvaluationsViewModel
        {
            DeploymentCount = deploymentCount,
            AgentCount = agentCount,
            Metrics =
            [
                new EvaluationMetricInfo
                {
                    Name = "Coherence",
                    Icon = "bi-diagram-2",
                    Description = "Measures how logically consistent and well-structured the model's response is. A coherent answer flows naturally from premise to conclusion.",
                    ScoreRange = "1–5",
                    Category = "Quality",
                },
                new EvaluationMetricInfo
                {
                    Name = "Relevance",
                    Icon = "bi-bullseye",
                    Description = "Assesses whether the response directly addresses the user's question or task, without unnecessary detours or off-topic content.",
                    ScoreRange = "1–5",
                    Category = "Quality",
                },
                new EvaluationMetricInfo
                {
                    Name = "Fluency",
                    Icon = "bi-chat-text",
                    Description = "Evaluates the grammatical correctness, readability, and naturalness of the generated text.",
                    ScoreRange = "1–5",
                    Category = "Quality",
                },
                new EvaluationMetricInfo
                {
                    Name = "Groundedness",
                    Icon = "bi-pin-map",
                    Description = "For RAG scenarios: checks whether every claim in the response is supported by the retrieved context documents, helping prevent hallucinations.",
                    ScoreRange = "1–5",
                    Category = "RAG / Grounding",
                },
                new EvaluationMetricInfo
                {
                    Name = "Retrieval Score",
                    Icon = "bi-search",
                    Description = "Measures how well the retrieval step surfaces the most relevant context chunks for the given query, independent of the generation quality.",
                    ScoreRange = "1–5",
                    Category = "RAG / Grounding",
                },
                new EvaluationMetricInfo
                {
                    Name = "Violence",
                    Icon = "bi-shield-exclamation",
                    Description = "Content safety metric that scores the presence of violent or graphic content in the model's output.",
                    ScoreRange = "0–7 (severity)",
                    Category = "Safety",
                },
                new EvaluationMetricInfo
                {
                    Name = "Hate & Unfairness",
                    Icon = "bi-shield-x",
                    Description = "Detects content that demeans individuals or groups based on protected characteristics such as race, religion, or gender.",
                    ScoreRange = "0–7 (severity)",
                    Category = "Safety",
                },
                new EvaluationMetricInfo
                {
                    Name = "Sexual Content",
                    Icon = "bi-shield",
                    Description = "Identifies sexually explicit or suggestive content in model responses, supporting safe deployment in regulated environments.",
                    ScoreRange = "0–7 (severity)",
                    Category = "Safety",
                },
                new EvaluationMetricInfo
                {
                    Name = "Self-Harm",
                    Icon = "bi-heart-pulse",
                    Description = "Flags content that could encourage self-harm behaviors, an important safety gate before production deployment.",
                    ScoreRange = "0–7 (severity)",
                    Category = "Safety",
                },
            ],
        };

        return View(vm);
    }
}
