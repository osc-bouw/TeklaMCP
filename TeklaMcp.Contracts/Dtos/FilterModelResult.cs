using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class PositionDto
    {
        public string Plane { get; set; } = "";
        public double PlaneOffset { get; set; }
        public string Depth { get; set; } = "";
        public double DepthOffset { get; set; }
        public string Rotation { get; set; } = "";
        public double RotationOffset { get; set; }
    }

    public class NumberingSeriesDto
    {
        public string Prefix { get; set; } = "";
        public int StartNumber { get; set; }
    }

    

    public class ModelObjectDto
    {
        public string Guid { get; set; } = "";
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string Profile { get; set; } = "";
        public string Material { get; set; } = "";
        public string Class { get; set; } = "";
        public string Finish { get; set; } = "";
        public string CastUnitType { get; set; } = "";
        
        public NumberingSeriesDto? AssemblyNumber { get; set; }
        public NumberingSeriesDto? PartNumber { get; set; }
        public PositionDto? Position { get; set; }

        // Geometry for Beams/Columns
        public Point3? StartPoint { get; set; }
        public Point3? EndPoint { get; set; }

        public Dictionary<string, object>? UserDefinedAttributes { get; set; }
    }

    public class FilterModelResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<ModelObjectDto> Objects { get; set; } = new List<ModelObjectDto>();
    }
}
