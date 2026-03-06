namespace MyFoundryPortal.ViewModels;

public class ConnectionViewModel
{
    public string Name { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string AuthType { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
}
