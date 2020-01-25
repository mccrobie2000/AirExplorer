//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataServices.Database.VirtualRadarServer
{
    using System;
    using System.Collections.Generic;
    
    public partial class Operator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Operator()
        {
            this.Routes = new HashSet<Route>();
        }
    
        public long OperatorId { get; set; }
        public string Icao { get; set; }
        public string Iata { get; set; }
        public string Name { get; set; }
        public string PositioningFlightPattern { get; set; }
        public string CharterFlightPattern { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Route> Routes { get; set; }
    }
}
