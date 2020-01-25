using BusinessServices;
using BusinessServices.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Web.Areas.Explorer.Models
{
    public class AirportsNearByModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public IList<AirportNearByDTO> Airports { get; set; }

        public async Task Load(AirportBusinessService airportBusinessServices, double latitude, double longitude, double radius)
        {
            Latitude = latitude;
            Longitude = longitude;

            var list = await airportBusinessServices.GetAirportsNearBy(latitude, longitude, radius);
            Airports = list.Airports;
        }
    }
}
