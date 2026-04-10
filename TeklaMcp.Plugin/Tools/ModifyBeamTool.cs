using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class ModifyBeamTool
    {
        private readonly TeklaModelFacade _facade;

        public ModifyBeamTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "ModifyBeam",
            Description = "Modifies an existing beam in Tekla Structures by GUID. Only the provided properties are changed; omitted properties keep their current values.",
            InputSchema = new
            {
                type     = "object",
                required = new[] { "guid" },
                properties = new
                {
                    guid     = new { type = "string",  description = "GUID of the beam to modify." },
                    profile  = new { type = "string",  description = "New profile string (e.g. HEA200)." },
                    material = new { type = "string",  description = "New material string (e.g. S235JR)." },
                    @class   = new { type = "integer", description = "New class number." },
                    name     = new { type = "string",  description = "New name." },
                    start    = new { type = "object",  description = "New start point in mm.", properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                    end      = new { type = "object",  description = "New end point in mm.",   properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                    position = new
                    {
                        type = "object",
                        description = "Position overrides. Only provided fields are changed.",
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
        };

        public ModifyBeamResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeRequired<ModifyBeamArgs>(body, "Invalid ModifyBeam arguments.");

            if (string.IsNullOrWhiteSpace(args.Guid))
                throw new ArgumentException("guid is required and must not be empty.");

            var result = ToolExecution.Run(
                () => _facade.ModifyBeam(args),
                message => new ModifyBeamResult { Success = false, Message = message });

            McpAuditLog.Write("ModifyBeam", args, result.Success, args.Guid, result.Message);
            return result;
        }
    }
}
