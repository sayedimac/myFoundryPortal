# MyFoundryPortal

A lightweight MVP demo of the **Azure AI Foundry Portal**, built with ASP.NET Core MVC and the [Azure AI Projects SDK](https://www.nuget.org/packages/Azure.AI.Projects) (v1.1.0) + [Azure AI Agents Persistent SDK](https://www.nuget.org/packages/Azure.AI.Agents.Persistent) (v1.1.0).

---

## Features

| Feature | Description |
|---|---|
| **Models** | Enumerate model deployments in your AI Foundry project |
| **Agents** | List, create, and delete persistent agents |
| **Playground** | Chat interactively with any registered agent |
| **Dashboard** | At-a-glance counts for deployments, agents, and connections |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
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
│   ├── HomeController.cs       # Dashboard
│   ├── ModelsController.cs     # Model deployments (list + details)
│   └── AgentsController.cs     # Agents (list, create, delete, playground)
├── Services/
│   └── FoundryService.cs       # Wraps AIProjectClient + PersistentAgentsClient
├── ViewModels/
│   ├── DeploymentViewModel.cs
│   └── AgentViewModel.cs       # AgentViewModel, CreateAgentViewModel, ChatViewModel
├── Views/
│   ├── Home/Index.cshtml       # Dashboard
│   ├── Models/Index.cshtml     # Model list
│   ├── Models/Details.cshtml   # Model detail
│   ├── Agents/Index.cshtml     # Agent list
│   ├── Agents/Create.cshtml    # Create agent form
│   └── Agents/Playground.cshtml # Chat playground
└── wwwroot/css/portal.css      # Foundry-style UI theme
```

---

## SDK References

- [Azure.AI.Projects NuGet](https://www.nuget.org/packages/Azure.AI.Projects)
- [Azure.AI.Agents.Persistent NuGet](https://www.nuget.org/packages/Azure.AI.Agents.Persistent)
- [Azure AI Projects .NET API docs](https://learn.microsoft.com/en-us/dotnet/api/azure.ai.projects)
