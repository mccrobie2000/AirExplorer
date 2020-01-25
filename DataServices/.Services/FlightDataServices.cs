using DataServices.Configuration;
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

namespace DataServices.Services
{
    public class OrderByAttribute : Attribute
    {
    }

    public class FlightDataServices
    {
        static object sCache;
        static object iCache;
        static object fCache;

        public FlightDataServices()
        {
        }

        private Expression<Func<T, TKey>> GetFieldAccessExpression<T, TKey>(string fieldName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var field = Expression.Lambda<Func<T, TKey>>(Expression.Convert(Expression.Property(parameter, fieldName), typeof(TKey)), parameter);
            return field;
        }

        private IList<string> GetOrderByPropertiesFromType<T, TKey>()
        {
            IList<string> properties = new List<string>();

            Type t = typeof(T);
            Type tkey = typeof(TKey);

            MetadataTypeAttribute[] metadataAttributes = (MetadataTypeAttribute[])t.GetCustomAttributes(typeof(MetadataTypeAttribute), true);

            foreach (var pi in t.GetProperties())
            {
                if (pi.PropertyType == tkey && OrderByAttribute.IsDefined(pi, typeof(OrderByAttribute)))
                    properties.Add(pi.Name);
                else
                    foreach (var attribute in metadataAttributes)
                    {
                        t = attribute.MetadataClassType;
                        var otherpi = t.GetProperty(pi.Name);
                        if (otherpi != null && pi.PropertyType == tkey && OrderByAttribute.IsDefined(otherpi, typeof(OrderByAttribute)))
                        {
                            properties.Add(pi.Name);
                            break;
                        }
                    }
            }

            return properties;
        }

        private Type GetPropertyType<T>(string propertyName)
        {
            Type t = typeof(T);
            var propertyType = t.GetProperties().FirstOrDefault(p => p.Name == propertyName)?.PropertyType;
            return propertyType;
        }

        private List<Tuple<string, bool>> GetTypeConfiguration<T>()
        {
            Type t = typeof(T);

            List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();

            var orderBySection = (OrderBySection) System.Configuration.ConfigurationManager.GetSection("orderBy");
            foreach (var clazz in orderBySection.Classes)
            {
                if (t.FullName == clazz.Type)
                {
                    foreach (var field in clazz.Fields)
                    {
                        list.Add(new Tuple<string, bool>(field.Name, field.OrderBy));
                    }
                }
            }

            return list;
        }

        private void AdjustFromConfiguration<T>(IList<string> stringProperties, IList<string> intProperties, IList<string> floatProperties)
        {
            // read the configuration file for the type hive
            // it will have a list of fields which specify canOrder and cannotOrder
            // for cannotOrder, remove the property name from all three lists
            // for canOrder, hunt down the property, determine its type, and add to the appropriate list

            List<Tuple<string, bool>> configuration = GetTypeConfiguration<T>();

            foreach (var c in configuration)
            {
                if (c.Item2)
                {
                    var propertyType = GetPropertyType<T>(c.Item1);

                    if (propertyType == typeof(string) && !stringProperties.Contains(c.Item1)) stringProperties.Add(c.Item1);
                    else if (propertyType == typeof(int) && !intProperties.Contains(c.Item1)) intProperties.Add(c.Item1);
                    else if (propertyType == typeof(double) && !floatProperties.Contains(c.Item1)) floatProperties.Add(c.Item1);
                }
                else
                {
                    stringProperties.Remove(c.Item1);
                    intProperties.Remove(c.Item1);
                    floatProperties.Remove(c.Item1);
                }
            }
        }

        private void LoadOrderBy<T>(
            out Dictionary<string, Expression<Func<T, string>>> orderByStrings,
            out Dictionary<string, Expression<Func<T, int>>> orderByInts,
            out Dictionary<string, Expression<Func<T, double>>> orderByFloats)
        {
            orderByStrings = new Dictionary<string, Expression<Func<T, string>>>();
            orderByInts = new Dictionary<string, Expression<Func<T, int>>>();
            orderByFloats = new Dictionary<string, Expression<Func<T, double>>>();

            IList<string> stringProperties = GetOrderByPropertiesFromType<T, string>();
            IList<string> intProperties = GetOrderByPropertiesFromType<T, int>();
            IList<string> floatProperties = GetOrderByPropertiesFromType<T, double>();

            // Now, adjust the list by the configuration file
            AdjustFromConfiguration<T>(stringProperties, intProperties, floatProperties);

            foreach (var propertyName in stringProperties)
            {
                orderByStrings.Add(propertyName, GetFieldAccessExpression<T, string>(propertyName));
            }
            foreach (var propertyName in intProperties)
            {
                orderByInts.Add(propertyName, GetFieldAccessExpression<T, int>(propertyName));
            }
            foreach (var propertyName in floatProperties)
            {
                orderByFloats.Add(propertyName, GetFieldAccessExpression<T, double>(propertyName));
            }
        }

