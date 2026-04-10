using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class DeleteObjectsArgs
    {
        public List<string> Guids { get; set; } = new List<string>();
    }
}
