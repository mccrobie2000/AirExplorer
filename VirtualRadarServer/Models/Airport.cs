using System;
using System.Collections.Generic;

namespace VirtualRadarServer.Models
{
    public partial class Airport
    {
        public long AirportId { get; set; }
        public string Icao { get; set; }
        public string Iata { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public long CountryId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long? Altitude { get; set; }

        public Country Country { get; set; }
    }
}
