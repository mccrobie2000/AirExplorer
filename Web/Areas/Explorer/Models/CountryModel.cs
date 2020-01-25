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
    public class CountryModel
    {
        [UIHint("CountryInformation")]
        public CountryDTO Country { get; set; }

        public CountryModel()
        {
        }

        public async Task Load(AirportBusinessService dataServices, long countryId)
        {
            Country = await dataServices.GetCountryWithAirports(countryId);
        }
    }
}
