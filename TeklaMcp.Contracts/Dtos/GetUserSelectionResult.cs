using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class GetUserSelectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<ModelObjectDto> Objects { get; set; } = new List<ModelObjectDto>();
    }
}
