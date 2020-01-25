using VirtualRadarServer.Models;
using DataServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices.Models
{
    public class AirportNearByDTO : AirportDTO
    {
        public double Distance { get; set; }

        public AirportNearByDTO()
        {
        }

        public AirportNearByDTO(Airport airport, double distance) : base(airport)
        {
            Distance = distance;
        }
    }
}
