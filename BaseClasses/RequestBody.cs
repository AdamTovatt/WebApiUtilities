using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiUtilities;
using WebApiUtilities.Models;

namespace Sakur.WebApiUtilities.BaseClasses
{
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
        public string GetInvalidBodyMessage()
        {
            StringBuilder stringBuilder = new StringBuilder("Missing fields in request body: ");

            foreach (string property in GetMissingProperties())
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
        public List<string> GetMissingProperties()
        {
            List<string> properties = new List<string>();
            PropertyInfo[] propertyInfos = GetType().GetProperties();

            foreach (PropertyInfo property in propertyInfos)
            {
                bool hasJsonIgnoreProperty = property.IsDefined(typeof(JsonIgnoreAttribute), false);

                if (!hasJsonIgnoreProperty && property.GetValue(this) == GetDefault(property.GetType()))
                    properties.Add(property.Name);
            }

            return properties;
        }

        /// <summary>
        /// Used to automatically validate the request body based on the Required attribute.
        /// </summary>
        /// <returns>Wether or not all the required properties were provided</returns>
        public bool ValidateByRequiredAttributes()
        {
            PropertyInfo[] requiredProperties = GetType().GetProperties().Where(x => x.IsDefined(typeof(RequiredAttribute), false)).ToArray();

            foreach (PropertyInfo property in requiredProperties)
            {
                if (property.GetValue(this) == GetDefault(property.GetType()))
                    return false;
            }

            return true;
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}
