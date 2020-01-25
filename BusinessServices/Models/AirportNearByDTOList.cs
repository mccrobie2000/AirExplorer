using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices.Models
{
    public class AirportNearByDTOList
    {
        public int TotalRecords { get; set; }
        public IList<AirportNearByDTO> Airports { get; set; }
    }
}
