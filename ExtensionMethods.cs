using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace WebApiUtilities
{
    /// <summary>
    /// Extension methods built into the web api utilities
    /// </summary>
    public static class ExtensionMethods
    {
        private static Random random = new Random();

        /// <summary>
        /// Will make the first character of a string lowercase
        /// </summary>
        /// <param name="str">The string to make the first letter of lowercase</param>
        /// <returns></returns>
        public static string FirstCharToLowerCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
                return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

            return str;
        }

        /// <summary>
        /// Will create a signed hash of a message using a secret
        /// </summary>
        /// <param name="message"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string GetSignedHash(this string message, string secret)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (HMACSHA1 hmacsha1 = new HMACSHA1(keyBytes))
            {
                byte[] hashMessage = hmacsha1.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashMessage);
            }
        }

        /// <summary>
        /// Will split a string by upper case letters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string[] SplitByUpperCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new string[0];
            }

            StringBuilder currentPart = new StringBuilder();
            List<string> parts = new List<string>();

            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    if (currentPart.Length > 0)
                    {
                        parts.Add(currentPart.ToString());
                        currentPart.Clear();
                    }
                }

                currentPart.Append(c);
            }

            if (currentPart.Length > 0)
            {
                parts.Add(currentPart.ToString());
            }

            return parts.ToArray();
        }

        /// <summary>
        /// Will join an array of strings with a character
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="joinCharacter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string JoinWith(this string[] inputArray, string joinCharacter)
        {
            if (inputArray == null)
            {
                throw new ArgumentNullException(nameof(inputArray));
            }

            if (joinCharacter == null)
            {
                throw new ArgumentNullException(nameof(joinCharacter));
            }

            return string.Join(joinCharacter, inputArray);
        }

        /// <summary>
        /// Will create a hash from the content of a string
        /// </summary>
        /// <param name="text">The text input string</param>
        /// <returns>A hash as a string</returns>
        public static string GetContentHash(this string text)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Will replace any parameters in a given text with the values of the parameters provided. Parameters are expected to be written as {{parameterName}}
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ApplyParameters(this string text, params object[]? parameters)
        {
            if (parameters == null || !text.Contains('{'))
                return text;

            string result = text;

            foreach (object parameter in parameters)
            {
                foreach (PropertyInfo propertyInfo in parameter.GetType().GetProperties())
                {
                    string propertyName = propertyInfo.Name;
                    object? propertyValue = propertyInfo.GetValue(parameter);

                    if (propertyValue == null)
                        continue;

                    result = result.Replace($"{{{{{propertyName}}}}}", propertyValue.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Will transfer any properties from the source object to the target object that have the same name and type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T TransferPropertiesTo<T>(this object source, T target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            PropertyInfo[] sourceProperties = source.GetType().GetProperties();
            PropertyInfo[] targetProperties = target.GetType().GetProperties();

            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                PropertyInfo? targetProperty = targetProperties.FirstOrDefault(p => p.Name == sourceProperty.Name && p.PropertyType == sourceProperty.PropertyType);

                if (targetProperty != null && targetProperty.CanWrite)
                {
                    object? value = sourceProperty.GetValue(source);
                    targetProperty.SetValue(target, value);
                }
            }

            return target;
        }

        /// <summary>
        /// Will take a random element from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the list is empty</exception>
        public static T TakeRandom<T>(this List<T> list)
        {
            if (list.Count == 0)
                throw new ArgumentException("List is empty", nameof(list));

            return list[random.Next(list.Count)];
        }

        /// <summary>
        /// Will take a random element from an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If the array is empty</exception>
        public static T TakeRandom<T>(this T[] values)
        {
            if (values.Length == 0)
                throw new ArgumentException("Array is empty", nameof(values));

            return values[random.Next(values.Length)];
        }

        /// <summary>
        /// Will convert a list of integers to a string that can be used in a SQL query.
        /// </summary>
        /// <param name="ids">A list of integers like [1, 2, 3]</param>
        /// <returns>A string like "(1, 2, 3)" that can be used in sql like "WHERE id IN {list}" where {list} is "(1, 2, 3)".</returns>
        public static string ToSqlIdParameterList(this List<int> ids)
        {
            if (ids.Count == 0)
                return "(0)";

            return $"({string.Join(",", ids)})";
        }
    }
}
