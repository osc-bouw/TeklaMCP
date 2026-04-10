namespace TeklaMcp.Contracts.Dtos
{
    public class PlaceColumnArgs
    {
        public Point3  Base    { get; set; } = new Point3();
        public double  Height  { get; set; }
        public string  Profile { get; set; } = "";
        public string? Material { get; set; }
        public int?    Class   { get; set; }
        public string? Name    { get; set; }
    }
}
