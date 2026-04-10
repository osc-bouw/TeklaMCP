using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class OverlapGroup
    {
        /// <summary>The duplicated mark (e.g. "P1" or "A3").</summary>
        public string Mark { get; set; } = "";

        /// <summary>"part" or "assembly"</summary>
        public string NumberType { get; set; } = "";

        /// <summary>GUIDs of all parts sharing this mark.</summary>
        public List<string> Guids { get; set; } = new List<string>();

        /// <summary>Basic info about each part (name, profile, material).</summary>
        public List<OverlapPartInfo> Parts { get; set; } = new List<OverlapPartInfo>();
    }

    public class OverlapPartInfo
    {
        public string Guid { get; set; } = "";
        public string Name { get; set; } = "";
        public string Profile { get; set; } = "";
        public string Material { get; set; } = "";
        public string Type { get; set; } = "";
    }

    public class OverlappingNumberingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int TotalPartsScanned { get; set; }
        public int OverlapGroupCount { get; set; }
        public List<OverlapGroup> OverlapGroups { get; set; } = new List<OverlapGroup>();
    }
}
