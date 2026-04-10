using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Tekla;
using TeklaMcp.Server.Tools;

namespace TeklaMcp.Server
{
    /// <summary>
    /// MCP Streamable HTTP server
    /// Implements the Model Context Protocol JSON-RPC 2.0 transport at POST /
    /// Listens on http://127.0.0.1:51234/
    /// </summary>
    public sealed class McpHostedServer : IDisposable
    {
        private const string Prefix = "http://127.0.0.1:51234/";
        private const string ProtocolVersion = "2024-11-05";
        private const string ObjectNamesResourceUri = "tekla://model/object-names";
        private const string GridsResourceUri = "tekla://model/grids";

        private readonly HttpListener _listener = new HttpListener();
        private readonly List<McpToolDefinition> _toolDefs = new List<McpToolDefinition>();
        private readonly Dictionary<string, Func<JsonElement, object>> _handlers =
            new Dictionary<string, Func<JsonElement, object>>();
        private TeklaModelFacade _facade = null!;
        private Thread? _thread;
        private volatile bool _running;

        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public McpHostedServer()
        {
            _listener.Prefixes.Add(Prefix);
            RegisterTools();
        }

        private void RegisterTools()
        {
            _facade = new TeklaModelFacade();
            var facade = _facade;
            var beamTool = new PlaceBeamTool(facade);
            var columnTool = new PlaceColumnTool(facade);
            var filterTool = new FilterModelTool(facade);
            var userBeamTool = new UserSelectedBeamTool(facade);
            var userSelectTool = new GetUserSelectionTool(facade);
            var deleteTool = new DeleteObjectTool(facade);
            var deletesTool = new DeleteObjectsTool(facade);
            var modifyBeamTool = new ModifyBeamTool(facade);
            var placeBeamsTool = new PlaceBeamsTool(facade);
            var overlappingNumberingTool = new OverlappingNumberingTool(facade);

            RegisterTool(beamTool.GetDefinition(), "PlaceBeam", body => beamTool.Invoke(body));
            RegisterTool(columnTool.GetDefinition(), "PlaceColumn", body => columnTool.Invoke(body));
            RegisterTool(filterTool.GetDefinition(), "FilterModel", body => filterTool.Invoke(body));
            RegisterTool(userBeamTool.GetDefinition(), "UserSelectedBeam", body => userBeamTool.Invoke(body));
            RegisterTool(userSelectTool.GetDefinition(), "GetUserSelection", body => userSelectTool.Invoke(body));
            RegisterTool(deleteTool.GetDefinition(), "DeleteObject", body => deleteTool.Invoke(body));
            RegisterTool(deletesTool.GetDefinition(), "DeleteObjects", body => deletesTool.Invoke(body));
            RegisterTool(modifyBeamTool.GetDefinition(), "ModifyBeam", body => modifyBeamTool.Invoke(body));
            RegisterTool(placeBeamsTool.GetDefinition(), "PlaceBeams", body => placeBeamsTool.Invoke(body));
            RegisterTool(overlappingNumberingTool.GetDefinition(), "OverlappingNumbering", body => overlappingNumberingTool.Invoke(body));
        }

        private void RegisterTool(McpToolDefinition definition, string name, Func<JsonElement, object> handler)
        {
            _toolDefs.Add(definition);
            _handlers[name] = handler;
        }

        public void Start()
        {
            _listener.Start();
            _running = true;
            _thread = new Thread(ListenLoop) { IsBackground = true, Name = "McpHttpServer" };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
            _thread?.Join(TimeSpan.FromSeconds(5));
        }

        public void Dispose() => Stop();

        private void ListenLoop()
        {
            while (_running)
            {
                HttpListenerContext? ctx = null;
                try { ctx = _listener.GetContext(); }
                catch { break; }
                ThreadPool.QueueUserWorkItem(_ => Handle(ctx));
            }
        }

        private void Handle(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;
            resp.ContentType = "application/json; charset=utf-8";

            try
            {
                var path = req.Url?.AbsolutePath ?? "";
                var method = req.HttpMethod;

                if (method == "GET" && path == "/health")
                {
                    Send(resp, 200, "{\"status\":\"ok\"}");
                    return;
                }

                if (method == "POST" && path == "/")
                {
                    HandleJsonRpc(req, resp);
                    return;
                }

                Send(resp, 404, "{\"error\":\"Not found\"}");
            }
            catch (Exception ex)
            {
                try { Send(resp, 500, JsonSerializer.Serialize(new { error = ex.Message })); }
                catch { }
            }
        }

