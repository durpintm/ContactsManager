using CRUDExample.Controllers;
using CRUDServiceContracts;
using DTO;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRUDExample.Filters.ActionFilters
{
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
    {

        private readonly ICountryService _countryService;
        private readonly ILogger<PersonCreateAndEditPostActionFilter> _logger;
        public PersonCreateAndEditPostActionFilter(ICountryService countryService, ILogger<PersonCreateAndEditPostActionFilter> logger)
        {
            _countryService = countryService;
            _logger = logger;

        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is PersonsController personsController)
            {
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> countries = await _countryService.GetCountryList();
                    personsController.ViewBag.Countries = countries.Select(temp =>
                        new SelectListItem()
                        {
                            Text = temp.CountryName,
                            Value = temp.CountryId.ToString()
                        });

                    personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                    var personRequest = context.ActionArguments["personRequest"];

                    context.Result = personsController.View(personRequest); // short-circuits or skips the subsequent action filters and action methods
                }
                else
                {
                    await next(); // invokes the subsequent filter or action method
                }
            }
            else
            {
                await next();
            }

            _logger.LogInformation("In after logic of PersonsCreateAndEdit Action Filter");
        }
    }
}
