using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class PlaceColumnTool
    {
        private readonly TeklaModelFacade _facade;

        public PlaceColumnTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "PlaceColumn",
            Description = "Places a vertical column upward from a base point.",
            InputSchema = new
            {
                type     = "object",
                required = new[] { "base", "height", "profile" },
                properties = new
                {
                    @base    = new { type = "object", properties = new { x = new { type = "number" }, y = new { type = "number" }, z = new { type = "number" } } },
                    height   = new { type = "number", exclusiveMinimum = 0 },
                    profile  = new { type = "string" },
                    material = new { type = "string" },
                    @class   = new { type = "integer" },
                    name     = new { type = "string" }
                }
            }
        };

        public PlaceColumnResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeRequired<PlaceColumnArgs>(body, "Invalid PlaceColumn arguments.");

            Validate(args);

            var result = ToolExecution.Run(
                () => _facade.InsertColumn(args),
                message => new PlaceColumnResult { Success = false, Message = message });

            McpAuditLog.Write("PlaceColumn", args, result.Success, result.Guid, result.Message);
            return result;
        }

        private static void Validate(PlaceColumnArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Profile))
                throw new ArgumentException("profile is required and must not be empty.");

            if (args.Height <= 0)
                throw new ArgumentException("height must be > 0.");
        }
    }
}
