using OrderBy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Database.VirtualRadarServer
{
    [MetadataType(typeof(CountryMetadata))]
    public partial class Country
    {
    }

    //TODO chuck - change OrderBy so that all properties are orderable except those marked - do we even care if they're orderable?
    //TODO chuck - maybe have webcontrols just default to sortable if property in the orderby list except those that are marked NOT Orderable
    public class CountryMetadata
    {
        [OrderBy]
        public string Name { get; set; }
    }
}
