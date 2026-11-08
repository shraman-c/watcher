namespace Watcher.Core.Models;

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string DefaultFolder { get; set; } = string.Empty;
    public List<string> Extensions { get; set; } = new();
    public string Color { get; set; } = "#6C9BFF";
    public string IconName { get; set; } = "Folder";
    public bool IsSystem { get; set; } = false;
}

