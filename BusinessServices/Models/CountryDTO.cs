using VirtualRadarServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices.Models
{
    public class CountryDTO
    {
        public CountryDTO()
        {
        }

        public CountryDTO(Country country)
        {
            CountryId = country.CountryId;
            Name = country.Name;

            Airports = country.Airports.Select(a => new AirportDTO(a)).ToList();
        }

        public static string[] Includes { get { return new string[] { nameof(Airports) }; } }

        public long CountryId { get; set; }
        public string Name { get; set; }
        public IList<AirportDTO> Airports { get; set; }
    }
}
