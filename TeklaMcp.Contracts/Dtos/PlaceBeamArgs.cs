namespace TeklaMcp.Contracts.Dtos
{
    public class PlaceBeamArgs
    {
        public Point3 Start    { get; set; } = new Point3();
        public Point3 End      { get; set; } = new Point3();
        public string Profile  { get; set; } = "";
        public string? Material { get; set; }
        public int?    Class   { get; set; }
        public string? Name    { get; set; }
        public PositionArgs? Position { get; set; }
    }

    /// <summary>
    /// Optional position overrides. Each string field must match the Tekla enum name exactly.
    /// Omitted fields fall back to defaults (Plane=MIDDLE, Depth=BEHIND, Rotation=FRONT).
    /// </summary>
    public class PositionArgs
    {
        /// <summary>LEFT | MIDDLE | RIGHT</summary>
        public string? Plane { get; set; }
        public double? PlaneOffset { get; set; }

        /// <summary>FRONT | MIDDLE | BEHIND</summary>
        public string? Depth { get; set; }
        public double? DepthOffset { get; set; }

        /// <summary>FRONT | TOP | BACK | BOTTOM</summary>
        public string? Rotation { get; set; }
        public double? RotationOffset { get; set; }
    }
}
