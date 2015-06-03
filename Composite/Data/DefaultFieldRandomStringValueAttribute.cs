﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Composite.C1Console.Events;
using Composite.Core.Linq;
using Composite.Core.Types;
using Composite.Core.WebClient;

namespace Composite.Data
{
    /// <summary>
    /// Sets the field's value to a random base64 string value of the specified length.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DefaultFieldRandomStringValueAttribute : Attribute
    {
        private readonly bool _checkCollisions;

        private static readonly ConcurrentDictionary<Type, IEnumerable<RandomStringValueProperty>> ReflectionCache =
            new ConcurrentDictionary<Type, IEnumerable<RandomStringValueProperty>>();

        internal int Length { get; private set; }

        private class RandomStringValueProperty
        {
            public PropertyInfo Property;
            public DefaultFieldRandomStringValueAttribute Attribute;
            public bool IsKey;
        }

        /// <summary>
        /// Sets the field's value to a random base64 string value of the specified length.
        /// </summary>
        /// <param name="length">The length of a generated random string. Allowed range is [3..22].</param>
        /// <param name="checkCollisions">When set to 2, the inserted value will be checked for a collision.</param>
        public DefaultFieldRandomStringValueAttribute(int length = 8, bool checkCollisions = false)
        {
            Verify.ArgumentCondition(length >= 3, "length", "Minimum allowed length is 3 characters");
            Verify.ArgumentCondition(length <= 22, "length", "Maximum allowed length is 22 characters, which is an equivalent to a Guid value");

            Length = length;
            _checkCollisions = checkCollisions;
        }

        static DefaultFieldRandomStringValueAttribute()
        {
            DataEvents<IData>.OnBeforeAdd += OnBeforeAddData;
            GlobalEventSystemFacade.SubscribeToFlushEvent(a => ReflectionCache.Clear());
        }

        private static void OnBeforeAddData(object sender, DataEventArgs dataEventArgs)
        {
            var data = dataEventArgs.Data;
            var interfaceType = data.DataSourceId.InterfaceType;

            var fieldsToFill = GetRandomStringProperties(interfaceType);

            foreach (var field in fieldsToFill)
            {
                var value = field.Property.GetValue(data, null);
                if (value != null) continue;

                string randomString = GenerateRandomString(field.Attribute.Length);

                if (field.Attribute._checkCollisions)
                {
                    bool uniqueValueFound = false;

                    const int tries = 2;
                    for (int i = 0; i < tries; i++)
                    {
                        if (!ValueIsInUse(interfaceType, field.Property, randomString, field.IsKey))
                        {
                            uniqueValueFound = true;
                            break;
                        }

                        randomString = GenerateRandomString(field.Attribute.Length);
                    }

                    if (!uniqueValueFound)
                    {
                        Verify.That(uniqueValueFound, "Failed to generate a unique random string value after {0} tries. Field name: {1}, random value length: {2}",
                            tries, field.Property.Name, field.Attribute.Length);
                    }
                }

                field.Property.SetValue(data, randomString);
            }
        }

        private static bool ValueIsInUse(Type interfaceType, PropertyInfo property, string value, bool isKeyField)
        {
            if (isKeyField)
            {
                var keyCollection = new DataKeyPropertyCollection();
                keyCollection.AddKeyProperty(property, value);

                return DataFacade.TryGetDataByUniqueKey(interfaceType, keyCollection) != null;
            }

            var parameter = Expression.Parameter(interfaceType, "data");
            var predicateExpression = ExpressionHelper.CreatePropertyPredicate(parameter, new[]
            {
                new Tuple<PropertyInfo, object>(property, value)
            });

            Type delegateType = typeof(Func<,>).MakeGenericType(new [] { interfaceType, typeof(bool) });

            LambdaExpression lambdaExpression = Expression.Lambda(delegateType, predicateExpression, new [] { parameter });

            MethodInfo methodInfo = DataFacade.GetGetDataWithPredicatMethodInfo(interfaceType);

            IQueryable queryable = (IQueryable)methodInfo.Invoke(null, new object[] { lambdaExpression });

            return queryable.OfType<IData>().Any();
        }

        private static string GenerateRandomString(int length)
        {
            return UrlUtils.CompressGuid(Guid.NewGuid()).Substring(0, length);
        }

        private static IEnumerable<RandomStringValueProperty> GetRandomStringProperties(Type interfaceType)
        {
            return ReflectionCache.GetOrAdd(interfaceType, type =>
            {
                List<RandomStringValueProperty> result = null;

                var stringProperties = type.GetPropertiesRecursively().Where(property => property.PropertyType == typeof(string));
                foreach (var property in stringProperties)
                {
                    var attribute = property.GetCustomAttribute<DefaultFieldRandomStringValueAttribute>(true);
                    if (attribute == null) continue;

                    result = result ?? new List<RandomStringValueProperty>();

                    var keyPropertyNames = type.GetKeyPropertyNames();

                    bool isKey = keyPropertyNames.Count == 1 && keyPropertyNames[0] == property.Name;

                    result.Add(new RandomStringValueProperty { Attribute = attribute, Property = property, IsKey = isKey});
                }

                return result ?? Enumerable.Empty<RandomStringValueProperty>();
            });
        }
    }
}
