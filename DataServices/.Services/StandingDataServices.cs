using DataServices.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OrderBy;
using LonelySharp;
using DataServices.Database.VirtualRadarServer;

namespace DataServices.VirtualRadarServer.StandingData
{
    public class StandingDataServices
    {
        public StandingDataServices()
        {
        }

        public IList<Airport> GetAirports(string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            orderDirection = orderDirection?.ToUpper() ?? "ASC";
            return GetAirports(orderBy, orderDirection == "ASC", offset, take);
        }

        public IList<Airport> GetAirports(string orderBy = "", bool ascending = true, int? offset = null, int? take = null)
        {
            List<Airport> airports = new List<Airport>();

            using (StandingDataContainer context = new StandingDataContainer())
            {
                IQueryable<Airport> query = context.Airports.OrderBy(orderBy, ascending);
                if (offset.HasValue && take.HasValue)
                {
                    query = query.Skip(offset.Value).Take(take.Value);
                }
                airports.AddRange(query.ToList());
            }

            return airports;
        }

        public IList<string> GetOrderByFields<T>()
        {
            return OrderByExtensions.GetOrderByFields<T>();
        }

        public IList<Airport> GetAirportsNearBy(double latitude, double longitude, double radius)
        {
            var geolocation = GeoLocation.FromDegrees(latitude, longitude);
            var bounding = geolocation.BoundingCoordinates(radius);

            double minLatitude = bounding[0].getLatitudeInDegrees();
            double minLongitude = bounding[0].getLongitudeInDegrees();
            double maxLatitude = bounding[1].getLatitudeInDegrees();
            double maxLongitude = bounding[1].getLongitudeInDegrees();

            List<Airport> locations = new List<Airport>();

            using (StandingDataContainer context = new StandingDataContainer())
            {
                IQueryable<Airport> query = context.Airports
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

                locations.AddRange(query.Select(s => s).ToList());
            }

            return locations;
        }
    }
}
