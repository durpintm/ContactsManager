using ServiceContracts;
using DTO;
using Services;
using Entities;
using Moq;
using RepositoryContracts;
using FluentAssertions;
using AutoFixture;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountryService _countryService;

        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;

        private readonly ICountriesRepository _countriesRepository;

        private readonly IFixture _fixture;


        // Constructor
        public CountriesServiceTest()
        {
            //var countriesInitialData = new List<Country>() { };
            //DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            //ApplicationDbContext dbContext = dbContextMock.Object;

            //dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

            //_countryService = new CountryService(null);

            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;
            _countryService = new CountryService(_countriesRepository);

            _fixture = new Fixture();
        }

        #region AddCountry
        // When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            // Arrange
            CountryAddRequest? request = null;

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
             {
                 // Act
                 await _countryService.AddCountry(countryAddRequest: request);
             });
        }

        // When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            // Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = null };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
             {
                 // Act
                 await _countryService.AddCountry(countryAddRequest: request);
             });
        }

        // When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest first_country_request = _fixture.Build<CountryAddRequest>()
                 .With(temp => temp.CountryName, "Test name").Create();
            CountryAddRequest second_country_request = _fixture.Build<CountryAddRequest>()
              .With(temp => temp.CountryName, "Test name").Create();

            Country first_country = first_country_request.ToCountry();
            Country second_country = second_country_request.ToCountry();

            _countriesRepositoryMock
             .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
             .ReturnsAsync(first_country);

            //Return null when GetCountryByCountryName is called
            _countriesRepositoryMock
             .Setup(temp => temp.GetCountryByName(It.IsAny<string>()))
             .ReturnsAsync(null as Country);

            CountryResponse first_country_from_add_country = await _countryService.AddCountry(first_country_request);

            //Act
            var action = async () =>
            {
                //Return first country when GetCountryByCountryName is called
                _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(first_country);

                _countriesRepositoryMock.Setup(temp => temp.GetCountryByName(It.IsAny<string>())).ReturnsAsync(first_country);

                await _countryService.AddCountry(second_country_request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }


        // When you supply proper country name, it should insert (add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            // Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = "Japan" };

            // Act
            CountryResponse response = await _countryService.AddCountry(countryAddRequest: request);

            List<CountryResponse> countries_from_GetAllCountries = await _countryService.GetCountryList();

            // Assert
            Assert.True(response.CountryId != Guid.Empty);

            Assert.Contains(response, countries_from_GetAllCountries);
        }

        #endregion

        #region GetAllCountries

        // The list of countries should be empty by default (before adding any countries)
        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            // Act
            List<CountryResponse> actual_response = await _countryService.GetCountryList();

            // Assert
            Assert.Empty(actual_response);
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            // Arrange
            List<CountryAddRequest> country_request_list = new List<CountryAddRequest>()
            {
                new CountryAddRequest(){CountryName = "USA"},
                new CountryAddRequest(){CountryName = "Canada"},
                new CountryAddRequest(){CountryName = "Japan"},
                new CountryAddRequest(){CountryName = "Korea"},
            };

            // Act
            List<CountryResponse> countries_list_from_add_country = new List<CountryResponse>();

            foreach (CountryAddRequest country_request in country_request_list)
            {
                CountryResponse response = await _countryService.AddCountry(country_request);

                countries_list_from_add_country.Add(response);
            }

            List<CountryResponse> actualCountryResponseList = await _countryService.GetCountryList();

            // Read each element form countries_list_from_add_country
            foreach (CountryResponse expected_country in countries_list_from_add_country)
            {
                Assert.Contains(expected_country, actualCountryResponseList);
            }
        }

        #endregion

        #region GetCountryById

        // If we supply null as CountryId, it should return null as CountryResponse
        [Fact]
        public async Task GetCountyByCountryId_NullCountryId()
        {
            // Arrange
            Guid? countryId = null;

            // Act
            CountryResponse? country_response_from_get_method = await _countryService.GetCountryById(countryId);

            // Assert
            Assert.Null(country_response_from_get_method);
        }

        // If we supply a valid country id, it should return the matching country details as CountryResponse object
        [Fact]
        public async Task GetCountryByCountryId_ValidCountryId()
        {
            // Arrange
            CountryAddRequest request = new CountryAddRequest()
            {
                CountryName = "Netherlands"
            };

            CountryResponse country_response_from_add = await _countryService.AddCountry(request);

            // Act
            CountryResponse? countryResponse_from_get = await _countryService.GetCountryById(country_response_from_add.CountryId);

            Assert.Equal(country_response_from_add, countryResponse_from_get);
        }

        #endregion

    }
}
