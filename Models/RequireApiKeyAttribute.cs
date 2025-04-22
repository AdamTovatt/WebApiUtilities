using System;

namespace WebApiUtilities.Models
{
    /// <summary>
    /// Indicates that an action or controller requires API key validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireApiKeyAttribute : Attribute
    {
    }
}
