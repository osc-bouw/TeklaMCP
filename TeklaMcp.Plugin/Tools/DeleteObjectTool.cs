using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class DeleteObjectTool
    {
        private readonly TeklaModelFacade _facade;

        public DeleteObjectTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "DeleteObject",
            Description = "Deletes a model object in Tekla Structures by its GUID.",
            InputSchema = new
            {
                type     = "object",
                required = new[] { "guid" },
                properties = new
                {
                    guid = new { type = "string", description = "The GUID of the model object to delete." }
                }
            }
        };

        public DeleteObjectResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeRequired<DeleteObjectArgs>(body, "Invalid DeleteObject arguments.");

            if (string.IsNullOrWhiteSpace(args.Guid))
                throw new ArgumentException("guid is required and must not be empty.");

            var result = ToolExecution.Run(
                () => _facade.DeleteObject(args),
                message => new DeleteObjectResult { Success = false, Message = message });

            McpAuditLog.Write("DeleteObject", args, result.Success, args.Guid, result.Message);
            return result;
        }
    }
}
