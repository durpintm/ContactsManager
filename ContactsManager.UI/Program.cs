using Middleware;
using Rotativa.AspNetCore;
using Serilog;
using ServicesExtensions;

var builder = WebApplication.CreateBuilder(args);

//builder.Logging.ClearProviders().AddConsole();
//builder.Logging.AddDebug();
//builder.Logging.AddEventLog();

// Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration) // reading configuration from appsettings.json / built in IConfiguration
    .ReadFrom.Services(services);// read out current app's servicesand make them available to serilog
});

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("info-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");


if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    //app.UseExceptionHandlingMiddleware();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
}

if (builder.Environment.IsEnvironment("Test") == false)
{
    RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

//app.UseStatusCodePagesWithRedirects("/{0}");

app.UseSerilogRequestLogging();

// Http logging
app.UseHttpLogging();

app.UseStaticFiles();

app.UseRouting(); // Identifying action method based on url route

app.UseAuthentication(); // Reading Identity Cookie from the browser
app.UseAuthorization(); // Validates access permissions of the user
//app.MapControllers(); // Execute the filter pipeline (action + filters)

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{controller}/{action}/{id?}"
//        );
//});

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//        name: "default",
//        pattern: "{controller}/{action}/{id?}");
//});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();


public partial class Program { } // make the auto-generated Program accessible programmatically