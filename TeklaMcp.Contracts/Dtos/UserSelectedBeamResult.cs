using System;
using System.Collections.Generic;
using System.Text;

namespace TeklaMcp.Contracts.Dtos
{
    public class UserSelectedBeamResult
    {
        public bool Success { get; set; }


        public string Message { get; set; } = "";
        public BeamDto Beam { get; set; } = new BeamDto();

    }

    public class BeamDto
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

        public double Length { get; set; }

        public double width { get; set; }

        public double height { get; set; }

        public Dictionary<string, object>? UserDefinedAttributes { get; set; }
    }
}
