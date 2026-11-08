using Watcher.Core.Interfaces;
using Watcher.Core.Models;

namespace Watcher.Core.Rules;

public class RuleEngine : IRuleEngine
{
    private readonly object _lock = new();
    private List<OrganizationRule> _rules = new();
    private readonly Dictionary<string, (string Category, string TargetFolder)> _extensionCache = new(StringComparer.OrdinalIgnoreCase);

    public RuleEngine(IEnumerable<OrganizationRule>? initialRules = null)
    {
        RefreshRules(initialRules ?? DefaultCategories.GetDefaultRules());
    }

    public void RefreshRules(IEnumerable<OrganizationRule> rules)
    {
        lock (_lock)
        {
            _rules = rules.Where(r => r.Enabled).OrderBy(r => r.Priority).ToList();
            _extensionCache.Clear();

            foreach (var rule in _rules)
            {
                foreach (var ext in rule.Extensions)
                {
                    var cleanExt = ext.Trim().TrimStart('.').ToLowerInvariant();
                    if (!string.IsNullOrEmpty(cleanExt) && !_extensionCache.ContainsKey(cleanExt))
                    {
                        var target = string.IsNullOrWhiteSpace(rule.TargetFolder) ? rule.CategoryName : rule.TargetFolder;
                        _extensionCache[cleanExt] = (rule.CategoryName, target);
                    }
                }
            }
        }
    }

    public (string CategoryName, string TargetFolderName) EvaluateFile(string fileName, string fallbackUnknownTarget)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrEmpty(ext))
        {
            return (fallbackUnknownTarget, fallbackUnknownTarget);
        }

        lock (_lock)
        {
            if (_extensionCache.TryGetValue(ext, out var matched))
            {
                return matched;
            }
        }

        return (fallbackUnknownTarget, fallbackUnknownTarget);
    }
}

