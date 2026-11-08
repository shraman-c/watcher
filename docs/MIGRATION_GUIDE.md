# Watcher 1.0 to Watcher 2.0 — Migration Guide

This guide details how Watcher 2.0 safely handles your existing Watcher 1.0 configuration, rules, and data.

---

## 1. Zero-Data-Loss Migration Policy

Watcher 2.0 includes automated migration routines that ensure:
- Your existing watched folder paths are preserved.
- Your customized extensions and category mappings are imported.
- Original files (including `~/.watcher_config.json` and `settings.json`) are **never deleted or overwritten**.
- Backups (`*.v1.bak`) are created before first run.

---

## 2. Configuration Location Comparison

| Configuration Aspect | Watcher 1.0 (Python/PyQt6) | Watcher 2.0 (.NET 9 / SQLite) |
| :--- | :--- | :--- |
| **Config File** | `~/.watcher_config.json` or `./settings.json` | `%LOCALAPPDATA%\Watcher\watcher.db` |
| **Storage Engine** | Unindexed JSON file | SQLite Database (WAL mode, indexed) |
| **Rule Format** | Plain text: `Category = ext1,ext2,...` | Normalized `Categories` & `OrganizationRules` tables |
| **Activity History** | Transient (in-memory `QTextEdit`) | Persistent `ActivityLogs` table with Undo support |
| **WebView Cache** | N/A | `%LOCALAPPDATA%\Watcher\WebView2Data\` |

---

## 3. How Automated Migration Works

When Watcher 2.0 starts for the first time:
1. `LegacyConfigMigrator` inspects the SQLite database. If the database already has folders configured, migration is skipped.
2. If the database is empty, it searches for legacy configurations in:
   - `C:\Users\<Username>\.watcher_config.json`
   - `./settings.json` (application root)
3. For each folder found in the legacy `folders` array:
   - It normalizes the path and checks if the directory exists.
   - It preserves the `unknown` fallback target (default `"Other"`).
   - It preserves the `quiet` mode flag.
   - It inserts the folder into the `WatchedFolders` table.
4. If a `rules_text` block is present (e.g., `Documents = pdf,doc...`), it parses each line, creates any custom categories if they don't already exist, and associates the extension lists with active `OrganizationRules`.
5. The original configuration files remain untouched on disk.

---

## 4. Manual Configuration Rollback or Export

If you ever wish to export your rules from Watcher 2.0 or restore original defaults:
1. Open Watcher 2.0 and go to **Organization Rules**.
2. Click **Restore Defaults** to reset to the factory 9 category set.
3. Your original Watcher 1.0 files remain backed up at:
   - `%USERPROFILE%\.watcher_config.json.v1.bak`
   - `c:\Users\<Username>\watcher\settings.json.v1.bak`

