namespace Sakur.WebApiUtilities.Models
{
    /// <summary>
    /// Used to specify that a property is required in a request body not necessary if the validation of the body is done manually and the message for missing properties is
    /// set manually. But this is good to use if you want to be able to automatically validate the body and get a message for which properties are missing.
    /// </summary>
    public class RequiredAttribute : System.Attribute { }
}
