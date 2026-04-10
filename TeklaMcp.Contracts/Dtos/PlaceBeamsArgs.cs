using System.Collections.Generic;

namespace TeklaMcp.Contracts.Dtos
{
    public class PlaceBeamsArgs
    {
        public List<PlaceBeamArgs> Beams { get; set; } = new List<PlaceBeamArgs>();
    }
}