        private void HandleJsonRpc(HttpListenerRequest req, HttpListenerResponse resp)
        {
            string rawBody;
            using (var reader = new StreamReader(req.InputStream, req.ContentEncoding ?? Encoding.UTF8))
                rawBody = reader.ReadToEnd();

            var execution = ExecuteJsonRpc(rawBody);
            if (execution.IsNotification)
            {
                resp.StatusCode = 202;
                resp.Close();
                return;
            }

            Send(resp, execution.StatusCode, execution.Body);
        }

        private JsonRpcExecutionResult ExecuteJsonRpc(string rawBody)
        {
            JsonDocument doc;
            try { doc = JsonDocument.Parse(rawBody); }
            catch
            {
                return JsonRpcExecutionResult.Response(200, BuildError(null, -32700, "Parse error"));
            }

            using (doc)
            {
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return JsonRpcExecutionResult.Response(200, BuildError(null, -32600, "Invalid Request"));
                }

                var hasId = root.TryGetProperty("id", out var idEl);
                var mcpMethod = root.TryGetProperty("method", out var methodEl)
                    ? methodEl.GetString() ?? ""
                    : "";
                var paramsEl = root.TryGetProperty("params", out var p) ? p : default;

                if (!hasId)
                {
                    return JsonRpcExecutionResult.Notification;
                }

                try
                {
                    switch (mcpMethod)
                    {
                        case "initialize":
                            return JsonRpcExecutionResult.Response(200, BuildResult(idEl, BuildInitializeResult()));
                        case "ping":
                            return JsonRpcExecutionResult.Response(200, BuildResult(idEl, new JsonObject()));
                        case "tools/list":
                            return JsonRpcExecutionResult.Response(200, BuildResult(idEl, BuildToolsListResult()));
                        case "tools/call":
                            return JsonRpcExecutionResult.Response(200, BuildToolsCallResponse(idEl, paramsEl));
                        case "resources/list":
                            return JsonRpcExecutionResult.Response(200, BuildResult(idEl, BuildResourcesListResult()));
                        case "resources/read":
                            return JsonRpcExecutionResult.Response(200, BuildResourcesReadResponse(idEl, paramsEl));
                        default:
                            return JsonRpcExecutionResult.Response(200, BuildError(idEl, -32601, $"Method not found: {mcpMethod}"));
                    }
                }
                catch (Exception ex)
                {
                    return JsonRpcExecutionResult.Response(200, BuildError(idEl, -32603, ex.Message));
                }
            }
        }

        private JsonObject BuildInitializeResult()
        {
            return new JsonObject
            {
                ["protocolVersion"] = ProtocolVersion,
                ["capabilities"] = new JsonObject
                {
                    ["tools"] = new JsonObject(),
                    ["resources"] = new JsonObject()
                },
                ["serverInfo"] = new JsonObject
                {
                    ["name"] = "tekla-mcp-server",
                    ["version"] = "1.0.0"
                }
            };
        }

        private JsonObject BuildToolsListResult()
        {
            var toolsArray = new JsonArray();
            foreach (var def in _toolDefs)
            {
                var schemaJson = JsonSerializer.Serialize(def.InputSchema, JsonOpts);
                var schemaNode = JsonNode.Parse(schemaJson);

                toolsArray.Add(new JsonObject
                {
                    ["name"] = def.Name,
                    ["description"] = def.Description,
                    ["inputSchema"] = schemaNode
                });
            }

            return new JsonObject { ["tools"] = toolsArray };
        }

        private string BuildToolsCallResponse(JsonElement id, JsonElement paramsEl)
        {
            var toolName = "";
            JsonElement args = default;

            if (paramsEl.ValueKind == JsonValueKind.Object)
            {
                if (paramsEl.TryGetProperty("name", out var nameEl))
                    toolName = nameEl.GetString() ?? "";

                if (paramsEl.TryGetProperty("arguments", out var argsEl))
                    args = argsEl;
            }

            if (!_handlers.TryGetValue(toolName, out var handler))
            {
                return BuildError(id, -32602, $"Unknown tool: {toolName}");
            }

            if (args.ValueKind == JsonValueKind.Undefined)
            {
                using (var empty = JsonDocument.Parse("{}"))
                {
                    args = empty.RootElement.Clone();
                }
            }

            object toolResult;
            try
            {
                toolResult = handler(args);
            }
            catch (ArgumentException ex)
            {
                return BuildError(id, -32602, ex.Message);
            }
            catch (Exception ex)
            {
                return BuildError(id, -32603, ex.Message);
            }

            var resultText = JsonSerializer.Serialize(toolResult, JsonOpts);
            var content = new JsonArray
            {
                new JsonObject { ["type"] = "text", ["text"] = resultText }
            };

            return BuildResult(id, new JsonObject { ["content"] = content });
        }