        private void GetOrderByDictionaries<T>(out Dictionary<string, Expression<Func<T, string>>> stringSorts,
            out Dictionary<string, Expression<Func<T, int>>> intSorts,
            out Dictionary<string, Expression<Func<T, double>>> floatSorts)
        {
            // Get our cache of types / order by fields
            Dictionary<Type, object> stringCache = sCache as Dictionary<Type, object>;
            Dictionary<Type, object> intCache = iCache as Dictionary<Type, object>;
            Dictionary<Type, object> floatCache = fCache as Dictionary<Type, object>;

            // Cache not set up yet, do it now
            if (stringCache == null)
            {
                stringCache = new Dictionary<Type, object>();
                sCache = stringCache;
            }
            if (intCache == null)
            {
                intCache = new Dictionary<Type, object>();
                iCache = intCache;
            }
            if (floatCache == null)
            {
                floatCache = new Dictionary<Type, object>();
                fCache = floatCache;
            }

            // Do we know the orderBy's for the type?
            if (!stringCache.Keys.Contains(typeof(T)) || !intCache.Keys.Contains(typeof(T)))
            {
                // No, load them now

                LoadOrderBy<T>(out stringSorts, out intSorts, out floatSorts);

                stringCache.Add(typeof(T), stringSorts);
                intCache.Add(typeof(T), intSorts);
                floatCache.Add(typeof(T), floatSorts);
            }
            else
            {
                // Yes, get them from the cache
                stringSorts = stringCache[typeof(T)] as Dictionary<string, Expression<Func<T, string>>>;
                intSorts = intCache[typeof(T)] as Dictionary<string, Expression<Func<T, int>>>;
                floatSorts = floatCache[typeof(T)] as Dictionary<string, Expression<Func<T, double>>>;
            }
        }

        public IList<string> GetOrderByFields<T>()
        {
            Dictionary<string, Expression<Func<T, string>>> stringSorts;
            Dictionary<string, Expression<Func<T, int>>> intSorts;
            Dictionary<string, Expression<Func<T, double>>> floatSorts;

            GetOrderByDictionaries<T>(out stringSorts, out intSorts, out floatSorts);

            List<string> fields = new List<string>();

            fields.AddRange(stringSorts.Keys);
            fields.AddRange(intSorts.Keys);
            fields.AddRange(floatSorts.Keys);

            return fields;
        }

        private IQueryable<T> OrderBy<T>(IQueryable<T> query, string orderByField, bool ascending)
        {
            Dictionary<string, Expression<Func<T, string>>> stringSorts;
            Dictionary<string, Expression<Func<T, int>>> intSorts;
            Dictionary<string, Expression<Func<T, double>>> floatSorts;

            GetOrderByDictionaries<T>(out stringSorts, out intSorts, out floatSorts);

            IQueryable<T> result = null;

            if (stringSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.OrderBy(stringSorts[orderByField]);
                else
                    result = query.OrderByDescending(stringSorts[orderByField]);
            }
            else if (intSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.OrderBy(intSorts[orderByField]);
                else
                    result = query.OrderByDescending(intSorts[orderByField]);
            }
            else if (floatSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.OrderBy(floatSorts[orderByField]);
                else
                    result = query.OrderByDescending(floatSorts[orderByField]);
            }
            else
                result = query;

            return result;
        }

        public IList<Location> GetLocations(string orderBy = "", int? offset = null, int? take = null)
        {
            return GetLocations(orderBy, true, offset, take);
        }

        public IList<Location> GetLocations(string orderBy = "", bool ascending = true, int? offset = null, int? take = null)
        {
            List<Location> locations = new List<Location>();

            using (AirContainer context = new AirContainer())
            {
                var query = context.Locations.Select(s => s);

                query = OrderBy(query, orderBy, ascending);

                if (offset.HasValue && take.HasValue)
                {
                    query = query.Skip(offset.Value).Take(take.Value);
                }

                var list = query.ToList();

                locations.AddRange(list);
            }

            return locations;
        }

        public IList<Flight> GetFlights(string orderBy = "", int? offset = null, int? take = null)
        {
            return GetFlights(orderBy, true, offset, take);
        }

        public IList<Flight> GetFlights(string orderBy = "", bool ascending = true, int? offset = null, int? take = null)
        {
            List<Flight> flights = new List<Flight>();

            using (AirContainer context = new AirContainer())
            {
                var query = context.Flights.Select(s => s);

                query = OrderBy(query, orderBy, ascending);

                if (offset.HasValue && take.HasValue)
                {
                    query = query.Skip(offset.Value).Take(take.Value);
                }

                var list = query.ToList();

                flights.AddRange(list);
            }

            return flights;
        }

        public IList<Flight> GetFlightsFrom(string orderBy = "", bool ascending = true, int? offset = null, int? take = null)
        {
            List<Flight> flights = new List<Flight>();

            using (AirContainer context = new AirContainer())
            {
                var query = context.Flights.Select(s => s);

                query = query.Where(f => f.FlightId > 3);

                query = OrderBy(query, orderBy, ascending);

                if (offset.HasValue && take.HasValue)
                {
                    query = query.Skip(offset.Value).Take(take.Value);
                }

                var list = query.ToList();

                flights.AddRange(list);
            }

            return flights;
        }
    }
}
