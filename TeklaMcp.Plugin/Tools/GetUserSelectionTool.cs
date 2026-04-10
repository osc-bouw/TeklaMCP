using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class GetUserSelectionTool
    {
        private readonly TeklaModelFacade _facade;

        public GetUserSelectionTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "GetUserSelection",
            Description = "Returns the list of objects currently selected by the user in Tekla Structures.",
            InputSchema = new { type = "object" }
        };

        public GetUserSelectionResult Invoke(JsonElement body)
        {
            var result = ToolExecution.Run(
                () => _facade.GetUserSelection(),
                message => new GetUserSelectionResult { Success = false, Message = message });

            McpAuditLog.Write("GetUserSelection", null, result.Success, null, result.Message);
            return result;
        }
    }
}
