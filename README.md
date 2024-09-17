# WebApiUtilities
Tiny nuget package to help with web api development in asp.net core.
Nuget link: https://www.nuget.org/packages/SakurWebApiUtilities/

# Rate limiting
Requires a distributed memory cache:

(in Program.cs or where your webapi is built)

```
builder.Services.AddDistributedMemoryCache();
```

Then add rate limiting:
```
app.UseRateLimiting();
```

Above controller methods you can now add:
```
[Limit(MaxRequests = 20, TimeWindow = 10)]
```
MaxRequests: The amount of requests allowed during the time specified in seconds in TimeWindow
TimeWindow: The time window in seconds during which MaxRequests amount of requests are allowed

# Auth
(in Program.cs or where your webapi is built)
```
builder.Services.SetupAuth();
```
/// <summary>
/// Will setup the authentication for the service collection
/// </summary>
/// <param name="services">The service collection to use</param>
/// <param name="authDomain">The domain for the auth</param>
/// <param name="authAudience">The audience for the auth</param>
/// <param name="roles">The roles to have in the auth</param>
/// <param name="authenticationScheme">The scheme to use, default is "Bearer"</param>
/// <returns>The service collection again so that calls can be chained</returns>