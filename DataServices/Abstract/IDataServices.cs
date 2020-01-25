using VirtualRadarServer.Models;
using DataServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServices
{
    public interface IDataServices
    {
        Task<CountryList> GetCountries(string[] includes = null, string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null);
        Task<AirportList> GetAirports(string[] includes = null, string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null);
        Task<AirportList> GetAirports(long countryId, string[] includes = null, string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null);
        Task<Airport> GetAirport(long airportId, string[] includes = null);
        Task<Country> GetCountry(long countryId, string[] includes = null);
        Task<AirportList> GetAirportsNearBy(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude, string[] includes = null);
    }
}