        private JsonObject BuildResourcesListResult()
        {
            var resources = new JsonArray
            {
                new JsonObject
                {
                    ["uri"] = ObjectNamesResourceUri,
                    ["name"] = "Model Object Names",
                    ["description"] = "Distinct list of all object names present in the current Tekla model.",
                    ["mimeType"] = "application/json"
                },
                new JsonObject
                {
                    ["uri"] = GridsResourceUri,
                    ["name"] = "Model Grids",
                    ["description"] = "All grid objects in the current Tekla model, including coordinates and labels on each axis.",
                    ["mimeType"] = "application/json"
                }
            };

            return new JsonObject { ["resources"] = resources };
        }

        private string BuildResourcesReadResponse(JsonElement id, JsonElement paramsEl)
        {
            var uri = "";
            if (paramsEl.ValueKind == JsonValueKind.Object && paramsEl.TryGetProperty("uri", out var uriEl))
                uri = uriEl.GetString() ?? "";

            if (uri == ObjectNamesResourceUri)
            {
                var names = _facade.GetModelObjectNames();
                var text = JsonSerializer.Serialize(names, JsonOpts);
                var contents = new JsonArray
                {
                    new JsonObject
                    {
                        ["uri"] = ObjectNamesResourceUri,
                        ["mimeType"] = "application/json",
                        ["text"] = text
                    }
                };
                return BuildResult(id, new JsonObject { ["contents"] = contents });
            }

            if (uri == GridsResourceUri)
            {
                var grids = _facade.GetAllGrids();
                var text = JsonSerializer.Serialize(grids, JsonOpts);
                var contents = new JsonArray
                {
                    new JsonObject
                    {
                        ["uri"] = GridsResourceUri,
                        ["mimeType"] = "application/json",
                        ["text"] = text
                    }
                };
                return BuildResult(id, new JsonObject { ["contents"] = contents });
            }

            return BuildError(id, -32602, $"Unknown resource: {uri}");
        }

        private static string BuildResult(JsonElement id, JsonNode result)
        {
            var envelope = BuildEnvelope(id);
            envelope["result"] = result;
            return envelope.ToJsonString();
        }

        private static string BuildError(JsonElement? id, int code, string message)
        {
            JsonObject envelope = id.HasValue
                ? BuildEnvelope(id.Value)
                : new JsonObject { ["jsonrpc"] = "2.0", ["id"] = JsonValue.Create((object?)null) };

            envelope["error"] = new JsonObject
            {
                ["code"] = code,
                ["message"] = message
            };

            return envelope.ToJsonString();
        }

        private static JsonObject BuildEnvelope(JsonElement id)
        {
            JsonNode idNode = JsonSerializer.SerializeToNode(id) ?? JsonValue.Create((object?)null)!;
            return new JsonObject { ["jsonrpc"] = "2.0", ["id"] = idNode };
        }

        private static void Send(HttpListenerResponse resp, int statusCode, string json)
        {
            resp.StatusCode = statusCode;
            var bytes = Encoding.UTF8.GetBytes(json);
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.OutputStream.Close();
        }

        public string HandleJsonRpcRelay(string rawBody)
        {
            var execution = ExecuteJsonRpc(rawBody);
            return execution.IsNotification ? string.Empty : execution.Body;
        }

        private readonly struct JsonRpcExecutionResult
        {
            private JsonRpcExecutionResult(bool isNotification, int statusCode, string body)
            {
                IsNotification = isNotification;
                StatusCode = statusCode;
                Body = body;
            }

            public bool IsNotification { get; }
            public int StatusCode { get; }
            public string Body { get; }

            public static JsonRpcExecutionResult Notification => new JsonRpcExecutionResult(true, 202, string.Empty);
            public static JsonRpcExecutionResult Response(int statusCode, string body) =>
                new JsonRpcExecutionResult(false, statusCode, body);
        }
    }
}
