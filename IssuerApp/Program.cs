using IssuerApp;
using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Extensions;
using IssuerApp.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.Title = "IssuerApp";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        theme: AnsiConsoleTheme.Literate)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // -- Services --

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddHttpClient(Constants.HttpClient.Default, _ => { })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                builder.Environment.IsDevelopment()
                    ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    : null
        });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(connectionString, sqliteOptions =>
        {
            sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqliteOptions.MigrationsAssembly("IssuerApp");
        });

        if (builder.Environment.IsDevelopment())
            options.EnableSensitiveDataLogging();

        options.ConfigureWarnings(warnings =>
            warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));
    });

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
        UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "VCdemos.IssuerApp.Identity";
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });

    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>()
        .SetApplicationName("IssuerApp");

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy => policy
            .AllowAnyMethod()
            .AllowAnyOrigin()
            .AllowAnyHeader());
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.WriteIndented = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

    builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizeFolder("/");
            options.Conventions.AllowAnonymousToPage("/Login");
            options.Conventions.AllowAnonymousToPage("/Error");
            options.Conventions.AllowAnonymousToPage("/Index");
        })
        .AddRazorRuntimeCompilation();

    builder.Services.AddJsonSerializerOptions();

    // VCALM: exchange records kept in memory for lifetime of process.
    builder.Services.AddSingleton<IExchangeService, ExchangeService>();

    // VC Bitstring Status List — singleton so the same list is shared across requests.
    builder.Services.AddSingleton<IStatusListService, StatusListService>();

    // did:web server for organizations
    builder.Services.Configure<DidWebOptions>(builder.Configuration.GetSection(DidWebOptions.SectionName));
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IDidWebService, DidWebService>();
    builder.Services.AddSingleton<IEd25519SigningService, Ed25519SigningService>();
    builder.Services.AddJsonLdCanonicalizationService();

    // -- Database seeding --

    var didWebHost = builder.Configuration["DidWeb:DefaultHost"]
        ?? throw new InvalidOperationException("Configuration 'DidWeb:DefaultHost' not found.");

    Log.Information("Connection string: {ConnectionString}", connectionString);
    Log.Information("DidWeb host: {DidWebHost}", didWebHost);
    Log.Information("Starting database seeding...");

    try
    {
        await SeedData.EnsureUserSeedData(connectionString, didWebHost, builder.Environment.IsDevelopment());
        Log.Information("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database seeding failed. Exception: {Message}", ex.Message);
        var inner = ex.InnerException;
        while (inner != null)
        {
            Log.Fatal("Inner exception: {InnerMessage}", inner.Message);
            inner = inner.InnerException;
        }
        throw;
    }

    // -- Middleware pipeline --

    var app = builder.Build();

    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();

    app.UseForwardedHeaders();

    var basePath = builder.Configuration[Constants.Configuration.BasePath];
    if (!string.IsNullOrEmpty(basePath))
    {
        Log.Debug("Found base path '{BasePath}'.", basePath);
        app.UsePathBase(basePath);
        app.Use((context, next) =>
        {
            if (string.IsNullOrEmpty(context.Request.PathBase))
                context.Request.PathBase = new PathString(basePath);
            return next();
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("CorsPolicy");
    app.UseStaticFiles();
    app.UseCookiePolicy();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapDefaultControllerRoute();

    Log.Information("Environment: {EnvironmentName}", app.Environment.EnvironmentName);
    Log.Information("ASPNETCORE_URLS: {Urls}", Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "(not set)");
    Log.Information("ASPNETCORE_HTTPS_PORT: {HttpsPort}", Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT") ?? "(not set)");

    var kestrelSection = builder.Configuration.GetSection("Kestrel:Endpoints");
    if (kestrelSection.Exists())
        Log.Information("Kestrel endpoints configured: {Endpoints}", string.Join(", ", kestrelSection.GetChildren().Select(c => c.Key)));
    else
        Log.Information("No Kestrel:Endpoints section in config.");

    Log.Information("Starting host...");
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly. Exception: {Message}", e.Message);
    var inner = e.InnerException;
    while (inner != null)
    {
        Log.Fatal("Inner exception: {InnerMessage}", inner.Message);
        inner = inner.InnerException;
    }
}
finally
{
    SqliteConnection.ClearAllPools();
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program { }
