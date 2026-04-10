using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class PlaceBeamTool
    {
        private readonly TeklaModelFacade _facade;

        public PlaceBeamTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "PlaceBeam",
            Description = "Places a beam between two global coordinates in millimeters.",
            InputSchema = new
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
                        description = "Optional position overrides. Omitted fields use defaults: plane=MIDDLE, depth=BEHIND, rotation=FRONT.",
                        properties = new
                        {
                            plane         = new { type = "string", @enum = new[] { "LEFT", "MIDDLE", "RIGHT" } },
                            planeOffset   = new { type = "number", description = "Offset in mm along the plane axis." },
                            depth         = new { type = "string", @enum = new[] { "FRONT", "MIDDLE", "BEHIND" } },
                            depthOffset   = new { type = "number", description = "Offset in mm along the depth axis." },
                            rotation      = new { type = "string", @enum = new[] { "FRONT", "TOP", "BACK", "BOTTOM" } },
                            rotationOffset = new { type = "number", description = "Rotation offset in degrees." }
                        }
                    }
                }
            }
        };

        public PlaceBeamResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeRequired<PlaceBeamArgs>(body, "Invalid PlaceBeam arguments.");

            Validate(args);

            var result = ToolExecution.Run(
                () => _facade.InsertBeam(args),
                message => new PlaceBeamResult { Success = false, Message = message });

            McpAuditLog.Write("PlaceBeam", args, result.Success, result.Guid, result.Message);
            return result;
        }

        private static void Validate(PlaceBeamArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Profile))
                throw new ArgumentException("profile is required and must not be empty.");

            var s = args.Start;
            var e = args.End;
            if (s.X == e.X && s.Y == e.Y && s.Z == e.Z)
                throw new ArgumentException("start and end must be different points.");
        }
    }
}
