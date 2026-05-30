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
using WalletApp;
using WalletApp.Data;
using WalletApp.Data.Models;
using WalletApp.Extensions;
using WalletApp.Services;

Console.Title = "WalletApp";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
        theme: AnsiConsoleTheme.Code)
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

    var didWebHost = builder.Configuration["DidWeb:DefaultHost"]
        ?? throw new InvalidOperationException("DidWeb:DefaultHost not found in configuration.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(connectionString, sqliteOptions =>
        {
            sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            sqliteOptions.MigrationsAssembly("WalletApp");
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
        options.Cookie.Name = "VCdemos.WalletApp.Identity";
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
    });

    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>()
        .SetApplicationName("WalletApp");

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

    // VCALM: mock Holder Coordinator service for the simple issuance workflow.
    builder.Services.AddScoped<IHolderExchangeService, HolderExchangeService>();

    // VCALM: Holder Coordinator service for the verification workflow.
    builder.Services.AddScoped<IHolderVerificationService, HolderVerificationService>();

    // VC Rendering Methods – html render suite host-page resolver.
    builder.Services.AddScoped<WalletApp.Services.VcRender.HtmlRenderSuiteService>();

    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();

    // did:web server for holders
    builder.Services.Configure<DidWebOptions>(builder.Configuration.GetSection(DidWebOptions.SectionName));
    builder.Services.AddScoped<IDidWebHolderService, DidWebHolderService>();
    builder.Services.AddSingleton<Library.Crypto.IEd25519SigningService, Library.Crypto.Ed25519SigningService>();
    builder.Services.AddJsonLdCanonicalizationService();

    // -- Database seeding --

    Log.Information("Default connection string: {ConnectionString}", connectionString);
    Log.Information("Starting database seeding (Default)...");

    try
    {
        await SeedData.EnsureUserSeedData(connectionString, didWebHost, builder.Environment.IsDevelopment());
        Log.Information("Default database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Default database seeding failed. Exception: {Message}", ex.Message);
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
