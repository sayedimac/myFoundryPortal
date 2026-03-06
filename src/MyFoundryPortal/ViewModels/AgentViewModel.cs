namespace MyFoundryPortal.ViewModels;

public class AgentViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateAgentViewModel
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Model { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Instructions { get; set; } = "You are a helpful assistant.";

    public List<string> AvailableModels { get; set; } = [];
}

public class ChatViewModel
{
    public string AgentId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string AgentModel { get; set; } = string.Empty;
    public string AgentInstructions { get; set; } = string.Empty;
    public string? ThreadId { get; set; }
    public List<ChatMessageViewModel> Messages { get; set; } = [];
}

public class ChatMessageViewModel
{
    public string Role { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsUser => Role == "user";
}
