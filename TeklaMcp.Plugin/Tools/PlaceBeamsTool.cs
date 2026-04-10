using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class PlaceBeamsTool
    {
        private readonly TeklaModelFacade _facade;

        public PlaceBeamsTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "PlaceBeams",
            Description = "Places multiple beams in Tekla Structures in a single operation. All beams are committed together.",
            InputSchema = new
            {
                type     = "object",
                required = new[] { "beams" },
                properties = new
                {
                    beams = new
                    {
                        type  = "array",
                        description = "List of beams to place.",
                        items = new
                        {
                            type     = "object",
                            required = new[] { "start", "end", "profile" },
                            properties = new
                            {
                                start    = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                                end      = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                                profile  = new { type = "string" },
                                material = new { type = "string" },
                                @class   = new { type = "integer" },
                                name     = new { type = "string" },
                                position = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        plane          = new { type = "string", @enum = new[] { "LEFT", "MIDDLE", "RIGHT" } },
                                        planeOffset    = new { type = "number" },
                                        depth          = new { type = "string", @enum = new[] { "FRONT", "MIDDLE", "BEHIND" } },
                                        depthOffset    = new { type = "number" },
                                        rotation       = new { type = "string", @enum = new[] { "FRONT", "TOP", "BACK", "BOTTOM" } },
                                        rotationOffset = new { type = "number" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        public PlaceBeamsResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeRequired<PlaceBeamsArgs>(body, "Invalid PlaceBeams arguments.");

            if (args.Beams == null || args.Beams.Count == 0)
                throw new ArgumentException("beams must contain at least one entry.");

            var result = ToolExecution.Run(
                () => _facade.InsertBeams(args),
                message => new PlaceBeamsResult { Success = false, Message = message });

            McpAuditLog.Write("PlaceBeams", args, result.Success, null, result.Message);
            return result;
        }
    }
}
