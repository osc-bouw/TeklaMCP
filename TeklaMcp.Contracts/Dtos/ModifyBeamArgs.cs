namespace TeklaMcp.Contracts.Dtos
{
    public class ModifyBeamArgs
    {
        public string Guid { get; set; } = "";

        // All properties below are optional — only provided ones are applied.
        public string? Profile   { get; set; }
        public string? Material  { get; set; }
        public int?    Class     { get; set; }
        public string? Name      { get; set; }
        public Point3? Start     { get; set; }
        public Point3? End       { get; set; }
        public PositionArgs? Position { get; set; }
    }
}
