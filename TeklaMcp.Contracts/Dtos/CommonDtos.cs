namespace TeklaMcp.Contracts.Dtos
{
    public class Point3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class McpToolDefinition
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public object InputSchema { get; set; } = new object();
    }
}
