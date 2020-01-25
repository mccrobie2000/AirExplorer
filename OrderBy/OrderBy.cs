using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace OrderBy
{
    public class OrderByHelper<TModel>
    {
        private static object sCache;
        private static object iCache;
        static private object fCache;

        private Expression<Func<TModel, TKey>> GetFieldAccessExpression<TKey>(string fieldName)
        {
            var parameter = Expression.Parameter(typeof(TModel));
            var field = Expression.Lambda<Func<TModel, TKey>>(Expression.Convert(Expression.Property(parameter, fieldName), typeof(TKey)), parameter);
            return field;
        }

        private IList<string> GetOrderByPropertiesFromType<TKey>()
        {
            IList<string> properties = new List<string>();

            Type t = typeof(TModel);
            Type tkey = typeof(TKey);

            ModelMetadataTypeAttribute[] metadataAttributes = (ModelMetadataTypeAttribute[])t.GetCustomAttributes(typeof(ModelMetadataTypeAttribute), true);

            foreach (var pi in t.GetProperties())
            {
                if (pi.PropertyType == tkey && Attribute.IsDefined(pi, typeof(OrderByAttribute)))
                    properties.Add(pi.Name);
                else
                    foreach (var attribute in metadataAttributes)
                    {
                        t = attribute.MetadataType;
                        var otherpi = t.GetProperty(pi.Name);
                        if (otherpi != null && pi.PropertyType == tkey && Attribute.IsDefined(otherpi, typeof(OrderByAttribute)))
                        {
                            properties.Add(pi.Name);
                            break;
                        }
                    }
            }

            return properties;
        }

        private Type GetPropertyType(string propertyName)
        {
            Type t = typeof(TModel);
            var propertyType = t.GetProperties().FirstOrDefault(p => p.Name == propertyName)?.PropertyType;
            return propertyType;
        }

        private List<Tuple<string, bool>> GetTypeConfiguration()
        {
            Type t = typeof(TModel);

            List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();

            var orderBySection = (OrderBySection)System.Configuration.ConfigurationManager.GetSection("orderBy");
            if (orderBySection != null)
            {
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
            }

            return list;
        }

        private void AdjustFromConfiguration(IList<string> stringProperties, IList<string> intProperties, IList<string> floatProperties)
        {
            // read the configuration file for the type hive
            // it will have a list of fields which specify canOrder and cannotOrder
            // for cannotOrder, remove the property name from all three lists
            // for canOrder, hunt down the property, determine its type, and add to the appropriate list

            List<Tuple<string, bool>> configuration = GetTypeConfiguration();

            foreach (var c in configuration)
            {
                if (c.Item2)
                {
                    var propertyType = GetPropertyType(c.Item1);

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

        private void LoadOrderBy(
            out Dictionary<string, Expression<Func<TModel, string>>> orderByStrings,
            out Dictionary<string, Expression<Func<TModel, int>>> orderByInts,
            out Dictionary<string, Expression<Func<TModel, double>>> orderByFloats)
        {
            orderByStrings = new Dictionary<string, Expression<Func<TModel, string>>>();
            orderByInts = new Dictionary<string, Expression<Func<TModel, int>>>();
            orderByFloats = new Dictionary<string, Expression<Func<TModel, double>>>();

            IList<string> stringProperties = GetOrderByPropertiesFromType<string>();
            IList<string> intProperties = GetOrderByPropertiesFromType<int>();
            IList<string> floatProperties = GetOrderByPropertiesFromType<double>();

            // Now, adjust the list by the configuration file
            AdjustFromConfiguration(stringProperties, intProperties, floatProperties);

            foreach (var propertyName in stringProperties)
            {
                orderByStrings.Add(propertyName, GetFieldAccessExpression<string>(propertyName));
            }
            foreach (var propertyName in intProperties)
            {
                orderByInts.Add(propertyName, GetFieldAccessExpression<int>(propertyName));
            }
            foreach (var propertyName in floatProperties)
            {
                orderByFloats.Add(propertyName, GetFieldAccessExpression<double>(propertyName));
            }
        }

        private void GetOrderByDictionaries(out Dictionary<string, Expression<Func<TModel, string>>> stringSorts,
            out Dictionary<string, Expression<Func<TModel, int>>> intSorts,
            out Dictionary<string, Expression<Func<TModel, double>>> floatSorts)
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
            if (!stringCache.Keys.Contains(typeof(TModel)) || !intCache.Keys.Contains(typeof(TModel)))
            {
                // No, load them now

                LoadOrderBy(out stringSorts, out intSorts, out floatSorts);

                stringCache.Add(typeof(TModel), stringSorts);
                intCache.Add(typeof(TModel), intSorts);
                floatCache.Add(typeof(TModel), floatSorts);
            }
            else
            {
                // Yes, get them from the cache
                stringSorts = stringCache[typeof(TModel)] as Dictionary<string, Expression<Func<TModel, string>>>;
                intSorts = intCache[typeof(TModel)] as Dictionary<string, Expression<Func<TModel, int>>>;
                floatSorts = floatCache[typeof(TModel)] as Dictionary<string, Expression<Func<TModel, double>>>;
            }
        }

        public IList<string> GetOrderByFields()
        {
            Dictionary<string, Expression<Func<TModel, string>>> stringSorts;
            Dictionary<string, Expression<Func<TModel, int>>> intSorts;
            Dictionary<string, Expression<Func<TModel, double>>> floatSorts;

            GetOrderByDictionaries(out stringSorts, out intSorts, out floatSorts);

            List<string> fields = new List<string>();

            fields.AddRange(stringSorts.Keys);
            fields.AddRange(intSorts.Keys);
            fields.AddRange(floatSorts.Keys);

            return fields;
        }

        public IOrderedQueryable<TModel> ThenBy(IOrderedQueryable<TModel> query, string orderByField, string ascending)
        {
            ascending = ascending?.ToUpper() ?? "ASC";
            return ThenBy(query, orderByField, ascending == "ASC");
        }

        public IOrderedQueryable<TModel> ThenBy(IOrderedQueryable<TModel> query, string orderByField, bool ascending)
        {
            Dictionary<string, Expression<Func<TModel, string>>> stringSorts;
            Dictionary<string, Expression<Func<TModel, int>>> intSorts;
            Dictionary<string, Expression<Func<TModel, double>>> floatSorts;

            GetOrderByDictionaries(out stringSorts, out intSorts, out floatSorts);

            IOrderedQueryable<TModel> result = null;

            if (stringSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.ThenBy(stringSorts[orderByField]);
                else
                    result = query.ThenByDescending(stringSorts[orderByField]);
            }
            else if (intSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.ThenBy(intSorts[orderByField]);
                else
                    result = query.ThenByDescending(intSorts[orderByField]);
            }
            else if (floatSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.ThenBy(floatSorts[orderByField]);
                else
                    result = query.ThenByDescending(floatSorts[orderByField]);
            }
            else
                result = query;

            return result;
        }

        public IOrderedQueryable<TModel> OrderBy(IQueryable<TModel> query, string orderByField, string ascending)
        {
            ascending = ascending?.ToUpper() ?? "ASC";
            return OrderBy(query, orderByField, ascending == "ASC");
        }

        public IOrderedQueryable<TModel> OrderBy(IQueryable<TModel> query, string orderByField, bool ascending)
        {
            Dictionary<string, Expression<Func<TModel, string>>> stringSorts;
            Dictionary<string, Expression<Func<TModel, int>>> intSorts;
            Dictionary<string, Expression<Func<TModel, double>>> floatSorts;

            GetOrderByDictionaries(out stringSorts, out intSorts, out floatSorts);

            IOrderedQueryable<TModel> result = null;

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
            {
                result = (IOrderedQueryable<TModel>)((IQueryable<TModel>) query);
            }

            return result;
        }
    }
}
