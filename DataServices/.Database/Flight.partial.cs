using DataServices.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderBy;

namespace DataServices.Database
{
    [MetadataType(typeof(FlightMetadata))]
    public partial class Flight
    {
    }

	public class FlightMetadata
    {
		[OrderBy]
        public int FlightId { get; set; }

        [OrderBy]
		public string FlightNumber { get; set; }
    }
}
