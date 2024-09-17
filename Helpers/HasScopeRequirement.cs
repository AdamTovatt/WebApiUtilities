using Microsoft.AspNetCore.Authorization;
using System;

namespace Sakur.WebApiUtilities.Helpers
{
    /// <summary>
    /// Used to check if the user has the required scope
    /// </summary>
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The issuer of the token
        /// </summary>
        public string Issuer { get; }

        /// <summary>
        /// The scope that the user must have
        /// </summary>
        public string Scope { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="issuer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HasScopeRequirement(string scope, string issuer)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        }
    }
}
