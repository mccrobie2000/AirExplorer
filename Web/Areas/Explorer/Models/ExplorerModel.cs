using BusinessServices;
using BusinessServices.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Web.Areas.Explorer.Models
{
    public class ExplorerModel
    {
        public IList<AirportDTO> Airports { get; set; }
        public IList<CountryDTO> Countries { get; set; }

        public ExplorerModel()
        {
            Airports = new List<AirportDTO>();
            Countries = new List<CountryDTO>();
        }

        public void Load(AirportBusinessService dataServices)
        {
        }
    }
}