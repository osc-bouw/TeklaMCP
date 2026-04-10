namespace TeklaMcp.Contracts.Dtos
{
    public class OverlappingNumberingArgs
    {
        /// <summary>"part", "assembly", or "both" (default)</summary>
        public string CheckType { get; set; } = "both";

        /// <summary>Max number of overlap groups to return (default 200).</summary>
        public int? Limit { get; set; }
    }
}
