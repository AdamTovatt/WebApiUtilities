using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Sakur.WebApiUtilities.BaseClasses
{
    /// <summary>
    /// Request body for a web api request
    /// </summary>
    public abstract class RequestBody
    {
        /// <summary>
        /// Should be overridden to tell if a request is valid or not
        /// </summary>
        [JsonIgnore]
        public abstract bool Valid { get; }

        /// <summary>
        /// Will return a string containing information about which fields are missing
        /// </summary>
        /// <returns></returns>
        public string GetInvalidBodyMessage(bool allowEmptyStrings = false)
        {
            StringBuilder stringBuilder = new StringBuilder("Missing fields in request body: ");

            foreach (string property in GetMissingProperties(allowEmptyStrings))
            {
                stringBuilder.Append(property.FirstCharToLowerCase());
                stringBuilder.Append(", ");
            }

            return stringBuilder.Remove(stringBuilder.Length - 2, 2).ToString();
        }

        /// <summary>
        /// Will return a list of property names for the properties that are missing in the body
        /// </summary>
        /// <returns>A list of strings - the names of the missing properties (fields) in the body</returns>
        public List<string> GetMissingProperties(bool allowEmptyStrings = false)
        {
            List<string> properties = new List<string>();
            PropertyInfo[] propertyInfos = GetType().GetProperties();

            foreach (PropertyInfo property in propertyInfos)
            {
                bool hasJsonIgnoreProperty = property.IsDefined(typeof(JsonIgnoreAttribute), false);

                if (!hasJsonIgnoreProperty && !HasValue(allowEmptyStrings, property))
                    properties.Add(property.Name);
            }

            return properties;
        }

        /// <summary>
        /// Used to automatically validate the request body based on the Required attribute.
        /// </summary>
        /// <returns>Wether or not all the required properties were provided</returns>
        public bool ValidateByRequiredAttributes(bool allowEmptyStrings = false)
        {
            PropertyInfo[] requiredProperties = GetType().GetProperties().Where(x => x.IsDefined(typeof(RequiredAttribute), false)).ToArray();

            foreach (PropertyInfo property in requiredProperties)
            {
                if (!HasValue(allowEmptyStrings, property)) return false;
            }

            return true;
        }

        private bool HasValue(bool allowEmptyStrings, PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
            {
                string? value = (string?)property.GetValue(this);

                if (value == null || (!allowEmptyStrings && value == string.Empty))
                    return false;
            }
            else if (property.GetValue(this) == GetDefault(property.PropertyType))
                return false;

            return true;
        }

        private static object? GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
