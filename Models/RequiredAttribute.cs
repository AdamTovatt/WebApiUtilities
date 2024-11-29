namespace Sakur.WebApiUtilities.Models
{
    /// <summary>
    /// Used to specify that a property is required in a request body not necessary if the validation of the body is done manually and the message for missing properties is
    /// set manually. But this is good to use if you want to be able to automatically validate the body and get a message for which properties are missing.
    /// </summary>
    public class RequiredAttribute : System.Attribute
    {
        public object? DisallowedValue { get; set; }

        /// <summary>
        /// Creates a required attribute with no additional info.
        /// </summary>
        public RequiredAttribute() { }

        /// <summary>
        /// Creates a required attribute with a disallowed value.
        /// </summary>
        /// <param name="disallowedValue">The value that the property that this attribute is set on is not allowed to have.</param>
        public RequiredAttribute(object disallowedValue)
        {
            DisallowedValue = disallowedValue;
        }
    }
}
