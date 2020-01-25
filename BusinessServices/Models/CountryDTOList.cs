using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices.Models
{
    public class CountryDTOList
    {
        public int TotalRecords { get; set; }
        public IList<CountryDTO> Countries { get; set; }
    }
}
