using Controllers;
using DTO;
using Enums;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Filters.ActionFilters
{
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilter> _logger;
        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
        {
            _logger = logger;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // To do: add after logic here
            _logger.LogInformation("{FilterName}.{FilterMethod}", nameof(PersonsListActionFilter), nameof(OnActionExecuted));

            var personsController = (PersonsController)context.Controller;

            IDictionary<string, object?>? parameters = (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];

            if (parameters != null)
            {
                if (parameters.ContainsKey("searchBy"))
                {
                    personsController.ViewData["CurrentSearchBy"] = Convert.ToString(parameters[key: "searchBy"]);
                }

                if (parameters.ContainsKey("searchString"))
                {
                    personsController.ViewData["CurrentSearchString"] = Convert.ToString(parameters[key: "searchString"]);
                }

                if (parameters.ContainsKey("sortBy"))
                {
                    personsController.ViewData["CurrentSortBy"] = Convert.ToString(parameters[key: "sortBy"]);
                }
                else
                {
                    personsController.ViewData["CurrentSortBy"] = nameof(PersonResponse.PersonName);
                }

                if (parameters.ContainsKey("sortOrder"))
                {
                    personsController.ViewData["CurrentSortOrder"] = Convert.ToString(parameters[key: "sortOrder"]);
                }
                else
                {
                    personsController.ViewData["CurrentSortOrder"] = nameof(SortOrderOptions.ASC);
                }
            }

            personsController.ViewBag.SearchFields = new Dictionary<string, string> {
                { nameof(PersonResponse.PersonName),"Person Name"},
                { nameof(PersonResponse.Email),"Email"},
                { nameof(PersonResponse.DateOfBirth),"Date Of Birth"},
                { nameof(PersonResponse.Gender),"Gender"},
                { nameof(PersonResponse.CountryId),"Country"},
                { nameof(PersonResponse.Address),"Address"},
            };

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // To do: add before logic here
            _logger.LogInformation("{FilterName}.{FilterMethod}", nameof(PersonsListActionFilter), nameof(OnActionExecuting));

            context.HttpContext.Items["arguments"] = context.ActionArguments;

            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

                if (!string.IsNullOrEmpty(searchBy))
                {
                    var searchByOptions = new List<string>()
                    {
                        nameof(PersonResponse.PersonName),
                        nameof(PersonResponse.Email),
                        nameof(PersonResponse.DateOfBirth),
                        nameof(PersonResponse.Gender),
                        nameof(PersonResponse.CountryId),
                        nameof(PersonResponse.Address),
                    };

                    // reset the sarchBy parameter value
                    if (!searchByOptions.Any(temp => temp == searchBy))
                    {
                        _logger.LogInformation("SearchBy actual value {searchBy}", searchBy);

                        context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                        _logger.LogInformation("SearchBy updated value {searchBy}", context.ActionArguments["searchBy"]);
                    }
                }
            }
        }
    }
}
