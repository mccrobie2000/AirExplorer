using BusinessServices.Models;
using DataServices;
using VirtualRadarServer.Models;
using LonelySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessServices
{
    public class AirportBusinessService
    {
        protected IDataServices DataServices { get; set; }

        public AirportBusinessService(IDataServices dataServices)
        {
            DataServices = dataServices;
        }

        public async Task<AirportDTO> GetAirport(long airportId)
        {
            var airport = await DataServices.GetAirport(airportId, AirportDTO.Includes);

            return new AirportDTO(airport);
        }

        public async Task<AirportDTOList> GetAirportsAsync(string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            var airportList = await DataServices.GetAirports(AirportDTO.Includes, orderBy, orderDirection, offset, take);

            AirportDTOList airportDTOList = new AirportDTOList
            {
                Airports = airportList.Airports.Select(a => new AirportDTO(a)).ToList(),
                TotalRecords = airportList.TotalRecords
            };

            return airportDTOList;
        }

        public async Task<AirportDTOList> GetAirports(string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            var airportList = await DataServices.GetAirports(AirportDTO.Includes, orderBy, orderDirection, offset, take);

            AirportDTOList airportDTOList = new AirportDTOList()
            {
                Airports = airportList.Airports.Select(a => new AirportDTO(a)).ToList(),
                TotalRecords = airportList.TotalRecords
            };

            return airportDTOList;
        }

        public async Task<AirportDTOList> GetAirports(long countryId, string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            var airportList = await DataServices.GetAirports(countryId, AirportDTO.Includes, orderBy, orderDirection, offset, take);

            AirportDTOList list = new AirportDTOList
            {
                TotalRecords = airportList.TotalRecords,
                Airports = airportList.Airports.Select(a => new AirportDTO(a)).ToList()
            };

            return list;
        }

        public async Task<CountryDTOList> GetCountries(string orderBy = "", string orderDirection = "ASC", int? offset = null, int? take = null)
        {
            var countryList = await DataServices.GetCountries(CountryDTO.Includes, orderBy, orderDirection, offset, take);

            CountryDTOList list = new CountryDTOList
            {
                TotalRecords = countryList.TotalRecords,
                Countries = countryList.Countries.Select(c => new CountryDTO(c)).ToList()
            };

            return list;
        }

        public async Task<CountryDTO> GetCountryWithAirports(long countryId)
        {
            var country = await DataServices.GetCountry(countryId, CountryDTO.Includes);

            return new CountryDTO(country);
        }

        public async Task<AirportNearByDTOList> GetAirportsNearBy(double latitude, double longitude, double radius)
        {
            /*
             * We make a bounding square to search for airports.  The corners of the square may contain
             * airports just outside of the radius.  We will exclude those when we calculate the distance.
             */

            AirportNearByDTOList nearByList = new AirportNearByDTOList();

            var geolocation = GeoLocation.FromDegrees(latitude, longitude);
            var bounding = geolocation.BoundingCoordinates(radius);

            double minLatitude = bounding[0].getLatitudeInDegrees();
            double minLongitude = bounding[0].getLongitudeInDegrees();
            double maxLatitude = bounding[1].getLatitudeInDegrees();
            double maxLongitude = bounding[1].getLongitudeInDegrees();

            var airportList = await DataServices.GetAirportsNearBy(minLatitude, minLongitude, maxLatitude, maxLongitude, AirportDTO.Includes);

            nearByList.TotalRecords = airportList.TotalRecords;
            nearByList.Airports = new List<AirportNearByDTO>();

            GeoLocation center = GeoLocation.FromDegrees(latitude, longitude);

            foreach (var airport in airportList.Airports)
            {
                double distance = CalculateDistance(airport, center);
                if (distance <= radius)
                {
                    nearByList.Airports.Add(new AirportNearByDTO(airport, distance));
                }
            }

            return nearByList;
        }

        public double CalculateDistance(Airport airport, GeoLocation center)
        {
            double distance = -1;
            if (airport.Latitude.HasValue && airport.Longitude.HasValue)
            {
                GeoLocation airportLocation = GeoLocation.FromDegrees(airport.Latitude.Value, airport.Longitude.Value);
                distance = center.DistanceTo(airportLocation);
            }
            return distance;
        }
    }
}
