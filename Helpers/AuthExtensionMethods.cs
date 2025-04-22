using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sakur.WebApiUtilities.Helpers;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace WebApiUtilities.Helpers
{
    /// <summary>
    /// Extension methods for setting up authentication.
    /// </summary>
    public static class AuthExtensionMethods
    {
        /// <summary>
        /// Will setup the authentication for the service collection.
        /// </summary>
        /// <param name="services">The service collection to use</param>
        /// <param name="authDomain">The domain for the auth</param>
        /// <param name="authAudience">The audience for the auth</param>
        /// <param name="permissions">The permissions to have in the auth</param>
        /// <param name="jwtSecretKey">The secret key used for signing and validating JWT tokens</param>
        /// <param name="authenticationScheme">The scheme to use, default is "Bearer"</param>
        /// <returns>The service collection again so that calls can be chained</returns>
        public static IServiceCollection AddLocalJwtAuthentication(
            this IServiceCollection services,
            string authDomain,
            string authAudience,
            List<string> permissions,
            string jwtSecretKey,
            string authenticationScheme = "Bearer")
        {
            byte[] signingKey = Encoding.UTF8.GetBytes(jwtSecretKey);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = authenticationScheme;
                    options.DefaultChallengeScheme = authenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // Configure token validation parameters
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = authDomain,
                        ValidAudience = authAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(signingKey), // Use the provided secret key
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            services.AddAuthorization(options =>
            {
                foreach (string permission in permissions)
                    options.AddPolicy(permission, policy => policy.Requirements.Add(new HasScopeRequirement(permission, authDomain)));
            });

            return services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        }

        /// <summary>
        /// Will setup the authentication for the service collection.
        /// </summary>
        /// <param name="services">The service collection to use</param>
        /// <param name="authDomain">The domain for the auth</param>
        /// <param name="authAudience">The audience for the auth</param>
        /// <param name="permissions">The permissions to have in the auth</param>
        /// <param name="authenticationScheme">The scheme to use, default is "Bearer"</param>
        /// <returns>The service collection again so that calls can be chained</returns>
        public static IServiceCollection AddExternalJwtAuthentication(
            this IServiceCollection services,
            string authDomain,
            string authAudience,
            List<string> permissions,
            string authenticationScheme = "Bearer")
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = authenticationScheme;
                    options.DefaultChallengeScheme = authenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = authDomain;
                    options.Audience = authAudience;

                    // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`. Map it to a different claim by setting the NameClaimType below.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            services.AddAuthorization(options =>
            {
                foreach (string permission in permissions)
                    options.AddPolicy(permission, policy => policy.Requirements.Add(new HasScopeRequirement(permission, authDomain)));
            });

            return services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
        }
    }
}
