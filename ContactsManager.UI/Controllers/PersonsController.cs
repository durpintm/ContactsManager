using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilters;
using CRUDExample.Filters.ExceptionFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using CRUDServiceContracts;
using DTO;
using Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-From-Controller", "My-Key-From-Controller", 3 }, Order = 3)]
    [TypeFilter(typeof(HandleExceptionFilter))]
    [TypeFilter(typeof(PersonsAlwaysRunResultFilter))]
    public class PersonsController : Controller
    {
        private readonly IPersonGetterService _personGetterService;
        private readonly IPersonAdderService _personAdderService;
        private readonly IPersonSorterService _personSorterService;
        private readonly IPersonUpdaterService _personUpdaterService;
        private readonly IPersonDeleterService _personDeleterService;
        private readonly ICountryService _countryService;

        private readonly ILogger<PersonsController> _logger;
        public PersonsController(IPersonGetterService personGetterService, IPersonAdderService personAdderService, IPersonSorterService personSorterService, IPersonUpdaterService personUpdaterService, IPersonDeleterService personDeleterService, ICountryService countryService, ILogger<PersonsController> logger)
        {
            _personGetterService = personGetterService;
            _personAdderService = personAdderService;
            _personSorterService = personSorterService;
            _personUpdaterService = personUpdaterService;
            _personDeleterService = personDeleterService;
            _countryService = countryService;
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        [ServiceFilter(typeof(PersonsListActionFilter), Order = 4)]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "My-Key-From-Action", "My-Key-From-Action", 1 }, Order = 1)]
        //[ResponseHeaderActionFilter("my-key", "my-value", 1)]
        [ResponseHeaderFilterFactory("My-Key-From-Action", "My-Key-From-Action", 1)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        [SkipFilter]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            _logger.LogInformation("Index action method of PersonsController");
            //return NotFound();

            _logger.LogDebug($"SearchBy: {searchBy}, SearchString: {searchString}, SortBy: {sortBy}, SortOrder: {sortOrder}");

            List<PersonResponse> people = await _personGetterService.GetFilteredPersons(searchBy, searchString);
            //ViewBag.CurrentSearchBy = searchBy;
            //ViewBag.CurrentSearchString = searchString;

            // Sort
            List<PersonResponse> sortedPersons = await _personSorterService.GetSortedPersons(people, sortBy, sortOrder);

            //ViewBag.CurrentSortBy = sortBy.ToString();
            //ViewBag.CurrentSortOrder = sortOrder.ToString();
            _logger.LogInformation("Index action method completed of PersonsController");

            if (people.Count > 0)
            {
                return View(sortedPersons);
            }

            return View();
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            List<CountryResponse> countries = await _countryService.GetCountryList();
            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem()
            {
                Text = temp.CountryName,
                Value = temp.CountryId.ToString()
            });

            return View();
        }

        [Route("[action]")]
        [HttpPost]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] { false })]
        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {
            PersonResponse personResponse = await _personAdderService.AddPerson(personRequest);
            return View("Index");
        }

        [Route("[action]/{personId}")]
        [HttpGet]
        [TypeFilter(typeof(TokenResultFilter))]
        public async Task<IActionResult> Edit(Guid? personId)
        {
            PersonResponse? personResponse = await _personGetterService.GetPersonById(personId);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countries = await _countryService.GetCountryList();

            ViewBag.Countries = countries.Select(temp =>
            new SelectListItem()
            {
                Text = temp.CountryName,
                Value = temp.CountryId.ToString()
            });
            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personId}")]
        [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorizationFilter))]
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
        {
            PersonResponse? personResponse = await _personGetterService.GetPersonById(personRequest.PersonId);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonResponse updatedPerson = await _personUpdaterService.UpdatePerson(personRequest);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personId}")]
        public async Task<IActionResult> Delete(Guid? personId)
        {
            PersonResponse? personResponse = await _personGetterService.GetPersonById(personId);

            if (personResponse == null) return RedirectToAction("Index");

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personId}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personGetterService.GetPersonById(personUpdateRequest.PersonId);

            if (personResponse == null) { return RedirectToAction("Index"); }

            await _personDeleterService.DeletePerson(personResponse.PersonId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("PersonsPdf")]
        public async Task<IActionResult> PersonsPdf()
        {
            List<PersonResponse> persons = await _personGetterService.GetAllPersons();

            return new ViewAsPdf("PersonsPDF", persons, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins() { Top = 20, Right = 20, Bottom = 20, Left = 20 },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [HttpGet]
        [Route("PersonsCsv")]
        public async Task<IActionResult> PersonsCsv()
        {
            MemoryStream memoryStream = await _personGetterService.GetPersonsCSV();
            return File(memoryStream, "application/octet-stream", "persons.csv");
        }

        [HttpGet]
        [Route("PersonsExcel")]
        public async Task<IActionResult> PersonsExcel()
        {
            MemoryStream memoryStream = await _personGetterService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "persons.xlsx");
        }
    }
}
