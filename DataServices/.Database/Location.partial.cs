using DataServices.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices.Database
{
    [MetadataType(typeof(LocationMetadata))]
    public partial class Location
    {
    }

    public class LocationMetadata
    {
        //[OrderBy]
        public string Name { get; set; }

        //[OrderBy]
        public float Latitude { get; set; }

        //[OrderBy]
        public float Longitude { get; set; }
    }
}
