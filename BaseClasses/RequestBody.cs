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

            PropertyInfo[] requiredProperties = GetType().GetProperties().Where(x => x.IsDefined(typeof(RequiredAttribute), false)).ToArray();

            if (requiredProperties.Length > 0)
            {
                foreach (PropertyInfo property in requiredProperties)
                {
                    object? value = GetValue(allowEmptyStrings, property);

                    if (value == null)
                    {
                        properties.Add(property.Name);
                    }
                    else // It does have a value but maybe it's not allowed, we have to check that
                    {
                        RequiredAttribute requiredAttribute = property.GetCustomAttribute<RequiredAttribute>()!;
                        if (requiredAttribute.DisallowedValue != null && value.Equals(requiredAttribute.DisallowedValue))
                        {
                            properties.Add(property.Name); // We add it if a dissallowed value is specified and matches with the value of the property
                        }
                    }
                }
            }
            else
            {
                foreach (PropertyInfo property in propertyInfos)
                {
                    if (property.Name == "Valid" || property.IsDefined(typeof(JsonIgnoreAttribute), false))
                        continue;

                    if (GetValue(allowEmptyStrings, property) == null)
                        properties.Add(property.Name);
                }
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
                object? value = GetValue(allowEmptyStrings, property);

                if (value == null)
                {
                    return false;
                }
                else // It does have a value but maybe it's not allowed, we have to check that
                {
                    RequiredAttribute requiredAttribute = property.GetCustomAttribute<RequiredAttribute>()!;
                    if (requiredAttribute.DisallowedValue != null && value.Equals(requiredAttribute.DisallowedValue))
                    {
                        return false; // We say it is not valid if a dissallowed value is specified and matches with the value of the property
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Will get a value, checking if it exists. If it isn't considered to exist null will be returned, otherwise the value will be returned.
        /// </summary>
        /// <param name="allowEmptyStrings">Wether to allow empty strings or not.</param>
        /// <param name="property">The property to get the value of.</param>
        /// <returns>The value of it exists, null otherwise.</returns>
        private object? GetValue(bool allowEmptyStrings, PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
            {
                string? value = (string?)property.GetValue(this);

                if (value == null || (!allowEmptyStrings && value == string.Empty))
                    return null;

                return value;
            }
            else
            {
                object? theValue = property.GetValue(this);
                object? defaultValue = GetDefault(property.PropertyType);

                if (theValue == null) return null;
                if (theValue.Equals(defaultValue)) return null;
                return theValue;
            }
        }

        private static object? GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
