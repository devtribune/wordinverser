using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using WordInverser.API.HostedServices;
using WordInverser.API.Middleware;
using WordInverser.Business;
using WordInverser.Common.Interfaces;
using WordInverser.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Word Inverser API",
        Version = "v1",
        Description = "API for inverting words in sentences while preserving special characters"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Dynamic Service Registration - Load assemblies explicitly
var assembliesToScan = new[]
{
    typeof(Program).Assembly,              // API
    typeof(DALServiceRegistration).Assembly,    // DAL
    typeof(BusinessServiceRegistration).Assembly // Business
};

var serviceRegistrations = assembliesToScan
    .SelectMany(assembly => assembly.GetTypes())
    .Where(type => typeof(IServiceRegistration).IsAssignableFrom(type) && 
                   !type.IsInterface && 
                   !type.IsAbstract)
    .ToList();

foreach (var registrationType in serviceRegistrations)
{
    var registration = Activator.CreateInstance(registrationType) as IServiceRegistration;
    registration?.RegisterServices(builder.Services, builder.Configuration);
}

// Add Hosted Service for Cache Initialization
builder.Services.AddHostedService<CacheInitializationHostedService>();

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Word Inverser API V1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

// Global Exception Handler
app.UseGlobalExceptionHandler();

// Request/Response Logging Middleware
app.UseRequestResponseLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();
