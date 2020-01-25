using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices.Models
{
    public class AirportDTOList
    {
        public int TotalRecords { get; set; }
        public IList<AirportDTO> Airports { get; set; }
    }
}
