using OrderBy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Database.VirtualRadarServer
{
    [MetadataType(typeof(AirportMetadata))]
    public partial class Airport
    {
    }

    //TODO chuck - change OrderBy so that all properties are orderable except those marked - do we even care if they're orderable?
    //TODO chuck - maybe have webcontrols just default to sortable if property in the orderby list except those that are marked NOT Orderable
    public class AirportMetadata
    {
        [OrderBy]
        public string Icao { get; set; }
        [OrderBy]
        public string Iata { get; set; }
        [OrderBy]
        public string Name { get; set; }
        [OrderBy]
        public string Location { get; set; }
        [OrderBy]
        public double? Latitude { get; set; }
        [OrderBy]
        public double? Longitude { get; set; }
        [OrderBy(Include = typeof(Country), JoinField = nameof(Country.Name))]
        public long CountryId { get; set; }
    }
}
