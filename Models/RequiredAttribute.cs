using System;

namespace Sakur.WebApiUtilities.Models
{
    /// <summary>
    /// Attribute used to mark properties as required in a request body.
    /// Supports disallowed values and integer range checks.
    /// </summary>
    public class RequiredAttribute : Attribute
    {
        /// <summary>
        /// A value that is not allowed. If the property has this value, it is considered invalid.
        /// </summary>
        public object? DisallowedValue { get; set; }

        /// <summary>
        /// A minimum allowed value for integer properties. The property must be greater than this value.
        /// </summary>
        public int? GreaterThan { get; set; }

        /// <summary>
        /// Creates a new <see cref="RequiredAttribute"/> with no additional constraints.
        /// </summary>
        public RequiredAttribute() { }

        /// <summary>
        /// Creates a new <see cref="RequiredAttribute"/> with a disallowed value.
        /// </summary>
        /// <param name="disallowedValue">A value that the property must not have.</param>
        public RequiredAttribute(object disallowedValue)
        {
            DisallowedValue = disallowedValue;
        }

        /// <summary>
        /// Creates a new <see cref="RequiredAttribute"/> that enforces the value to be greater than a specified integer.
        /// </summary>
        /// <param name="greaterThan">The value must be greater than this if the property is an integer.</param>
        public RequiredAttribute(int greaterThan)
        {
            GreaterThan = greaterThan;
        }
    }
}
