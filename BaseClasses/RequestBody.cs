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
    /// Base class for web API request bodies. Supports validation using the <see cref="RequiredAttribute"/>.
    /// </summary>
    public abstract class RequestBody
    {
        /// <summary>
        /// Gets whether the request body is valid. Must be overridden in derived classes.
        /// </summary>
        [JsonIgnore]
        public abstract bool Valid { get; }

        /// <summary>
        /// Generates a message listing the names of any missing or invalid required properties.
        /// </summary>
        /// <param name="allowEmptyStrings">If false, empty strings are considered missing.</param>
        /// <returns>A string describing which fields are missing.</returns>
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
        /// Gets a list of property names that are missing or invalid based on required attributes.
        /// </summary>
        /// <param name="allowEmptyStrings">If false, empty strings are considered missing.</param>
        /// <returns>A list of missing or invalid property names.</returns>
        public List<string> GetMissingProperties(bool allowEmptyStrings = false)
        {
            List<string> properties = new List<string>();
            PropertyInfo[] propertyInfos = GetType().GetProperties();

            PropertyInfo[] requiredProperties = GetType().GetProperties()
                .Where(x => x.IsDefined(typeof(RequiredAttribute), false)).ToArray();

            if (requiredProperties.Length > 0)
            {
                foreach (PropertyInfo property in requiredProperties)
                {
                    object? value = GetValue(allowEmptyStrings, property);

                    if (value == null)
                    {
                        properties.Add(property.Name);
                    }
                    else
                    {
                        RequiredAttribute requiredAttribute = property.GetCustomAttribute<RequiredAttribute>()!;
                        if (requiredAttribute.DisallowedValue != null && value.Equals(requiredAttribute.DisallowedValue))
                            properties.Add(property.Name);

                        if (requiredAttribute.GreaterThan != null && property.PropertyType == typeof(int) &&
                            !((int)value > requiredAttribute.GreaterThan))
                            properties.Add(property.Name);
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
        /// Validates the request body using the <see cref="RequiredAttribute"/> on properties.
        /// </summary>
        /// <param name="allowEmptyStrings">If false, empty strings are considered invalid.</param>
        /// <returns>True if all required properties are valid; otherwise false.</returns>
        public bool ValidateByRequiredAttributes(bool allowEmptyStrings = false)
        {
            PropertyInfo[] requiredProperties = GetType().GetProperties()
                .Where(x => x.IsDefined(typeof(RequiredAttribute), false)).ToArray();

            foreach (PropertyInfo property in requiredProperties)
            {
                object? value = GetValue(allowEmptyStrings, property);

                if (value == null)
                {
                    return false;
                }
                else
                {
                    RequiredAttribute requiredAttribute = property.GetCustomAttribute<RequiredAttribute>()!;
                    if (requiredAttribute.DisallowedValue != null && value.Equals(requiredAttribute.DisallowedValue))
                        return false;

                    if (requiredAttribute.GreaterThan != null && property.PropertyType == typeof(int) &&
                        !((int)value > requiredAttribute.GreaterThan))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the value of a property, optionally treating empty strings or default values as null.
        /// </summary>
        /// <param name="allowEmptyStrings">If false, empty strings are treated as null.</param>
        /// <param name="property">The property to get the value from.</param>
        /// <returns>The value, or null if considered missing.</returns>
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
                if (theValue == null) return null;

                if (!property.PropertyType.IsValueType)
                {
                    object? defaultValue = GetDefault(property.PropertyType);
                    if (theValue.Equals(defaultValue)) return null;
                }

                return theValue;
            }
        }

        /// <summary>
        /// Gets the default value for a given type.
        /// </summary>
        /// <param name="type">The type to get the default value for.</param>
        /// <returns>The default value.</returns>
        private static object? GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
