# Word Inverser API

A REST API built with .NET 9 that inverses words in sentences while preserving special characters at the beginning and end of words.

## Features

- **Word Inversion**: Inverses words in a sentence while maintaining special characters at word boundaries
- **Memory Caching**: Uses in-memory cache for performance optimization with database persistence
- **Request/Response Logging**: Automatically logs all API requests and responses to database
- **Pagination Support**: Provides paginated endpoints for searching and listing request/response pairs
- **API Versioning**: Built-in API versioning support for future enhancements
- **Swagger UI**: Interactive API documentation and testing interface
- **Global Exception Handling**: Centralized error handling with detailed logging
- **Batch Loading**: Initializes memory cache from database in batches during startup

## Architecture

The project follows a clean architecture pattern with the following layers:

- **WordInverser.API**: Web API layer with controllers, middleware, and hosted services
- **WordInverser.Business**: Business logic layer with services and word inversion algorithms
- **WordInverser.DAL**: Data Access Layer with EF Core, repositories, and Unit of Work pattern
- **WordInverser.Common**: Shared models, interfaces, and exceptions
- **WordInverser.Database**: SQL Database project with tables, stored procedures, and deployment scripts

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2019 or higher](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Microsoft.Build.Sql SDK](https://www.nuget.org/packages/Microsoft.Build.Sql) (for building DACPAC)

## Database Setup

### 1. Build the DACPAC

Navigate to the Database project directory and build the DACPAC:

```powershell
cd WordInverser.Database
dotnet build
```

This will create a DACPAC file at:
```
WordInverser.Database\bin\Debug\WordInverser.Database.dacpac
```

### 2. Deploy the Database

#### Option A: Using SqlPackage.exe

First, install SqlPackage if you haven't already:

```powershell
dotnet tool install -g microsoft.sqlpackage
```

Then deploy the DACPAC:

```powershell
sqlpackage /Action:Publish `
  /SourceFile:"WordInverser.Database\bin\Debug\WordInverser.Database.dacpac" `
  /TargetServerName:"localhost" `
  /TargetDatabaseName:"WordInverserDB" `
  /TargetTrustServerCertificate:True
```

#### Option B: Using SQL Server Management Studio (SSMS)

1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Right-click on "Databases" and select "Deploy Data-tier Application"
4. Follow the wizard and browse to the DACPAC file
5. Configure the database name as `WordInverserDB`

### 3. Verify Database Creation

Connect to SQL Server and verify the database and tables:

```sql
USE WordInverserDB;
GO

SELECT * FROM INFORMATION_SCHEMA.TABLES;
GO
```

You should see two tables:
- `WordCache`
- `RequestResponse`

## Application Configuration

### Update Connection String

Update the connection string in `WordInverser.API\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=WordInverserDB;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

**Note:** If using a SQL Server named instance, include the instance name in the server:
```json
"Server=localhost\\INSTANCENAME;Database=WordInverserDB;..."
```

For SQL Server authentication, use:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=WordInverserDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  }
}
```

## Running the Application

### 1. Restore Dependencies

From the solution root directory:

```powershell
dotnet restore
```

### 2. Build the Solution

```powershell
dotnet build
```

### 3. Run the API

```powershell
cd WordInverser.API
dotnet run
```

The application will start and:
1. Initialize the memory cache from the database (loading in batches)
2. Open your default browser to the Swagger UI page
3. Be ready to accept requests once cache initialization completes

Default URL: `https://localhost:7000` (or check the console output)

## API Endpoints

### Word Inversion

**POST** `/api/v1/words/inverse`

Inverses all words in the provided sentence.

Request:
```json
{
  "sentence": "Hello, World! This is a test."
}
```

Response:
```json
{
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "responseTime": "2025-12-21T10:30:00Z",
  "isSuccess": true,
  "errorMessage": null,
  "errors": [],
  "inversedSentence": "olleH, dlroW! sihT si a tset.",
  "processingTimeMs": 15
}
```

### Get All Request/Response Pairs

**GET** `/api/v1/requestresponse?pageNumber=1&pageSize=10`

Returns a paginated list of all request/response pairs.

Response:
```json
{
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "responseTime": "2025-12-21T10:30:00Z",
  "isSuccess": true,
  "data": [
    {
      "id": 1,
      "requestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "request": "{\"sentence\":\"Hello World\"}",
      "response": "{\"inversedSentence\":\"olleH dlroW\"}",
      "tags": "[\"Hello\",\"World\"]",
      "exception": null,
      "isSuccess": true,
      "createdDate": "2025-12-21T10:30:00Z",
      "processingTimeMs": 15
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 1,
  "totalPages": 1,
  "hasPrevious": false,
  "hasNext": false
}
```

### Search Request/Response by Word

**GET** `/api/v1/requestresponse/search?searchWord=Hello&pageNumber=1&pageSize=10`

Searches for request/response pairs containing the specified word.

## Word Inversion Logic

The word inversion algorithm follows these rules:

1. **Special characters at the start/end are preserved**: `"Hello!"` → `"olleH!"`
2. **Special characters in the middle are reversed with the word**: `"ab-cd"` → `"dc-ba"`
3. **Whitespace is preserved**: Multiple spaces and sentence structure remain intact
4. **Empty strings are handled**: Empty words or sentences return as-is

Examples:
- `"Hello, World!"` → `"olleH, dlroW!"`
- `"@test#"` → `"@tset#"`
- `"a-b-c"` → `"c-b-a"`
- `"--abc--"` → `"--cba--"`

## Memory Cache

The application uses in-memory caching for performance:

- Cache is initialized on startup by loading all word pairs from the database in batches
- Each word and its inverse are stored for quick lookup
- New word inversions are automatically cached and persisted to the database
- Cache initialization must complete before the API accepts requests
- If cache is not ready, endpoints return 503 Service Unavailable

## Middleware

### Request/Response Logging Middleware

Automatically logs all API requests and responses to the database including:
- Request body
- Response body
- Processing time
- Exception details (if any)
- Tags (extracted words from request for searching)

### Global Exception Handler Middleware

Catches and handles all unhandled exceptions with proper HTTP status codes:
- `CacheNotReadyException` → 503 Service Unavailable
- All other exceptions → 500 Internal Server Error

## Database Tables

### WordCache

Stores individual words and their inversed versions for caching.

| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary key (identity) |
| Word | NVARCHAR(500) | Original word (unique) |
| InversedWord | NVARCHAR(500) | Inversed version |
| CreatedDate | DATETIME2 | Creation timestamp |
| UpdatedDate | DATETIME2 | Last update timestamp |

### RequestResponse

Stores all API request/response pairs for auditing and searching.

| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary key (identity) |
| RequestId | UNIQUEIDENTIFIER | Unique request identifier |
| Request | NVARCHAR(MAX) | Request JSON |
| Response | NVARCHAR(MAX) | Response JSON |
| Tags | NVARCHAR(MAX) | JSON array of words for searching |
| Exception | NVARCHAR(MAX) | Exception details (if any) |
| IsSuccess | BIT | Success flag |
| CreatedDate | DATETIME2 | Request timestamp |
| ProcessingTimeMs | BIGINT | Processing time in milliseconds |

## Development

### Adding New Services

To add a new service that should be automatically registered:

1. Create a class implementing `IServiceRegistration` in the appropriate project
2. Implement the `RegisterServices` method
3. The service will be automatically discovered and registered during startup

Example:

```csharp
public class MyServiceRegistration : IServiceRegistration
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMyService, MyService>();
    }
}
```

### Adding Pagination

To add a paginated endpoint:

1. Create a request model inheriting from `PagedRequest`
2. Return a `PagedResponse<T>` from your service
3. Use the existing repository patterns for data access

## Troubleshooting

### Cache Not Ready Error

If you receive "503 Service Unavailable" with "Cache not ready" message:
- Wait a few moments for the cache initialization to complete
- Check the logs to see the progress of cache loading
- Verify database connectivity

### Database Connection Issues

If the application can't connect to the database:
- Verify SQL Server is running
- Check the connection string in appsettings.json
- Ensure the database was deployed successfully
- Check firewall settings

### Build Errors

If you encounter build errors:
- Ensure .NET 9 SDK is installed: `dotnet --version`
- Run `dotnet restore` to restore all NuGet packages
- Clean and rebuild: `dotnet clean && dotnet build`

## Project Structure

```
WordInverser/
├── WordInverser.API/              # Web API project
│   ├── Controllers/               # API controllers
│   ├── Middleware/                # Custom middleware
│   ├── HostedServices/            # Background services
│   └── Program.cs                 # Application entry point
├── WordInverser.Business/         # Business logic layer
│   ├── Interfaces/                # Service interfaces
│   └── Services/                  # Service implementations
├── WordInverser.DAL/              # Data access layer
│   ├── Context/                   # EF Core DbContext
│   ├── Entities/                  # Database entities
│   ├── Interfaces/                # Repository interfaces
│   └── Repositories/              # Repository implementations
├── WordInverser.Common/           # Shared code
│   ├── Interfaces/                # Common interfaces
│   ├── Models/                    # DTOs and models
│   └── Exceptions/                # Custom exceptions
├── WordInverser.Database/         # SQL Database project
│   ├── Tables/                    # Table definitions
│   ├── StoredProcedures/          # Stored procedures
│   └── Scripts/                   # Pre/Post deployment scripts
└── WordInverser.Tests/            # Unit test project
    ├── Business/                  # Business layer tests
    └── Controllers/               # Controller tests
```

## Technologies Used

- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 9.0**: ORM for database access
- **SQL Server**: Relational database
- **Microsoft.Build.Sql**: SQL Database project SDK
- **Swashbuckle**: Swagger/OpenAPI documentation
- **Memory Cache**: In-memory caching
- **API Versioning**: Version management for APIs

## License

This project is for reference purposes.

## Support

For issues or questions, please check the application logs in the console output.
