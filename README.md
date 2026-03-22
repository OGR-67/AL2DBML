# AL2DBML

Generates a DBML database schema from a Business Central AL project by parsing table and field definitions.

## Prerequisites

No runtime required — AL2DBML is distributed as a self-contained binary.

| Platform | Supported |
|---|---|
| Windows (x64) | ✓ |
| macOS (Apple Silicon) | ✓ |
| Linux (x64) | ✓ |

## Installation

### Windows (PowerShell)

```powershell
$dest = "$env:LOCALAPPDATA\al2dbml"
New-Item -ItemType Directory -Force -Path $dest | Out-Null
Invoke-WebRequest -Uri "https://github.com/OGR-67/AL2DBML/releases/latest/download/al2dbml-win-x64.exe" -OutFile "$dest\al2dbml.exe"
$path = [Environment]::GetEnvironmentVariable("Path", "User")
if ($path -notlike "*al2dbml*") {
    [Environment]::SetEnvironmentVariable("Path", "$path;$dest", "User")
}
Write-Host "Done. Restart your terminal."
```

### macOS

```bash
curl -L https://github.com/OGR-67/AL2DBML/releases/latest/download/al2dbml-osx-arm64 -o al2dbml
chmod +x al2dbml
xattr -d com.apple.quarantine al2dbml
sudo mv al2dbml /usr/local/bin/al2dbml
```

> **Note:** The `xattr` command removes the Gatekeeper quarantine flag that macOS sets on downloaded binaries. Without it, macOS will block the app on first run. If you downloaded the binary manually instead of using this script, go to **System Settings → Privacy & Security** and click **Allow Anyway**.

### Linux

```bash
curl -L https://github.com/OGR-67/AL2DBML/releases/latest/download/al2dbml-linux-x64 -o al2dbml
chmod +x al2dbml
sudo mv al2dbml /usr/local/bin/al2dbml
```

## Quick start

```bash
# 1. Initialize AL2DBML in your project (run once)
al2dbml init

# 2. Generate the DBML schema
al2dbml generate
```

## Commands

### `generate`

Parses AL files and generates a `.dbml` schema file.

```
al2dbml generate [-i <input>] [-o <output>] [-n <name>]
```

| Option | Description | Default |
|---|---|---|
| `-i`, `--input` | Path to an AL project folder, `.al` file, or `.code-workspace` file | Value from `config.local.json`, then `.` |
| `-o`, `--output` | Output directory | Value from `config.json`, then `.` |
| `-n`, `--name` | Output file name (without extension) | Value from `config.json`, then `schema` |

### `init`

Initializes AL2DBML interactively in the current directory. Creates the `.al2dbml/` config folder and optionally sets up a pre-commit hook.

```
al2dbml init
```

Re-running `init` on an existing config pre-fills the prompts with current values.

## Configuration

`init` creates two files in `.al2dbml/`:

| File | Versioned | Content |
|---|---|---|
| `config.json` | Yes | Output path and file name — shared across contributors |
| `config.local.json` | No (gitignored) | Input path — contributor-specific, useful for workspaces |

`config.json` example:
```json
{
  "Output": {
    "Path": "./docs/",
    "Name": "schema"
  }
}
```

`config.local.json` example:
```json
{
  "Input": {
    "Path": "./MyProject.code-workspace"
  }
}
```

## Pre-commit hook

When enabled during `init`, AL2DBML adds a section to `.git/hooks/pre-commit` that automatically regenerates the schema on each commit:

```sh
# [al2dbml-start]
if command -v al2dbml > /dev/null 2>&1; then
    al2dbml generate || printf "\033[33mWarning: al2dbml generate failed, skipping DBML update.\033[0m\n"
else
    printf "\033[33mWarning: al2dbml not found, skipping DBML update.\033[0m\n"
fi
# [al2dbml-end]
```

If `al2dbml` is not installed on a contributor's machine, the hook prints a warning and lets the commit proceed normally.

To remove the hook, run:

```bash
al2dbml remove-hook
```

## Contributing

- [Architecture](docs/ARCHITECTURE.md) — project structure, CLI design, DI setup
- [AL Parsing](docs/AL_PARSING.md) — AL syntax specifics and parsing decisions
