using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace OrderBy
{
    /// <summary>
    /// Decorates a property as being sortable.
    /// </summary>
    public class OrderByAttribute : Attribute
    {
        public Type Include { get; set; }
        public string JoinField { get; set; }
    }

    public class OrderByNameAttribute : Attribute
    {
        public OrderByNameAttribute(string name) { Name = name; }
        public string Name { get; set; }
    }

    internal class OrderByJoin<TModel, TProperty>
    {
        public Type Include { get; set; }
        public string JoinField { get; set; }
        public OrderByAttribute OrderBy { get; set; }
        public string FullName { get; set; }
        public Expression<Func<TModel, TProperty>> Expression { get; set; }
    }

    /// <summary>
    /// Helpers for IQueryable OrderBy
    /// </summary>
    public static class OrderByExtensions
    {
        private static object sCache;
        private static object iCache;
        private static object fCache;
        private static object lCache;
        private static object jCache;

        private static Expression<Func<TModel, TKey>> GetFieldAccessExpression<TModel, TKey>(string fieldName)
        {
            var parameter = Expression.Parameter(typeof(TModel));
            var field = Expression.Lambda<Func<TModel, TKey>>(Expression.Convert(Expression.Property(parameter, fieldName), typeof(TKey)), parameter);
            return field;
        }

        private static Expression GetJoinFieldAccessExpression<TModel>(Type join, string fieldName)
        {
            Type tmodel = typeof(TModel);

            var inputParameter = Expression.Parameter(tmodel);

            var outerAccess = Expression.Property(inputParameter, join.Name);
            var innerAccess = Expression.Property(outerAccess, fieldName);

            Type onType = GetPropertyType(join, fieldName);
            var field = Expression.Lambda(Expression.Convert(innerAccess, onType), inputParameter);
            return field;
        }

        private static IList<string> GetOrderByPropertiesFromType<TModel, TKey>(IDictionary<string, OrderByAttribute> myOrderByJoins = null)
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
                            OrderByAttribute orderByAttribute = Attribute.GetCustomAttribute(otherpi, typeof(OrderByAttribute)) as OrderByAttribute;
                            if (orderByAttribute != null && orderByAttribute.Include != null)
                            {
                                if (myOrderByJoins != null)
                                {
                                    myOrderByJoins.Add(pi.Name, orderByAttribute);
                                }
                            }
                            else //TODO chuck : change this to look for OrderByJoin attribute, not OrderBy for everything :(
                            {
                                properties.Add(pi.Name);
                            }
                            break;
                        }
                    }
            }

            return properties;
        }

        private static Type GetPropertyType<TModel>(string propertyName)
        {
            Type t = typeof(TModel);
            return GetPropertyType(t, propertyName);
        }

        private static Type GetPropertyType(Type type, string propertyName)
        {
            var propertyType = type.GetProperties().FirstOrDefault(p => p.Name == propertyName)?.PropertyType;
            return propertyType;
        }

        private static List<Tuple<string, bool>> GetTypeConfiguration<TModel>()
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

        private static void AdjustFromConfiguration<TModel>(IList<string> stringProperties,
            IList<string> intProperties,
            IList<string> floatProperties,
            IList<string> longProperties,
            IDictionary<string, OrderByAttribute> myOrderByJoins)
        {
            // read the configuration file for the type hive
            // it will have a list of fields which specify canOrder and cannotOrder
            // for cannotOrder, remove the property name from all three lists
            // for canOrder, hunt down the property, determine its type, and add to the appropriate list

            List<Tuple<string, bool>> configuration = GetTypeConfiguration<TModel>();

            foreach (var c in configuration)
            {
                if (c.Item2)
                {
                    var propertyType = GetPropertyType<TModel>(c.Item1);

                    if (propertyType == typeof(string) && !stringProperties.Contains(c.Item1)) stringProperties.Add(c.Item1);
                    else if (propertyType == typeof(int) && !intProperties.Contains(c.Item1)) intProperties.Add(c.Item1);
                    else if (propertyType == typeof(double) && !floatProperties.Contains(c.Item1)) floatProperties.Add(c.Item1);
                    else if (propertyType == typeof(long) && !floatProperties.Contains(c.Item1)) longProperties.Add(c.Item1);

                    // TODO chuck : We could add an OrderByJoin from the configuration file by getting the type/member, constructing an OrderByAttribute and inserting it
                }
                else
                {
                    stringProperties.Remove(c.Item1);
                    intProperties.Remove(c.Item1);
                    floatProperties.Remove(c.Item1);
                    longProperties.Remove(c.Item1);
                    myOrderByJoins.Remove(c.Item1);
                }
            }
        }

        private static void LoadOrderBy<TModel>(
            out Dictionary<string, Expression<Func<TModel, string>>> orderByStrings,
            out Dictionary<string, Expression<Func<TModel, int>>> orderByInts,
            out Dictionary<string, Expression<Func<TModel, double>>> orderByFloats,
            out Dictionary<string, Expression<Func<TModel, long>>> orderByLongs,
            out Dictionary<string, OrderByJoin<TModel, string>> orderByJoins)
        {
            orderByStrings = new Dictionary<string, Expression<Func<TModel, string>>>();
            orderByInts = new Dictionary<string, Expression<Func<TModel, int>>>();
            orderByFloats = new Dictionary<string, Expression<Func<TModel, double>>>();
            orderByLongs = new Dictionary<string, Expression<Func<TModel, long>>>();
            orderByJoins = new Dictionary<string, OrderByJoin<TModel, string>>();

            Dictionary<string, OrderByAttribute> myJoins = new Dictionary<string, OrderByAttribute>();

            IList<string> stringProperties = GetOrderByPropertiesFromType<TModel, string>(myJoins);
            IList<string> intProperties = GetOrderByPropertiesFromType<TModel, int>(myJoins);
            IList<string> floatProperties = GetOrderByPropertiesFromType<TModel, double>(myJoins);
            IList<string> longProperties = GetOrderByPropertiesFromType<TModel, long>(myJoins);

            // Now, adjust the list by the configuration file
            AdjustFromConfiguration<TModel>(stringProperties, intProperties, floatProperties, longProperties, myJoins);

            foreach (var propertyName in stringProperties)
            {
                orderByStrings.Add(propertyName, GetFieldAccessExpression<TModel, string>(propertyName));
            }
            foreach (var propertyName in intProperties)
            {
                orderByInts.Add(propertyName, GetFieldAccessExpression<TModel, int>(propertyName));
            }
            foreach (var propertyName in floatProperties)
            {
                orderByFloats.Add(propertyName, GetFieldAccessExpression<TModel, double>(propertyName));
            }
            foreach (var propertyName in longProperties)
            {
                orderByLongs.Add(propertyName, GetFieldAccessExpression<TModel, long>(propertyName));
            }
            foreach (var kvp in myJoins)
            {
                var untypedExpression = GetJoinFieldAccessExpression<TModel>(kvp.Value.Include, kvp.Value.JoinField);

                var type = typeof(Func<,>).MakeGenericType(typeof(TModel), typeof(string));
                var etype = typeof(Expression<>).MakeGenericType(type);

                dynamic expression = Convert.ChangeType(untypedExpression, etype);
                var fullName = $"{kvp.Value.Include.Name}.{kvp.Value.JoinField}";
                var orderByJoin = new OrderByJoin<TModel, string> { Include = kvp.Value.Include, JoinField = kvp.Value.JoinField, Expression = expression, FullName = fullName };
                orderByJoins.Add(kvp.Key, orderByJoin);
            }
        }

        private static void GetOrderByDictionaries<TModel>(out Dictionary<string, Expression<Func<TModel, string>>> stringSorts,
            out Dictionary<string, Expression<Func<TModel, int>>> intSorts,
            out Dictionary<string, Expression<Func<TModel, double>>> floatSorts,
            out Dictionary<string, Expression<Func<TModel, long>>> longSorts,
            out Dictionary<string, OrderByJoin<TModel, string>> joinSorts)
        {
            // Get our cache of types / order by fields
            Dictionary<Type, object> stringCache = sCache as Dictionary<Type, object>;
            Dictionary<Type, object> intCache = iCache as Dictionary<Type, object>;
            Dictionary<Type, object> floatCache = fCache as Dictionary<Type, object>;
            Dictionary<Type, object> longCache = lCache as Dictionary<Type, object>;
            Dictionary<Type, object> joinCache = jCache as Dictionary<Type, object>;

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
            if (longCache == null)
            {
                longCache = new Dictionary<Type, object>();
                lCache = longCache;
            }
            if (joinCache == null)
            {
                joinCache = new Dictionary<Type, object>();
                jCache = joinCache;
            }

            // Do we know the orderBy's for the type?
            if (!stringCache.Keys.Contains(typeof(TModel)) || !intCache.Keys.Contains(typeof(TModel)))
            {
                // No, load them now

                LoadOrderBy(out stringSorts, out intSorts, out floatSorts, out longSorts, out joinSorts);

                stringCache.Add(typeof(TModel), stringSorts);
                intCache.Add(typeof(TModel), intSorts);
                floatCache.Add(typeof(TModel), floatSorts);
                longCache.Add(typeof(TModel), longSorts);
                joinCache.Add(typeof(TModel), joinSorts);
            }
            else
            {
                // Yes, get them from the cache
                stringSorts = stringCache[typeof(TModel)] as Dictionary<string, Expression<Func<TModel, string>>>;
                intSorts = intCache[typeof(TModel)] as Dictionary<string, Expression<Func<TModel, int>>>;
                floatSorts = floatCache[typeof(TModel)] as Dictionary<string, Expression<Func<TModel, double>>>;
                longSorts = longCache[typeof(TModel)] as Dictionary<string, Expression<Func<TModel, long>>>;
                joinSorts = joinCache[typeof(TModel)] as Dictionary<string, OrderByJoin<TModel, string>>;
            }
        }

        /// <summary>
        /// Returns all properties marked as OrderBy either via <code>OrderByAttribute</code>
        /// or via configuration.  See <code>Configuration.OrderBySection</code>.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public static IList<string> GetOrderByFields<TModel>()
        {
            Dictionary<string, Expression<Func<TModel, string>>> stringSorts;
            Dictionary<string, Expression<Func<TModel, int>>> intSorts;
            Dictionary<string, Expression<Func<TModel, double>>> floatSorts;
            Dictionary<string, Expression<Func<TModel, long>>> longSorts;
            Dictionary<string, OrderByJoin<TModel, string>> joinSorts;

            GetOrderByDictionaries(out stringSorts, out intSorts, out floatSorts, out longSorts, out joinSorts);

            List<string> fields = new List<string>();

            fields.AddRange(stringSorts.Keys);
            fields.AddRange(intSorts.Keys);
            fields.AddRange(floatSorts.Keys);
            fields.AddRange(longSorts.Keys);

            foreach (var kvp in joinSorts)
            {
                fields.Add(kvp.Value.FullName);
            }

            return fields;
        }

        /// <summary>
        /// Orders an IQueryable by string and direction.  <paramref name="orderByField"/> may be empty.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderByField"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TModel> OrderBy<TModel>(this IQueryable<TModel> query, string orderByField, string ascending)
        {
            ascending = ascending?.ToUpper() ?? "ASC";
            return OrderBy(query, orderByField, ascending == "ASC");
        }

        public static IQueryable<TModel> Include<TModel>(this DbSet<TModel> dbSet, string[] includes) where TModel : class
        {
            IQueryable<TModel> dbQuery = dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    dbQuery = dbQuery.Include(include);
                }
            }

            return dbQuery;
        }

        public static IQueryable<TModel> IncludeOrderByJoins<TModel>(this IQueryable<TModel> dbQuery, string orderByField = "") where TModel : class
        {
            if (!string.IsNullOrEmpty(orderByField))
            {
                Dictionary<string, Expression<Func<TModel, string>>> stringSorts;
                Dictionary<string, Expression<Func<TModel, int>>> intSorts;
                Dictionary<string, Expression<Func<TModel, double>>> floatSorts;
                Dictionary<string, Expression<Func<TModel, long>>> longSorts;
                Dictionary<string, OrderByJoin<TModel, string>> joinSorts;

                orderByField = orderByField ?? string.Empty;

                GetOrderByDictionaries(out stringSorts, out intSorts, out floatSorts, out longSorts, out joinSorts);

                OrderByJoin<TModel, string> orderByJoin;
                if (joinSorts.TryGetValue(orderByField, out orderByJoin))
                {
                    dbQuery = dbQuery.Include(orderByJoin.Include.Name);
                }
            }

            return dbQuery;
        }

        /// <summary>
        /// Orders an IQueryable by string and direction.  <paramref name="orderByField"/> may be empty.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderByField"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TModel> OrderBy<TModel>(this IQueryable<TModel> query, string orderByField, bool ascending)
        {
            Dictionary<string, Expression<Func<TModel, string>>> stringSorts;
            Dictionary<string, Expression<Func<TModel, int>>> intSorts;
            Dictionary<string, Expression<Func<TModel, double>>> floatSorts;
            Dictionary<string, Expression<Func<TModel, long>>> longSorts;
            Dictionary<string, OrderByJoin<TModel, string>> joinSorts;

            orderByField = orderByField ?? string.Empty;

            GetOrderByDictionaries(out stringSorts, out intSorts, out floatSorts, out longSorts, out joinSorts);

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
            else if (longSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.OrderBy(longSorts[orderByField]);
                else
                    result = query.OrderByDescending(longSorts[orderByField]);
            }
            else if (joinSorts.Keys.Contains(orderByField))
            {
                var expression = joinSorts[orderByField].Expression;
                if (ascending)
                    result = query.OrderBy(expression);
                else
                    result = query.OrderByDescending(expression);
            }
            else
            {
                //result = query.OrderBy(o => 0);
                result = (IOrderedQueryable<TModel>) query;
            }

            return result;
        }

        /// <summary>
        /// Performs subsequent orders on an IOrderedQueryable.  <paramref name="orderByField"/> may be empty.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderByField"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TModel> ThenBy<TModel>(this IOrderedQueryable<TModel> query, string orderByField, string ascending)
        {
            ascending = ascending?.ToUpper() ?? "ASC";
            return ThenBy(query, orderByField, ascending == "ASC");
        }

        /// <summary>
        /// Performs subsequent orders on an IOrderedQueryable.  <paramref name="orderByField"/> may be empty.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query"></param>
        /// <param name="orderByField"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static IOrderedQueryable<TModel> ThenBy<TModel>(this IOrderedQueryable<TModel> query, string orderByField, bool ascending)
        {
            Dictionary<string, Expression<Func<TModel, string>>> stringSorts;
            Dictionary<string, Expression<Func<TModel, int>>> intSorts;
            Dictionary<string, Expression<Func<TModel, double>>> floatSorts;
            Dictionary<string, Expression<Func<TModel, long>>> longSorts;
            Dictionary<string, OrderByJoin<TModel, string>> joinSorts;

            GetOrderByDictionaries(out stringSorts, out intSorts, out floatSorts, out longSorts, out joinSorts);

            orderByField = orderByField ?? string.Empty;

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
            else if (longSorts.Keys.Contains(orderByField))
            {
                if (ascending)
                    result = query.ThenBy(longSorts[orderByField]);
                else
                    result = query.ThenByDescending(longSorts[orderByField]);
            }
            else
                result = query.ThenBy(o => 0);

            return result;
        }
    }

    #region Obsolete
    internal class xWebControlUtilities
    {
        public static PropertyInfo GetPropertyInfo<T>(Expression expression)
        {
            Type type = typeof(T);

            MemberExpression member = expression as MemberExpression;

            PropertyInfo propertyInfo = member.Member as PropertyInfo;

            return propertyInfo;
        }

        public static string GetDisplayName(PropertyInfo propertyInfo)
        {
            var displayName = propertyInfo.Name;

            var attributes = propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (attributes.Length > 0)
            {
                var displayNameAttribute = attributes[0] as DisplayNameAttribute;
                displayName = displayNameAttribute.DisplayName;
            }

            return displayName;
        }

        public static void GetPropertyParameters(Expression expression, out string propertyName, out string displayName, out Type type)
        {
            propertyName = "";
            displayName = "";
            type = typeof(Object);

            RecurseForProperty(expression, out propertyName, out displayName, out type);
        }

        private static void RecurseForProperty(Expression expression, out string propertyName, out string displayName, out Type type)
        {
            propertyName = "";
            displayName = "";
            type = typeof(Object);

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    propertyName = memberExpression.Member is PropertyInfo ? memberExpression.Member.Name : null;
                    displayName = memberExpression.Member is PropertyInfo ? GetDisplayName(memberExpression.Member as PropertyInfo) : null;
                    type = memberExpression.Type;
                    break;
                case ExpressionType.Convert:
                    var unary = expression as UnaryExpression;
                    if (unary != null)
                    {
                        RecurseForProperty(unary.Operand, out propertyName, out displayName, out type);
                    }
                    break;
            }
        }
    }
    #endregion

}
