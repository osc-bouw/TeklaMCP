using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class FilterModelArgs
    {
        public string? Name { get; set; }
        public string? Profile { get; set; }
        public string? Material { get; set; }
        public int? Class { get; set; }
        public string? Type { get; set; } // Beam, Column, Plate, etc.
        public Dictionary<string, string>? UserDefinedAttributes { get; set; }
        public int? Limit { get; set; }
    }
}
