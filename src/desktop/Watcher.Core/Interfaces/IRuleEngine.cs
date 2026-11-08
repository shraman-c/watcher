using Watcher.Core.Models;

namespace Watcher.Core.Interfaces;

public interface IRuleEngine
{
    /// <summary>
    /// Matches a file to a Category and destination folder name based on its extension and priority rules.
    /// </summary>
    (string CategoryName, string TargetFolderName) EvaluateFile(string fileName, string fallbackUnknownTarget);

    /// <summary>
    /// Loads or refreshes the rule set.
    /// </summary>
    void RefreshRules(IEnumerable<OrganizationRule> rules);
}

