using Filters.ActionFilters;
using ServiceContracts;
using Services;
using DatabaseContext;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;

namespace ServicesExtensions
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ResponseHeaderActionFilter>();

            // It adds controllers and views as services
            services.AddControllersWithViews(options =>
            {
                //options.Filters.Add< ResponseHeaderActionFilter>();

                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

                //options.Filters.Add(new ResponseHeaderActionFilter(logger, "My-Key-From-Global", "My-Value-From-Global", 2));

                options.Filters.Add(new ResponseHeaderActionFilter(logger) { Key = "My-Key-From-Global", Value = "My-Value-From-Global", Order = 2 });
            });

            // Add services into IoC container
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();
            services.AddScoped<ICountryService, CountryService>();

            services.AddScoped<IPersonGetterService, PersonGetterServiceWithFewExcelFields>();
            //services.AddScoped<IPersonGetterService, PersonGetterServiceChild>();
            services.AddScoped<PersonGetterService, PersonGetterService>();

            services.AddScoped<IPersonAdderService, PersonAdderService>();
            services.AddScoped<IPersonDeleterService, PersonDeleterService>();
            services.AddScoped<IPersonUpdaterService, PersonUpdaterService>();
            services.AddScoped<IPersonSorterService, PersonSorterService>();

            services.AddTransient<PersonsListActionFilter>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                //options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]);

                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.RequestProperties | HttpLoggingFields.ResponsePropertiesAndHeaders;
            });

            return services;
        }
    }
}
