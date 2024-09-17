using Microsoft.AspNetCore.Authorization;
using Sakur.WebApiUtilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakur.WebApiUtilities.Helpers
{
    /// <summary>
    /// Will handle scope requirements
    /// </summary>
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        /// <summary>
        /// Handles scope requirements
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type == "permissions" && c.Issuer == requirement.Issuer))
                return Task.CompletedTask;

            string[] permissions = context.User.FindFirst(c => c.Type == "permissions" && c.Issuer == requirement.Issuer)!.Value.Split(' ');

            // Succeed if the scope array contains the required scope
            if (permissions.Any(s => s == requirement.Scope))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
