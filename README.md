# TeklaMCP

A Model Context Protocol (MCP) server for [Tekla Structures](https://www.tekla.com/products/tekla-structures), enabling AI assistants (Claude, Cursor, etc.) to read and modify your structural model through natural language.

## What it does

TeklaMCP runs as a WPF desktop application alongside Tekla Structures. It exposes a local HTTP server (`http://127.0.0.1:51234/`) that speaks the [MCP protocol](https://modelcontextprotocol.io/) — so any MCP-compatible AI client can place beams, query objects, delete elements, and more, all by calling into the live Tekla model.

## MCP Tools

| Tool | Description |
|---|---|
| `PlaceBeam` | Place a beam between two 3D coordinates |
| `PlaceBeams` | Place multiple beams in a single call |
| `PlaceColumn` | Place a column at a base point with a given height |
| `ModifyBeam` | Modify profile, material, position, or endpoints of an existing beam |
| `FilterModel` | Query model objects by type, name, profile, material, class, or user-defined attributes |
| `GetUserSelection` | Read the objects currently selected by the user in Tekla |
| `UserSelectedBeam` | Prompt the user to pick a beam and return its properties |
| `DeleteObject` | Delete a single object by GUID |
| `DeleteObjects` | Delete multiple objects by GUID in one call |
| `OverlappingNumbering` | Find parts with duplicate cast-unit or assembly numbers |

## MCP Resources

| Resource URI | Description |
|---|---|
| `tekla://model/object-names` | Distinct list of all part names in the model |
| `tekla://model/grids` | All grid objects with coordinates and axis labels |

## Requirements

- Windows (WPF desktop app)
- [Tekla Structures](https://www.tekla.com/products/tekla-structures) 2023 or later, with a model open
- .NET Framework 4.8
- An MCP-compatible AI client (Claude Desktop, Cursor, etc.)

## Build

1. Clone the repository.
2. Set the Tekla API reference path. The `.csproj` defaults to looking for Tekla assemblies at:
   ```
   C:\Program Files\Tekla Structures\<version>\bin\plugins
   ```
   Override the version via MSBuild property:
   ```
   msbuild /p:TeklaVersion=2024
   ```
   Or set `TeklaApiPath` to point directly at the folder containing the Tekla DLLs.
3. Build with Visual Studio 2022+ or `dotnet build` / `msbuild`.

## Usage

1. Open a model in Tekla Structures.
2. Launch **TeklaMCP** (`TeklaMcp.Server.exe`).
3. Click **Start** — the server starts at `http://127.0.0.1:51234/`.
4. Configure your MCP client to connect to that URL.

### Claude Desktop (`claude_desktop_config.json`)

```json
{
  "mcpServers": {
    "tekla": {
      "url": "http://127.0.0.1:51234/"
    }
  }
}
```

### Health check

```
GET http://127.0.0.1:51234/health
```

Returns `{"status":"ok"}` when the server is running.

## Audit log

Every tool call is appended to:
```
%APPDATA%\Develoh\Tekla\mcp-audit.log
```

Each line is a JSON object with `timestamp`, `tool`, `args`, `success`, and `message`.

## Project structure

```
TeklaMcp.Contracts/   - Shared DTOs (args and result types)
TeklaMcp.Plugin/      - WPF app + MCP server host
  McpHostedServer.cs  - HTTP listener, JSON-RPC 2.0 dispatcher
  Tekla/              - TeklaModelFacade wrapping the Tekla Open API
  Tools/              - One class per MCP tool
  Logging/            - Audit log
```

## License

[GPL-3.0](LICENSE)

## About

TeklaMCP is created by **[Develoh](https://develoh.com)**.

Need a custom AI implementation in Tekla Structures or Revit? We build tailored solutions for structural engineering workflows. Get in touch at [develoh.com](https://develoh.com).
