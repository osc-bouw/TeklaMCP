using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class DeleteObjectsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<DeleteObjectItemResult> Results { get; set; } = new List<DeleteObjectItemResult>();
    }

    public class DeleteObjectItemResult
    {
        public string Guid { get; set; } = "";
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
