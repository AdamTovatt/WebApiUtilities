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
Will setup the authentication for the service collection

```
Parameters:
services = The service collection to use
authDomain = The domain for the auth
authAudience = The audience for the auth
roles = The roles to have in the auth
authenticationScheme = The scheme to use, default is "Bearer"

Returns: The service collection again so that calls can be chained
```
