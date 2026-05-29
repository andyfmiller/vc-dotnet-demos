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
using VerifierApp;
using VerifierApp.Data;
using VerifierApp.Data.Models;
using VerifierApp.Extensions;

Console.Title = "VerifierApp";

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

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHttpClient(Constants.HttpClient.Default, _ => { })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
    }
    else
    {
        builder.Services.AddHttpClient(Constants.HttpClient.Default);
    }

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = _ => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(connectionString, sqliteOptions =>
        {
            sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqliteOptions.MigrationsAssembly("VerifierApp");
        });

        if (builder.Environment.IsDevelopment())
            options.EnableSensitiveDataLogging();

        options.ConfigureWarnings(warnings =>
            warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));
    });

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>,
        UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.Name = "VCdemos.VerifierApp.Identity";
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });

    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>()
        .SetApplicationName("VerifierApp");

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
            options.Conventions.AllowAnonymousToPage("/");
            options.Conventions.AllowAnonymousToPage("/Error");
            options.Conventions.AllowAnonymousToPage("/Index");
            options.Conventions.AllowAnonymousToPage("/ReleaseNotes");
        })
        .AddRazorRuntimeCompilation();

    builder.Services.AddJsonSerializerOptions();

    // VCALM: exchange records kept in memory for lifetime of process.
    builder.Services.AddSingleton<VerifierApp.Services.IVerificationExchangeService,
        VerifierApp.Services.VerificationExchangeService>();

    // Credential status: checks live Bitstring Status List on presented credentials.
    builder.Services.AddTransient<VerifierApp.Services.ICredentialStatusService,
        VerifierApp.Services.CredentialStatusService>();

    // Crypto services required by the VCALM controller.
    builder.Services.AddSingleton<Library.Crypto.IEd25519SigningService,
        Library.Crypto.Ed25519SigningService>();
    builder.Services.AddJsonLdCanonicalizationService();
    builder.Services.AddHttpContextAccessor();

    // -- Database seeding --

    Log.Information("Connection string: {ConnectionString}", connectionString);
    Log.Information("Starting database seeding...");

    try
    {
        await SeedData.EnsureUserSeedData(connectionString);
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

    // Give access to services when DI is not available
    ServiceActivator.Configure(app.Services);

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseMigrationsEndPoint();
    }

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
