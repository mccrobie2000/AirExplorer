using VirtualRadarServer.Models;
using OrderBy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices.Models
{
    public class AirportDTO
    {
        public AirportDTO()
        {
        }

        public AirportDTO(Airport airport)
        {
            AirportId = airport.AirportId;
            Icao = airport.Icao;
            Iata = airport.Iata;
            Name = airport.Name;
            Location = airport.Location;
            CountryId = airport.CountryId;
            CountryName = airport.Country.Name;
            Latitude = airport.Latitude;
            Longitude = airport.Longitude;
            Altitude = airport.Altitude;
        }

        public static string[] Includes { get { return new string[] { nameof(Country) }; } }

        public long AirportId { get; internal set; }
        [DisplayName("ICAO")]
        public string Icao { get; internal set; }
        [DisplayName("IATA")]
        public string Iata { get; internal set; }
        [DisplayName("Name")]
        public string Name { get; internal set; }
        public string Location { get; internal set; }
        public long CountryId { get; internal set; }
        [DisplayName("Country Name")]
        [OrderByName("CountryId")]
        public string CountryName { get; internal set; }
        public double? Latitude { get; internal set; }
        public double? Longitude { get; internal set; }
        public double? Altitude { get; internal set; }
    }
}
