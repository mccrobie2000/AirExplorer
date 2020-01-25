using DataServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadarServer.Models;
using OrderBy;
using System.Linq.Expressions;
using LonelySharp;
using Microsoft.EntityFrameworkCore;

namespace DataServices
{
    /*
     * EntityFrameworkCore does not (yet) support multiple parallel asynchronous operations on the
     * same DbContext.  Use await for each operation.  See: https://docs.microsoft.com/en-us/ef/core/querying/async
     */
    public class DataServices : IDataServices
    {
        protected StandingDataContainer Context { get; set; }

        public DataServices(StandingDataContainer context)
        {
            Context = context;
        }

        public async Task<Airport> GetAirport(long airportId, string[] includes = null)
        {
            var task = Context.Airports.Include(includes).Where(a => a.AirportId == airportId).FirstOrDefaultAsync();

            await task;

            return task.Result;
        }

        public async Task<AirportList> GetAirports(string[] includes, string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            AirportList list = new AirportList();

            IQueryable<Airport> query = Context.Airports.Include(includes).IncludeOrderByJoins(orderBy).OrderBy(orderBy, orderDirection);

            list.TotalRecords = await query.CountAsync();

            if (offset.HasValue && take.HasValue)
            {
                query = query.Skip(offset.Value).Take(take.Value);
            }

            list.Airports = await query.ToListAsync();

            return list;
        }

        public async Task<AirportList> GetAirports(long countryId, string[] includes, string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            AirportList list = new AirportList();

            List<Airport> airports = new List<Airport>();

            IQueryable<Airport> query = Context.Airports.Include(includes).IncludeOrderByJoins(orderBy).Where(a => a.CountryId == countryId).OrderBy(orderBy, orderDirection);

            list.TotalRecords = await query.CountAsync();

            if (offset.HasValue && take.HasValue)
            {
                query = query.Skip(offset.Value).Take(take.Value);
            }

            list.Airports = await query.ToListAsync();

            return list;
        }

        public async Task<Country> GetCountry(long countryId, string[] includes = null)
        {
            var country = await Context.Countries.Include(includes).Where(c => c.CountryId == countryId).FirstOrDefaultAsync();

            return country;
        }

        public async Task<CountryList> GetCountries(string[] includes, string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            CountryList countryList = new CountryList();

            IQueryable<Country> query = Context.Countries.Include(includes).IncludeOrderByJoins(orderBy).OrderBy(orderBy, orderDirection);

            countryList.TotalRecords = await query.CountAsync();

            if (offset.HasValue && take.HasValue)
            {
                query = query.Skip(offset.Value).Take(take.Value);
            }

            countryList.Countries = await query.ToListAsync();

            return countryList;
        }

        /// <summary>
        /// Returns all airports withing the radius of the given latitude/longitude.  The airports are
        /// sorted by distance from the given point.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="radius"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public async Task<AirportList> GetAirportsNearBy(double minLatitude, double minLongitude, double maxLatitude, double maxLongitude, string[] includes = null)
        {
            AirportList list = new AirportList();

            IQueryable<Airport> query = Context.Airports.Include(includes)
                .Where(l => l.Latitude >= minLatitude && l.Latitude <= maxLatitude);

            if (minLongitude <= maxLongitude)
            {
                query = query
                    .Where(l => l.Longitude >= minLongitude && l.Longitude <= maxLongitude);
            }
            else
            {
                query = query
                    .Where(l => l.Longitude >= minLongitude || l.Longitude <= maxLongitude);
            }

            list.TotalRecords = await query.CountAsync();

            list.Airports = await query.ToListAsync();

            return list;
        }
    }
}
