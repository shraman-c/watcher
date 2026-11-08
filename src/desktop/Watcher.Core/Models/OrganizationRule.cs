namespace Watcher.Core.Models;

public class OrganizationRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CategoryId { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> Extensions { get; set; } = new();
    public string TargetFolder { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public bool Enabled { get; set; } = true;
    public bool IsCustom { get; set; } = false;
}

