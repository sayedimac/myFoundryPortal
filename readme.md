# MyFoundryPortal Demo App

A demo app showcasing the **Azure AI Foundry SDK** for the [Azure AI Engineer (AI-102)](https://learn.microsoft.com/en-us/credentials/certifications/azure-ai-engineer/?practice-assessment-type=certification) certification course.  
Built with ASP.NET Core MVC and the [Azure AI Projects SDK](https://www.nuget.org/packages/Azure.AI.Projects) (v1.1.0) + [Azure AI Agents Persistent SDK](https://www.nuget.org/packages/Azure.AI.Agents.Persistent) (v1.1.0).

---

## Features

| Feature | Description |
|---|---|
| **Dashboard** | At-a-glance counts for deployments, agents, and connections with quick-action links |
| **Models** | Enumerate model deployments in your AI Foundry project with full detail view |
| **Agents** | List, create, and delete persistent agents backed by any deployed model |
| **Playground** | Chat interactively with any registered agent using persistent threads |
| **Connections** | Browse all project connections (Azure OpenAI, AI Search, Blob Storage, etc.) with details |
| **Evaluations** | Learn evaluation concepts and metrics (quality, RAG grounding, safety) with an SDK code sample |
| **Telemetry** | OpenTelemetry tracing configuration and span catalog for all Foundry operations |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)
- An **Azure AI Foundry** project
- An **App Registration** in Azure AD with access to the Foundry project (or Managed Identity / Azure CLI credentials)

---

## Quick Start

### 1. Clone and navigate

```bash
git clone https://github.com/sayedimac/myFoundryPortal.git
cd myFoundryPortal/src/MyFoundryPortal
```

### 2. Configure credentials

Set the following environment variables **or** add them to `appsettings.Development.json` under the `FoundryPortal` section:

| Environment Variable | `appsettings.json` key | Description |
|---|---|---|
| `AZURE_AI_PROJECT_ENDPOINT` | `FoundryPortal:ProjectEndpoint` | AI Foundry project endpoint URL |
| `AZURE_TENANT_ID` | `FoundryPortal:TenantId` | Azure AD tenant ID |
| `AZURE_CLIENT_ID` | `FoundryPortal:ClientId` | App registration client ID |
| `AZURE_CLIENT_SECRET` | `FoundryPortal:ClientSecret` | App registration client secret |

> **Tip:** If `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, and `AZURE_CLIENT_SECRET` are all empty the app falls back to `DefaultAzureCredential` (Azure CLI, Managed Identity, etc.).

#### Finding your Project Endpoint

In the Azure AI Foundry portal → **Management** → **Overview** → copy the *Project endpoint* value. It looks like:

```
https://<hub-name>.services.ai.azure.com/api/projects/<project-name>
```

#### Example `appsettings.Development.json`

```json
{
  "FoundryPortal": {
    "ProjectEndpoint": "https://my-hub.services.ai.azure.com/api/projects/my-project",
    "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "ClientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "ClientSecret": "your-client-secret"
  }
}
```

### 3. Run

```bash
dotnet run
```

Open https://localhost:7259 (or http://localhost:5007) in your browser.

---

## Project Structure

```
src/MyFoundryPortal/
├── Controllers/
│   ├── HomeController.cs           # Dashboard
│   ├── ModelsController.cs         # Model deployments (list + details)
│   ├── AgentsController.cs         # Agents (list, create, delete, playground/chat)
│   ├── ConnectionsController.cs    # Project connections (list + details)
│   ├── EvaluationsController.cs    # Evaluation metrics demo page
│   └── TelemetryController.cs      # OpenTelemetry configuration page
├── Services/
│   └── FoundryService.cs           # Wraps AIProjectClient + PersistentAgentsClient
├── ViewModels/
│   ├── AgentViewModel.cs           # AgentViewModel, CreateAgentViewModel, ChatViewModel
│   ├── ConnectionViewModel.cs      # ConnectionViewModel
│   ├── DeploymentViewModel.cs      # DeploymentViewModel
│   ├── EvaluationsViewModel.cs     # EvaluationsViewModel + EvaluationMetricInfo
│   └── TelemetryViewModel.cs       # TelemetryViewModel + TelemetrySpanExample
├── Views/
│   ├── Home/Index.cshtml           # Dashboard
│   ├── Models/{Index,Details}      # Model list & detail
│   ├── Agents/{Index,Create,Playground}  # Agent management & chat
│   ├── Connections/{Index,Details} # Connection list & detail
│   ├── Evaluations/Index.cshtml    # Evaluation metrics & SDK demo
│   └── Telemetry/Index.cshtml      # Telemetry configuration
└── wwwroot/css/portal.css          # Foundry-style UI theme
```

---

## SDK References

- [Azure.AI.Projects NuGet](https://www.nuget.org/packages/Azure.AI.Projects)
- [Azure.AI.Agents.Persistent NuGet](https://www.nuget.org/packages/Azure.AI.Agents.Persistent)
- [Azure AI Projects .NET API docs](https://learn.microsoft.com/en-us/dotnet/api/azure.ai.projects)
- [Azure AI Engineer Certification (AI-102)](https://learn.microsoft.com/en-us/credentials/certifications/azure-ai-engineer/?practice-assessment-type=certification)
