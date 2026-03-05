namespace MyFoundryPortal.ViewModels;

public class DeploymentViewModel
{
    public string Name { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public string ModelPublisher { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public string SkuName { get; set; } = string.Empty;
    public long SkuCapacity { get; set; }
    public IReadOnlyDictionary<string, string> Capabilities { get; set; } = new Dictionary<string, string>();
}
