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
    public class AirportModel
    {
        [UIHint("AirportInformation")]
        public AirportDTO Airport { get; set; }

        public AirportModel()
        {
        }

        public async Task Load(AirportBusinessService dataServices, int? airportId)
        {
            if (airportId.HasValue)
            {
                Airport = await dataServices.GetAirport(airportId.Value);
            }
        }
    }
}