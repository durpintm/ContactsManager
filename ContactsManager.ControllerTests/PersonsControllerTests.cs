using AutoFixture;
using Controllers;
using ServiceContracts;
using DTO;
using Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CRUDTests
{
    public class PersonsControllerTests
    {
        private readonly ICountryService _countryService;
        private readonly IPersonGetterService _personsGetterService;
        private readonly IPersonAdderService _personsAdderService;
        private readonly IPersonDeleterService _personsDeleterService;
        private readonly IPersonUpdaterService _personsUpdaterService;
        private readonly IPersonSorterService _personsSorterService;

        private readonly Mock<ICountryService> _countryServiceMock;
        private readonly Mock<IPersonGetterService> _personsGetterServiceMock;
        private readonly Mock<IPersonAdderService> _personsAdderServiceMock;
        private readonly Mock<IPersonDeleterService> _personsDeleterServiceMock;
        private readonly Mock<IPersonUpdaterService> _personsUpdaterServiceMock;
        private readonly Mock<IPersonSorterService> _personsSorterServiceMock;

        private readonly Fixture _fixture;

        private readonly ILogger<PersonsController> _logger;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        public PersonsControllerTests()
        {
            _fixture = new Fixture();
            _countryServiceMock = new Mock<ICountryService>();

            _personsGetterServiceMock = new Mock<IPersonGetterService>();
            _personsAdderServiceMock = new Mock<IPersonAdderService>();
            _personsDeleterServiceMock = new Mock<IPersonDeleterService>();
            _personsUpdaterServiceMock = new Mock<IPersonUpdaterService>();
            _personsSorterServiceMock = new Mock<IPersonSorterService>();

            _loggerMock = new Mock<ILogger<PersonsController>>();

            _countryService = _countryServiceMock.Object;
            _personsGetterService = _personsGetterServiceMock.Object;
            _personsAdderService = _personsAdderServiceMock.Object;
            _personsSorterService = _personsSorterServiceMock.Object;
            _personsUpdaterService = _personsUpdaterServiceMock.Object;
            _personsDeleterService = _personsDeleterServiceMock.Object;
            _logger = _loggerMock.Object;
        }

        #region Index
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            // Arrange
            List<PersonResponse> persons_response_list = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsSorterService, _personsUpdaterService, _personsDeleterService, _countryService, _logger);

            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(persons_response_list);

            _personsSorterServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>())).ReturnsAsync(persons_response_list);

            // Act
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();

            viewResult.ViewData.Model.Should().Be(persons_response_list);
        }
        #endregion

        #region Create
        //[Fact]
        //public async Task Create_IfModelErrors_ToReturnCreateView()
        //{
        //    // Arrange
        //    PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();

        //    PersonResponse person_response = _fixture.Create<PersonResponse>();

        //    List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

        //    _countryServiceMock.Setup(temp => temp.GetCountryList()).ReturnsAsync(countries);

        //    _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);

        //    PersonsController personsController = new PersonsController(_personsService, _countryService, _logger);

        //    // Act
        //    personsController.ModelState.AddModelError("PersonName", "Person Name cannot be blank!");

        //    IActionResult result = await personsController.Create(person_add_request);

        //    // Assert
        //    ViewResult viewResult = Assert.IsType<ViewResult>(result);

        //    viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();

        //    viewResult.ViewData.Model.Should().Be(person_add_request);
        //}


        #endregion

        #region Create 2
        [Fact]
        public async Task Create_IfNoModelErrors_ToReturnRedirectToIndexView()
        {
            // Arrange
            PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();

            PersonResponse person_response = _fixture.Create<PersonResponse>();

            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            _countryServiceMock.Setup(temp => temp.GetCountryList()).ReturnsAsync(countries);

            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsSorterService, _personsUpdaterService, _personsDeleterService, _countryService, _logger);

            // Act
            IActionResult result = await personsController.Create(person_add_request);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewName.Should().Be("Index");
        }
        #endregion

    }
}
