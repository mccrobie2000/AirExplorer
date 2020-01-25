using System;
using System.Collections.Generic;

namespace VirtualRadarServer.Models
{
    public partial class Country
    {
        public Country()
        {
            Airports = new HashSet<Airport>();
        }

        public long CountryId { get; set; }
        public string Name { get; set; }

        public ICollection<Airport> Airports { get; set; }
    }
}
