using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class PlaceBeamsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<PlaceBeamResult> Results { get; set; } = new List<PlaceBeamResult>();
    }
}
