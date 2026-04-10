using System.Text.Json;
using TeklaMcp.Contracts.Dtos;
using TeklaMcp.Server.Logging;
using TeklaMcp.Server.Tekla;

namespace TeklaMcp.Server.Tools
{
    public class UserSelectedBeamTool
    {
        private readonly TeklaModelFacade _facade;

        public UserSelectedBeamTool(TeklaModelFacade facade) => _facade = facade;

        public McpToolDefinition GetDefinition() => new McpToolDefinition
        {
            Name = "UserSelectedBeam",
            Description = "Select a Beam in Tekla.",
            InputSchema = new
            {
                type = "object"
            }
        };

        public UserSelectedBeamResult Invoke(JsonElement body)
        {
            var result = ToolExecution.Run(
                () => _facade.GetInfoFromUserSelectedBeam(),
                message => new UserSelectedBeamResult { Success = false, Message = message });

            McpAuditLog.Write("UserSelectedBeam", null, result.Success, result.Beam.Guid, result.Message);
            return result;
        }
    }
}
