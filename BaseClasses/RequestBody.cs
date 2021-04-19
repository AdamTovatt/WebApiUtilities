using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

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
                stringBuilder.Append(property);
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

            foreach (PropertyInfo property in GetType().GetProperties().Where(prop => prop.IsDefined(typeof(JsonPropertyAttribute), false)).ToList())
            {
                if (property.GetValue(this) != null)
                    properties.Add(property.Name);
            }

            return properties;
        }
    }
}
