using System;
using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class FilterModelTool
    {
        private readonly TeklaModelFacade _facade;

        public FilterModelTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name        = "FilterModel",
            Description = "Filters model objects based on criteria like name, profile, material, class, or type. This is used for Information Take Off (ITO).",
            InputSchema = new
            {
                type = "object",
                properties = new
                {
                    name = new { type = "string", description = "Filter by object name (contains)." },
                    profile = new { type = "string", description = "Filter by profile string (contains)." },
                    material = new { type = "string", description = "Filter by material string (contains)." },
                    @class = new { type = "integer", description = "Filter by class (exact match)." },
                    type = new { type = "string", description = "Filter by object type (e.g. Beam, Column, Plate)." },
                    userDefinedAttributes = new
                    {
                        type = "object",
                        additionalProperties = new { type = "string" },
                        description = "Filter by User Defined Attributes (UDA) key-value pairs."
                    },
                    limit = new { type = "integer", description = "Maximum number of objects to return. Hard cap is 500." }
                }
            }
        };

        public FilterModelResult Invoke(JsonElement body)
        {
            var args = ToolExecution.DeserializeOrDefault<FilterModelArgs>(body);

            var result = ToolExecution.Run(
                () => _facade.FilterModel(args),
                message => new FilterModelResult { Success = false, Message = message });

            McpAuditLog.Write("FilterModel", args, result.Success, null, result.Message);
            return result;
        }
    }
}
