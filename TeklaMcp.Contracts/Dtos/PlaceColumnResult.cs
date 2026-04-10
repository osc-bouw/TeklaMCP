namespace TeklaMcp.Contracts.Dtos
{
    public class PlaceColumnResult
    {
        public bool Success { get; set; }
        public string? Guid { get; set; }
        public int ObjectId { get; set; }
        public string? Message { get; set; }
    }
}
