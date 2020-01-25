using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadarServer.Models;

namespace DataServices.Models
{
    public class AirportList
    {
        public int TotalRecords { get; set; }
        public IList<Airport> Airports { get; set; }
    }
}
