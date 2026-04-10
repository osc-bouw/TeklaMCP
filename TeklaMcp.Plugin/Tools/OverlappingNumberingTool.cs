using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class OverlappingNumberingTool
    {
        private readonly TeklaModelFacade _facade;

        public OverlappingNumberingTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "OverlappingNumbering",
            Description = "Scans the Tekla model for parts that share the same part mark or assembly mark (overlapping/duplicate numbering). Returns groups of objects with identical marks so QA issues can be identified.",
            InputSchema = new
            {
                type       = "object",
                required   = new string[0],
                properties = new
                {
                    checkType = new
                    {
                        type        = "string",
                        description = "Which mark type to check: \"part\", \"assembly\", or \"both\" (default).",
                        @enum       = new[] { "part", "assembly", "both" }
                    },
                    limit = new
                    {
                        type        = "integer",
                        description = "Maximum number of overlap groups to return (default 200)."
                    }
                }
            }
        };

        public OverlappingNumberingResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeOrDefault<OverlappingNumberingArgs>(body);

            var result = ToolExecution.Run(
                () => _facade.GetOverlappingNumbering(args),
                message => new OverlappingNumberingResult { Success = false, Message = message });

            McpAuditLog.Write("OverlappingNumbering", args, result.Success, null, result.Message);
            return result;
        }
    }
}
