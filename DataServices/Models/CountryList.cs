using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadarServer.Models;

namespace DataServices.Models
{
    public class CountryList
    {
        public int TotalRecords { get; set; }
        public IList<Country> Countries { get; set; }
    }
}
