using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class DeleteObjectsTool
    {
        private readonly TeklaModelFacade _facade;

        public DeleteObjectsTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "DeleteObjects",
            Description = "Deletes multiple model objects in Tekla Structures by their GUIDs.",
            InputSchema = new
            {
                type     = "object",
                required = new[] { "guids" },
                properties = new
                {
                    guids = new
                    {
                        type        = "array",
                        items       = new { type = "string" },
                        description = "List of GUIDs of the model objects to delete."
                    }
                }
            }
        };

        public DeleteObjectsResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeRequired<DeleteObjectsArgs>(body, "Invalid DeleteObjects arguments.");

            if (args.Guids == null || args.Guids.Count == 0)
                throw new ArgumentException("guids must contain at least one entry.");

            var result = ToolExecution.Run(
                () => _facade.DeleteObjects(args),
                message => new DeleteObjectsResult { Success = false, Message = message });

            McpAuditLog.Write("DeleteObjects", args, result.Success, null, result.Message);
            return result;
        }
    }
}
