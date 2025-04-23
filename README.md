# WebApiUtilities
A lightweight NuGet package designed to enhance web API development in ASP.NET Core. It provides various utilities and features to streamline common tasks and improve the efficiency of API development.

**NuGet link:** [SakurWebApiUtilities](https://www.nuget.org/packages/SakurWebApiUtilities/)

## Key Features
- **Rate Limiting**: Implement request rate limiting using `ClientStatistics` to track and enforce limits on client requests.
- **Authentication Setup**: Simplifies configuration of authentication schemes with customizable parameters for domains, audiences, and roles. Both with external auth validation like Auth0 and with local like a local "magic link" solution. There is also support for very simple api keys for application to application usage.
- **Scheduled Tasks**: Allows for the creation and management of recurring tasks that can be scheduled at specific intervals or times.
- **Request Body Validation**: Automatically validates incoming request bodies with detailed error reporting for missing fields.
- **Error Handling**: Provides `ApiException` and `ApiResponse` classes for structured error management and consistent API responses. The middleware for ApiExceptions can be added to handle any exceptions that are thrown to ensure responses are always in the same format.
- **Resource Mapping**: Provides a ```ResourceHelper``` that together with the ```Resource``` type allows for statically typed non-code resources that can be verified to exist in unit tests and/or at application startup to avoid runtime errors or excessive file existance checking and missing file handling together with hardcoded paths for files that just hopefully exist all the time.
- **Connection String Utilities**: Simplifies the creation of connection strings for database connections with custom SSL configurations.
- **Password Management**: Offers methods for hashing, validating, and generating random passwords.
- **Extension Methods**: Includes various helpful extension methods for common tasks, such as string manipulation and object property transfer.

## Rate Limiting
This package includes support for request rate limiting, using `ClientStatistics` to track request history for each client.

### How it Works
Rate limiting is enforced by tracking the timestamps of each client’s requests within a time window, capped by `MaxRequests`.

#### Client Statistics Tracking
The `ClientStatistics` class stores request timestamps in a queue-like structure that tracks only the last `MaxRequests` timestamps. This queue is then checked against the defined `TimeWindow` to determine if a client has exceeded the allowed rate.

#### Caching with IDistributedCache
The middleware uses `IDistributedCache` to store `ClientStatistics` across distributed instances, allowing rate limits to be enforced consistently across multiple servers. You will need to configure a distributed cache in `Program.cs`:

### Setup:
In `Program.cs` or wherever your Web API is built, add:

```csharp
builder.Services.AddDistributedMemoryCache(); // to add the distributed memory cache
app.UseRateLimiting(); // to add the rate limiting middleware
```

Now, use the [Limit] attribute to specify limits for controller methods:
```
[Limit(MaxRequests = 20, TimeWindow = 10)]
```
- **MaxRequests**: Maximum number of requests allowed within the `TimeWindow` period.
- **TimeWindow**: The time window (in seconds) during which `MaxRequests` is allowed.

## Auth
This package simplifies authentication configuration in your Web API.

### Setup:
In `Program.cs`, configure the authentication with the following:

```csharp
builder.Services.SetupAuth(authDomain, authAudience, roles, authenticationScheme);
```
Will setup the authentication for the service collection in cases where the token is externally validated like Auth0.

- **Parameters:**
  - `authDomain`: The authentication domain (issuer).
  - `authAudience`: The audience identifier for the token.
  - `roles`: Required roles for accessing the API.
  - `authenticationScheme`: The authentication scheme to use (default is `"Bearer"`).

Will return the service collection for easy chaining.

Example usage:
```csharp
builder.Services.AddExternalJwtAuthentication(
    authDomain: "https://my-auth-domain.com",
    authAudience: "my-api-audience",
    roles: new[] { "Admin", "User" },
    authenticationScheme: "Bearer" // not really needed since this is the default anyway
);
```

### Magic Link Authentication

The package includes support for **Magic Link Authentication**, allowing users to log in without passwords by clicking a secure link sent to their email.

### How It Works

1. **Generate a Magic Link Token**: A secure, time-limited token is generated for a user.
2. **Send the Link**: The token is included in a link sent via email.
3. **Validate the Token**: The server validates the token when the user clicks the link.
4. **Issue a JWT**: On successful validation, a JWT is issued to authenticate subsequent requests.

### Base Class: `AuthHelperBase`

The `AuthHelperBase` provides the core logic for generating and validating magic link tokens.

#### Example Usage

1. **Create a Derived Class**: Implement a derived class to provide configuration details.

```csharp
public sealed class AuthHelper : AuthHelperBase<AuthHelper>
{
    protected override void Initialize()
    {
        Configure(
            magicLinkSecretKey: "YourMagicLinkSecretKey",
            jwtSecretKey: "YourJwtSecretKey",
            issuer: "your-issuer",
            audience: "your-audience",
            magicLinkExpirationMinutes: 15,
            jwtExpirationMinutes: 60
        );
    }
}
```

2. **Generate a Magic Link Token**:

```csharp
string userId = "user123";
string token = AuthHelper.Instance.GenerateMagicToken(userId);
string magicLink = $"https://yourdomain.com/auth/magic?token={token}";

// Send the magic link via email
```

3. **Validate the Magic Link Token**:

```csharp
string token = "received-token-from-link";
if (AuthHelper.Instance.ValidateMagicToken(token, out string? userId))
{
    // Token is valid; issue a JWT
    string jwt = AuthHelper.Instance.GenerateJwtToken(userId, new[] { "user" });
    // Return the JWT to the user
}
else
{
    // Token is invalid or expired
}
```

4. **Don't forget to add the validation in Program.cs (or where your WebApplicationBuilder is)**
```csharp
builder.Services.AddLocalJwtAuthentication(
    authDomain: EnvironmentHelper.GetEnvironmentVariable("JWT_ISSUER"),
    authAudience: EnvironmentHelper.GetEnvironmentVariable("JWT_AUDIENCE"),
    permissions: new List<string>() { "admin" },
    jwtSecretKey: EnvironmentHelper.GetEnvironmentVariable("JWT_SECRET", 24) // minimum length of 24 characters
);
```
In the example above the authDomain, authAudience and jwtSecretKey are taken from the environment variables. You can provide them however you like and this is just one example of how to it could be done.

### Notes

- **Token Expiration**: The magic link token has a configurable expiration time (default is 15 minutes).
- **Security**: Ensure the magic link is sent securely and only to the intended recipient.

## Middleware

The `ApiExceptionMiddleware` handles any unhandled exceptions during request processing and ensures a consistent response format by returning an instance of `ApiResponse`.

Add it to the middleware pipeline in `Program.cs`:

```csharp
app.UseMiddleware<ApiExceptionMiddleware>();
```

### API Key Authorization

Instead of using middleware, API key validation is now done using the `[RequireApiKey]` attribute together with the `ApiKeyFilter`. This allows you to selectively apply API key protection to specific controllers or actions.

Register the filter when configuring controllers:

```csharp
services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyFilter>();
});
```

Then apply the attribute like this:

```csharp
[RequireApiKey]
[HttpGet("secure-endpoint")]
public IActionResult SecureEndpoint()
{
    return Ok("You provided a valid API key.");
}
```

## Scheduled Tasks
WebApiUtilities provides utilities for setting up scheduled tasks that can run at specific times or intervals. This is useful for automating recurring operations, such as data cleanup, sending periodic notifications, or syncing external data.

### Example Use Cases:
- **Data Synchronization**: Run a daily task to sync data from an external source (e.g., fetching the latest inventory data every night at 2 AM).
- **Log Cleanup**: Schedule an hourly task to remove outdated logs or temporary files.
- **Automated Notifications**: Trigger a weekly summary email or alert to users.

With WebApiUtilities, you can set tasks to run at fixed intervals or specific times of day, allowing for a wide range of scheduling needs.


### Task Types:
1. **IntervalTask**: Runs repeatedly at a fixed interval.
    - **Interval**: How frequently the task should run.
    - **InitialStartDelay**: Sets a delay before the task’s first execution. This can help stagger multiple tasks that run at the same interval (e.g., once per minute), avoiding the scenario where many tasks run simultaneously, causing spikes in load. By spreading out task start times, you achieve a more even distribution of workload over time.

  
2. **TimeOfDayTask**: Runs once a day at a specific time.
    - **ScheduledTime**: The time of day to execute the task (e.g., 04:00 for 4 AM).

### Usage:
1. Define tasks by inheriting from `IntervalTask` or `TimeOfDayTask`, and implement the `ExecuteAsync` method.

Here is an example of a `TimeOfDayTask` that updates a list of sanctioned names at 4 AM:
```csharp
public class UpdateSanctionsTask : TimeOfDayTask
{
    public override TimeSpan ScheduledTime => new TimeSpan(4, 0, 0); // 4 AM

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SanctionsManager.Instance.UpdateSanctionedNamesIfNeededAsync();
    }
}
 ```

2. Register tasks in the DI container (probably in your Main function):

```csharp
builder.Services.AddScheduledTasks(
    typeof(BroadcastTask),
    typeof(UpdateSanctionsTask));
```

This setup will handle instantiation and scheduling for registered tasks using the `ScheduledTaskManager`.

## Queued Tasks

WebApiUtilities provides support for queued background tasks, allowing you to enqueue tasks for immediate background execution. This is useful when you need to perform long-running operations without blocking the main request thread but still be sure they finish which you can't with the default tasks in .NET.

### Example Use Cases:
- **Thumbnail Generation**: Offload image processing tasks to the background.
- **Email Notifications**: Send emails asynchronously after user actions.
- **Data Import/Export**: Process large data imports without slowing down the main API.

### Defining a Queued Task

To define a queued task, create a class that inherits from `QueuedTaskBase` and implement the `ExecuteAsync` method:

```csharp
public class ThumbnailGenerationTask : QueuedTaskBase
{
    public string FilePath { get; }

    public ThumbnailGenerationTask(string filePath)
    {
        FilePath = filePath;
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await PhotoshopThumbnailHelper.CreateThumbnail(FilePath);
    }
}
```

### Adding Queued Task Processing

In `Program.cs`, register the background task queue and processor:

```csharp
builder.Services.AddQueuedTaskProcessing();
```

This sets up the `BackgroundTaskQueue` and `QueuedTaskProcessor` needed for background task execution.

### Enqueuing Tasks

You can enqueue tasks using the `BackgroundTaskQueue` singleton:

```csharp
BackgroundTaskQueue.Instance.QueueTask(new ThumbnailGenerationTask("path/to/image.png"));
```
Alternatively, if using Dependency Injection:

```csharp
public class SomeService
{
    private readonly BackgroundTaskQueue _taskQueue;

    public SomeService(BackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    public void GenerateThumbnail(string filePath)
    {
        _taskQueue.QueueTask(new ThumbnailGenerationTask(filePath));
    }
}
```

## Request Body Validation

The `RequestBody` class serves as a base class for defining the request body of your web API requests. It provides a structured way to validate incoming data and ensure that all required fields are present before processing the request.

### Key Features

- **Automatic Validation**: By inheriting from `RequestBody`, you can automatically validate whether the required properties are provided in the request body. This is done using the custom `[Required]` attribute, allowing for easy and consistent validation across your API.

- **Missing Properties Reporting**: The class includes functionality to generate messages detailing which required fields are missing. This is useful for debugging and providing informative error responses to clients.

- **Flexible Validation Options**: You can control whether empty strings are considered valid using the `allowEmptyStrings` parameter in the validation methods. This allows for more flexibility based on your API’s requirements.

### Example Usage

To use the `RequestBody` class, create a derived class that defines your specific request body structure. Here's an example:

```csharp
public class MyRequest : RequestBody
{
    [Required]
    public string Name { get; set; }

    [Required]
    public int Age { get; set; }

    public override bool Valid => ValidateByRequiredAttributes();

    // Additional properties and methods can be added as needed.
}
```

In this example, the `MyRequest` class specifies that both `Name` and `Age` are required fields. The `Valid` property leverages the automatic validation mechanism to check the presence of these fields.

### Validation Methods

- **GetInvalidBodyMessage**: Call this method to obtain a detailed string message indicating which fields are missing from the request body.

- **GetMissingProperties**: This method returns a list of property names that are required but not provided or that are explicitly disallowed.

- **ValidateByRequiredAttributes**: Use this method to perform a quick validation check against the properties marked with the `[Required]` attribute.

The `Valid` property can be overridden to provide custom validation logic if needed, but then the automatically generated error messages might not be accurate since they are based on the `[Required]` attribute.

If the `[Required]` attribute is not used for any property, custom validation logic should be implemented in the `Valid` property. The `GetInvalidBodyMessage` and `GetMissingProperties` methods can still be used to generate error messages and will then assume all properties that do not have the `JsonIgnore` attribute are required.

### Disallowed Values in the `[Required]` Attribute

The `[Required]` attribute includes a `DisallowedValue` property, allowing you to specify a value that should **not** be allowed for a property. If a property has this disallowed value, it will not be considered valid.

#### Example Usage:

```csharp
public class MyRequest : RequestBody
{
    [Required(disallowedValue: 0)] // 0 is allowed unless explicitly disallowed
    public int Age { get; set; }

    [Required(disallowedValue: "N/A")]
    public string Name { get; set; }

    public override bool Valid => ValidateByRequiredAttributes();
}
```

In this example:
- The `Age` property is required and cannot be `0`.
- The `Name` property is required and cannot be `"N/A"`.

#### Default Behavior for Missing or Invalid Values

- For **reference types** and **nullable value types**, a value is considered missing if it is `null`.
- For **value types** (like `int`, `bool`), the value is **valid by default**, including `0`, unless you explicitly disallow it using `DisallowedValue` or require it to be greater than a value using `GreaterThan`.

If you want to require a value type to be something other than its default (e.g. `int` not equal to `0`), you must explicitly use `[Required(disallowedValue: 0)]`.

If a value like `0` or `false` is acceptable, simply avoid using `[Required]` on that property.

## ApiException

The `ApiException` class represents errors that occur within the API. This class is crucial for handling exceptions gracefully and returning meaningful error messages to the client. Key features include:

- **Error Details**:
  - The class contains properties for storing an error message (`ErrorMessage`), the associated HTTP status code (`StatusCode`), and an optional error object (`ErrorObject`) that can contain additional context or data related to the error.

- **Constructors for Flexibility**:
  - There are multiple constructors allowing for different ways to initialize an `ApiException`:
    - **Message and Status Code**: The simplest constructor requires only a message and a status code.
    - **Error Object**: Another constructor allows for an error object to be passed, providing further detail about the exception.
    - **Combined Constructor**: A more comprehensive constructor accepts both an error object and a message, enabling detailed error reporting.

- **HTTP Status Code Utility**:
  - One of the most useful aspects of the `ApiException` class is the ability to include an HTTP status code. This allows exceptions to be thrown deep within the application logic and subsequently handled at the controller level. By capturing the correct status code, the API can return appropriate responses to clients based on the context of the error.

- **Readable Output**:
  - The `ToString()` method is overridden to provide a human-readable representation of the exception, combining the error message with the stack trace. This is useful for logging and debugging purposes.


## ApiResponse
The `ApiResponse` class is designed to encapsulate the response returned by the API. It provides constructors to create responses with various types of content, allowing for flexible API design. Below are the key functionalities:

- **Status Code Handling**:
  - The `ApiResponse` constructor accepts an `HttpStatusCode` parameter, enabling the specification of the HTTP status code for the response. The default is set to `HttpStatusCode.OK`, ensuring that responses are valid even without explicit status codes.

- **Response Body Customization**:
  - Multiple constructors allow for the inclusion of different response body formats:
    - **Message-Only Response**: One constructor allows for just a message to be included in the response body.
    - **Object Response**: Another constructor allows for an arbitrary object to be sent as the response body, providing the flexibility to return complex data structures.

- **Exception Handling**:
  - The constructor that accepts an `ApiException` creates a well-structured response based on the exception's data. This includes:
    - An optional error message.
    - An optional error object containing additional data related to the exception.
    - The appropriate HTTP status code derived from the exception.

## Resource Management

The library includes utilities for managing embedded resources in a structured and type-safe way using the `ResourceHelper` and `Resource` struct.

### Overview

Embedded resources can be used to include static files (e.g., `.html`, `.csv`, `.pdf`, etc.) directly in your assembly. This is useful for shipping files with your application without depending on the file system. The `ResourceHelper` provides APIs to read these files, while the `Resource` struct allows for structured mappings. This allows for statically typed resources that you can be sure actually exist at the expected path during runtime. No more hardcoded strings and checking if a file exists and trying to come up with a way to handle the case when it doesn't over and over again in the code.

### Initialization

The `ResourceHelper` is a singleton and must be initialized before use. You should call `ResourceHelper.Initialize()` early in your program, such as at application startup.

```csharp
// Startup.cs or Program.cs
ResourceHelper.Initialize(); // Verifies resource mappings and prepares for use
```

This method ensures that all resources mapped in your `Resource` struct exist as embedded files and vice versa. It's recommended to call this in unit tests too to detect mismatches during testing to avoid runtime errors. Still, if you forget that, provided you set it up to call Initialize() as early as possible in the real code you will just get a runtime error right at startup instead of suddenly in the middle of running the program.

### Creating Custom Resources

Define your embedded resources using the `Resource` struct in a nested class structure like this:

```csharp
public readonly partial struct Resource
{
    public static class Templates
    {
        public static readonly Resource InvoiceHtml = new Resource("Templates/invoice.html");
        public static readonly Resource ReportCsv = new Resource("Templates/report.csv");
    }

    public static class Documents
    {
        public static readonly Resource GuidePdf = new Resource("Documents/guide.pdf");
    }
}
```

The embedded files must reside in a `Resources` folder, and their paths must match the structure defined in the resource mappings. For example, here we would have the code project with a directory Resources that has a subdirectory Templates and another one called Documents. The Resources/Templates contains invoice.html and report.csv and Resources/Documents contains guide.pdf.

**Important:** Ensure these files are marked as `EmbeddedResource` in your `.csproj` file:

```xml
<ItemGroup>
  <EmbeddedResource Include="Resources\**\*" />
</ItemGroup>
```

The above code will automatically set all files in the Resources directory to embedded resource build type. If using this, make sure your own ```partial struct Resource``` is not defined in that directory. Or, if you want to define it in that directory, you can skip adding this to the project file and instead just mark each resource file as an embedded resource.

In Visual Studio, just right click the file, go to properties. Set the ```Build Action``` to ```Embedded Resource```.

### Reading Resources

Once initialized, you can read embedded resources using the helper:

```csharp
string htmlContent = await ResourceHelper.Instance.ReadAsStringAsync(Resource.Templates.InvoiceHtml);

using Stream pdfStream = ResourceHelper.Instance.GetFileStream(Resource.Documents.GuidePdf);
```

You can also get the MIME type of a file:

```csharp
string contentType = ResourceHelper.Instance.GetContentType(Resource.Templates.ReportCsv); // returns "text/csv"
```

### Deserializing Previous Resources
If you have code that gets a resource using the ```ResourceHelper```, then that code serializes or in some way saves the path to that resource for later use you can manually create an instance of ```Resource``` from that path string so that you can use it with ```ResourceHelper```. This is done by calling ```Resource.CreateUnverified(path)```. Please note, that by doing this, you are bypassing the file verification. Manually creating a ```Resource``` means that the ```ResourceHelper``` can not ensure that it exists in the unit tests or at application startup which might lead to unexpected run time errors (like the ones you always have to handle without this system) of the file not existing even though you have a path to it.

This means that it's very important to only use the ```Resource.CreateUnverified(path)``` if you are certain that that resource actually exists.

### Tips

- Call `ResourceHelper.Initialize()` at startup and in tests.
- Define all resources in a nested structure inside the `Resource` struct.
- Ensure all files are marked as embedded in the project file.
- Use `Resource.CreateUnverified(path)` cautiously — only when deserializing trusted input.

This setup ensures safe, verifiable usage of embedded resources with strong typing and clear validation.


# Helper Classes Summary

## ConnectionStringHelper
A static class designed to facilitate the creation of connection strings from a given URL. It includes a method to generate a connection string that accounts for various parameters like SSL mode and trust settings. It uses a nested `ConnectionStringBuilder` class to construct the connection string.

### Methods
- **GetConnectionStringFromUrl**: Creates a connection string from a provided URL, with options for SSL mode and trust settings.

---

## SslMode
An enumeration that defines the types of SSL modes that can be used for database connections:
- **Disable**: SSL is disabled.
- **Prefer**: SSL is preferred, but not required.
- **Require**: SSL is required.

---

## EnvironmentHelper
A static class that provides functionality to retrieve environment variables. It includes caching to improve performance and throws exceptions if the specified variable is not found or does not meet minimum length requirements.

### Methods
- **GetEnvironmentVariable**: Retrieves an environment variable, validating its existence and length, with an option to bypass the cache.

---

## NpgsqlExtensionMethods
A static class that provides extension methods for working with Npgsql database connections. It allows for the retrieval of objects and collections from the database using asynchronous methods.

### Methods
- **GetAsync**: Retrieves a list of objects from the database based on a query and parameters.
- **GetSingleOrDefaultAsync**: Retrieves a single object or the default value if not found.

### Detailed Explanation of GetAsync
The `GetAsync<T>` method in the `NpgsqlExtensionMethods` class is designed for retrieving complex objects from a PostgreSQL database asynchronously. It allows for mapping complex relationships between entities through manual parameter lookups, enabling developers to fetch and construct related objects efficiently.

### Example Usage Breakdown

It is a bit complicated to understand the use case and why it is needed but here’s a detailed breakdown of the an example, focusing on how `GetAsync` is utilized to fetch `Transaction` objects along with their related `Account`:

First, imagine a `Transaction` class and its properties, here is a simplified version:

```csharp
public class Transaction
{
    public int Id { get; set; }
    public decimal Value { get; set; }
    
    // AffectedAccount is a reference to another table's record (Account)
    public Account? AffectedAccount { get; set; } 
}
```

The `AffectedAccount` property is of type `Account?`, indicating that it references another entity that can be null if no related account exists. This relationship is key for understanding how `GetAsync` works with manual parameter lookups to populate this property.

The database table for `Transaction` might look like this:

```sql
CREATE TABLE transaction (
    id SERIAL PRIMARY KEY,
    value NUMERIC NOT NULL,
    affected_account INT REFERENCES account(id)
);
```

This is a common scenario in database design where entities have relationships with other entities, and you need to fetch related objects to construct a complete view of your data. Sometimes, these relationships are complex and require multiple queries or joins to retrieve all the necessary information. Sometimes the "sub-entities" can be stored in a cache in memory to avoid these joins or repeated database calls, it is useful to be able to combine the data from the cache with the data from the database.

Now, here is an example of how `GetAsync` is used to to fetch transactions and populate the `AffectedAccount` property with the corresponding `Account` object:

```csharp
public async Task<List<Transaction>> GetAll()
{
    const string query = "SELECT * FROM transaction";

    using (NpgsqlConnection connection = await GetConnectionAsync())
        return await connection.GetAsync<Transaction>(query, null, new Dictionary<string, Func<object?, Task<object?>>>()
        {
            {
                nameof(Transaction.AffectedAccount), async (x) =>
                {
                    if(x == null) return null;
                    return await AccountRepository.Instance.GetByIdAsync((int)x);
                }
            }
        });
}
```

### Key Components Explained

- The SQL query retrieves all records from the `transaction` table where.
- The `GetAsync<Transaction>` method is called to execute the SQL query. The third parameter is a dictionary that defines manual parameter lookups.
- The dictionary passed to `GetAsync` specifies how to handle the `AffectedAccount` property of `Transaction`.
    - The key is `nameof(Transaction.AffectedAccount)`, which tells the method to look up this property.
    - The value is a function that takes an object (expected to be the ID of the `AffectedAccount` because that's what's stored in the database) and returns the corresponding `Account` asynchronously:
     ```csharp
     async (x) =>
     {
         if (x == null) return null;
         return await AccountRepository.Instance.GetByIdAsync((int)x);
     }
     ```
   - If `x` is not null, it calls `GetByIdAsync` from the `AccountRepository` to fetch the `Account` object by its ID.

<details>
<summary>AccountRepository.GetByIdAsync() implementation</summary>
It doesn't really matter for the explanation because how it is implemented doesn't really concern the method that fetches the transactions but here is an example of how `GetByIdAsync` might be implemented in the `AccountRepository`:

```csharp
public async Task<Account?> GetByIdAsync(int id)
{
    if (accountsCache == null)
        await GetAllAsync();

    if (!accountsCache.ContainsKey(id))
        return null;

    return accountsCache[id];
}
```
</details>

### How It Works Together

- The `GetAsync` method executes the SQL query and starts mapping the results to `Transaction` objects. 
- For each transaction, if the `AffectedAccount` field in the database has a value, the provided function is invoked, allowing it to retrieve the full `Account` object asynchronously.
- This pattern allows you to maintain the relationships between your entities effectively without needing to manually join tables in your SQL queries or handling nested SQL commands.

### Benefits of This Approach

1. **Complex Object Relationships**: It simplifies the retrieval of complex entities and their relationships by allowing manual lookups for properties that reference other tables.
  
2. **Asynchronous Operation**: The use of asynchronous methods (`await`) ensures that database operations do not block the execution thread, enhancing application performance.

3. **Separation of Concerns**: The logic for retrieving related accounts is encapsulated in the `AccountRepository`, adhering to principles of clean architecture and separation of concerns.

4. **Dynamic Fetching**: This method provides a dynamic way to fetch related objects based on the specific requirements defined in the SQL query, making it flexible for various use cases.

5. **Caching Opportunities**: The `GetByIdAsync` method includes caching logic, which can improve performance by reducing repeated database calls for the same account data.

#### Conclusion

The use of `GetAsync` with manual parameter lookup in the context of the `Transaction` and `Account` demonstrates a powerful approach for handling complex data retrieval in a clean and efficient manner. It abstracts the intricacies of joining and fetching related data while allowing for asynchronous execution and better resource management.


## PasswordHelper
A static class containing utilities for password management, including hashing and validation.

### Methods
- **ValidPassword**: Validates a plaintext password against a stored hash.
- **CreatePasswordHash**: Creates a hash and salt from a plaintext password.
- **CreateRandomPassword**: Generates a random password of a specified length.

## Miscellaneous Extension Methods

1. **FirstCharToLowerCase**
   - **Description:** Converts the first character of a string to lowercase.
   - **Usage:** `string result = myString.FirstCharToLowerCase();`

2. **GetSignedHash**
   - **Description:** Creates a signed hash of a message using a specified secret.
   - **Usage:** `string hash = myMessage.GetSignedHash(mySecret);`

3. **SplitByUpperCase**
   - **Description:** Splits a string into an array of substrings based on uppercase letters.
   - **Usage:** `string[] parts = myString.SplitByUpperCase();`

4. **JoinWith**
   - **Description:** Joins an array of strings with a specified character.
   - **Usage:** `string result = myArray.JoinWith(", ");`

5. **GetContentHash**
   - **Description:** Creates an MD5 hash from the content of a string.
   - **Usage:** `string hash = myString.GetContentHash();`

6. **ApplyParameters**
   - **Description:** 
     The `ApplyParameters` method is designed to replace placeholders in a given text with corresponding property values from provided parameter objects. The placeholders are formatted as `{{parameterName}}`, where `parameterName` corresponds to the property names of the objects passed as parameters.
   - **How It Works:** 
     - The method first checks if the `parameters` array is null or if the `text` does not contain any placeholders. If so, it returns the original text.
     - It iterates over each parameter object and retrieves its properties using reflection.
     - For each property, it checks if its value is non-null and then replaces any occurrences of the corresponding placeholder in the text with the property's string value.
     - This method is useful for dynamically generating strings, such as templates, where specific values need to be injected based on object properties.
   - **Usage Example:**
     ```csharp
     var user = new { Name = "Alice", Age = 30 };
     string messageTemplate = "Hello, {{Name}}! You are {{Age}} years old.";
     string result = messageTemplate.ApplyParameters(user);
     // result: "Hello, Alice! You are 30 years old."
     ```

7. **TransferPropertiesTo**
   - **Description:** 
     The `TransferPropertiesTo` method facilitates the copying of property values from one object (source) to another (target) based on matching property names and types. This is particularly useful when you have two objects that share some properties, and you want to update the target object with values from the source object.
   - **How It Works:**
     - The method checks if the target object is null and throws an `ArgumentNullException` if it is.
     - It retrieves the properties of both the source and target objects using reflection.
     - It iterates through each property of the source object, searching for a property in the target object that has the same name and type.
     - If a matching property is found, it checks if the target property can be written to (i.e., it has a setter). If so, it copies the value from the source property to the target property.
     - This method helps ensure that only compatible properties are copied, preventing type mismatches and enhancing code safety.
   - **Usage Example:**
     ```csharp
     public class Source
     {
         public string Name { get; set; }
         public int Age { get; set; }
     }

     public class Target
     {
         public string Name { get; set; }
         public int Age { get; set; }
         public string Address { get; set; }
     }

     var source = new Source { Name = "Bob", Age = 25 };
     var target = new Target();
     target = source.TransferPropertiesTo(target);
     // target now has Name = "Bob" and Age = 25. Address remains null.
     ```

8. **TakeRandom (List<T>)**
   - **Description:** Selects a random element from a list.
   - **Usage:** `var randomElement = myList.TakeRandom();`

9. **TakeRandom (T[])**
   - **Description:** Selects a random element from an array.
   - **Usage:** `var randomElement = myArray.TakeRandom();`

10. **ToSqlIdParameterList**
    - **Description:** Converts a list of integers into a string format suitable for SQL queries, e.g., `(1, 2, 3)`.
    - **Usage:** `string sqlList = myIds.ToSqlIdParameterList();`
