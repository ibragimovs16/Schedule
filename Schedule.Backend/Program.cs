using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Schedule.DAL;
using Schedule.Domain.Models.Settings;
using Schedule.Services.Extensions;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();

var aspEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var appSettingsPath = "appsettings" + (aspEnv is null ? "" : $".{aspEnv}") + ".json";

#if DEBUG
Console.WriteLine("Debug mode");
#else
Console.WriteLine("Release mode");
#endif

// Setting up the DI

builder.Services.AddHttpClient();

builder.Services.InitializeRepositories();
builder.Services.InitializeServices();
builder.Services.InitializeHostedServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Schedule.API",
        Version = "v1",
        Description = "Апи для получения расписания для заочников университета \"Дубна\""
    });
    
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard auth",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    });
    
    c.OperationFilter<SecurityRequirementsOperationFilter>();
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile(appSettingsPath, true, true)
    .AddUserSecrets<Program>()
    .Build();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var securityKey = configuration.GetSection("JWT:SecurityKey").Value;

        if (securityKey is null)
            throw new Exception("Security key is not configured in appsettings.json");
            
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            LifetimeValidator = (_, expires, _, _) =>
                expires is not null && expires > DateTime.UtcNow
        };
    });

var dbConnectionSettings = configuration.GetSection("MsSql").Get<DbConnectionModel>();
builder.Services.AddDbContext<ApplicationDbContext>(
    o => o.UseSqlServer(
        dbConnectionSettings.ConnectionString
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();