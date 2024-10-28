using AutoFixture;
using ServiceContracts;
using DTO;
using Enums;
using Services;
using Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using Serilog;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace CRUDTests
{
    public class PersonServiceTests
    {
        private readonly IPersonGetterService _personsGetterService;
        private readonly IPersonAdderService _personsAdderervice;
        private readonly IPersonDeleterService _personsDeleterService;
        private readonly IPersonUpdaterService _personsUpdaterService;
        private readonly IPersonSorterService _personsSorterService;

        private readonly IPersonsRepository _personsRepository;

        private readonly Mock<IPersonsRepository> _personsRepositoryMock;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        public PersonServiceTests(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;
            var diagnosticContextMock = new Mock<IDiagnosticContext>();

            var loggerMockGetter = new Mock<ILogger<PersonGetterService>>();
            var loggerMockAdder = new Mock<ILogger<PersonAdderService>>();
            var loggerMockUpdater = new Mock<ILogger<PersonUpdaterService>>();
            var loggerMockSorter = new Mock<ILogger<PersonSorterService>>();

            _personsGetterService = new PersonGetterService(_personsRepository, loggerMockGetter.Object, diagnosticContextMock.Object);
            _personsAdderervice = new PersonAdderService(_personsRepository, loggerMockAdder.Object, diagnosticContextMock.Object);
            _personsDeleterService = new PersonDeleterService(_personsRepository);
            _personsUpdaterService = new PersonUpdaterService(_personsRepository, loggerMockUpdater.Object, diagnosticContextMock.Object);
            _personsSorterService = new PersonSorterService(_personsRepository, loggerMockSorter.Object, diagnosticContextMock.Object);

            _testOutputHelper = testOutputHelper;
        }


        #region AddPerson
        // When we supply a null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = null;

            // Act
            //await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            // {
            //     await _personService.AddPerson(personAddRequest);
            // });

            Func<Task> action = async () =>
            {
                await _personsAdderervice.AddPerson(personAddRequest);
            };
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        // When we supply a null value as PersonName, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.PersonName, null as string).Create();

            var person = personAddRequest.ToPerson();

            // When PersonsRepository.AddPerson() is called, it has to return the same "person" object
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            Func<Task> action = async () =>
            {
                await _personsAdderervice.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        // When we supply proper person details, it should insert the person into the person list; and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "someone@example.com").Create();

            var person = personAddRequest.ToPerson();
            PersonResponse person_response_expected = person.ToPersonResponse();

            // If we supply any argument value to the AddPerson method, it should return the same return value
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse person_response_from_add = await _personsAdderervice.AddPerson(personAddRequest);

            person_response_expected.PersonId = person_response_from_add.PersonId;

            // Assert
            //Assert.True(person_response_from_add.PersonId != Guid.Empty);
            //Assert.Contains(person_response_from_add, person_list);

            // Fluent Assertion
            person_response_from_add.PersonId.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().Be(person_response_expected);

        }
        #endregion

        #region GetPersonByPersonId
        // If we supply null as PersonId, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonById_NullPersonId_ToBeNull()
        {
            // Arrange
            Guid? personId = null;

            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonById(personId);

            // Assert
            //Assert.Null(person_response_from_get);

            person_response_from_get.Should().BeNull();
        }

        // If we supply a vaild person id, it should return the valid person details as PersonResponse object
        [Fact]
        public async Task GetPersonByPersonId_WithPersonId_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>().With(temp => temp.Email, "email@sample.com").With(temp => temp.Country, null as Country).Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonById(person.PersonId);

            // Assert
            //Assert.Equal(person_response_from_add, person_response_from_get);

            person_response_from_get.Should().Be(person_response_expected);
        }
        #endregion

        #region GetAllPersons
        // The GetAllPersons() should return an empty list by default 
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(new List<Person>());
            // Act
            List<PersonResponse> person_from_get = await _personsGetterService.GetAllPersons();

            // Assert
            //Assert.Empty(person_from_get);
            person_from_get.Should().BeEmpty();
        }

        // First, we will add few persons; and then when we call GetAllPersons(), it should return the same person that were added
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone4@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();


            // Print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach (var person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            // Act
            List<PersonResponse> person_list_from_get = await _personsGetterService.GetAllPersons();

            // Print person_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (var person_response_from_get in person_list_from_get)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            // Assert
            //foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            //{
            //    //Assert.Contains(person_response_from_add, person_list_from_get);
            //}

            person_list_from_get.Should().BeEquivalentTo(person_response_list_expected);

        }
        #endregion

        #region GetFilteredPersons
        // If the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            // Arrange
            List<Person> persons = new List<Person>() {

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone4@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            // Print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach (var person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            // Act
            List<PersonResponse> person_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "");

            // Print person_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (var person_response_from_get in person_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            person_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }

        // First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone4@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            // Print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach (var person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            // Act
            List<PersonResponse> person_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "sa");

            // Print person_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (var person_response_from_get in person_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            person_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }
        #endregion

        #region GetSortedPersons_Desc

        // When we sort based on PersonName in DESC, it should return persons list in descending order on PersonName
        [Fact]
        public async Task GetSortedPersons_Desc_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone1@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone2@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone3@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone4@email.com")
            .With(temp => temp.Country, null as Country)
            .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).
                            ReturnsAsync(persons);

            // Print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected: ");
            foreach (var person_response_from_add in persons)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            List<PersonResponse> allPersons = await _personsGetterService.GetAllPersons();

            // Act
            List<PersonResponse> person_list_from_sort = await _personsSorterService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

            // Print person_list_from_get
            _testOutputHelper.WriteLine("Actual: ");
            foreach (var person_response in person_list_from_sort)
            {
                _testOutputHelper.WriteLine(person_response.ToString());
            }

            person_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
        }

        #endregion

        #region UpdatePerson

        // When we supply null as PersonUpdateRequest, it should throw ArugmentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonUpdateRequest? person_update_request = null!;

            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        // When we supply invalid Person Id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonId_ToBeArgumentException()
        {
            // Arrange
            PersonUpdateRequest? person_update_request = _fixture.Build<PersonUpdateRequest>()
                .Create();

            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        // When the PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Email, "email@email.com")
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_from_add.ToPersonUpdateRequest();

            var action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        // First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetailsUpdation_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
               .With(temp => temp.Country, null as Country)
               .With(temp => temp.Email, "email@email.com")
               .With(temp => temp.Gender, "Male")
               .Create();


            PersonResponse person_response_expected = person.ToPersonResponse();


            PersonUpdateRequest? person_update_request = person_response_expected.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);

            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            // Act
            PersonResponse person_response_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);

            // Assert
            //Assert.Equal(person_response_from_update, person_response_from_get);

            person_response_from_update.Should().Be(person_response_expected);
        }
        #endregion

        #region DeletePerson

        // If you supply an valid PersonId, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonId_ToBeSuccessful()
        {

            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "someone@email.com")
                .With(temp => temp.Gender, "Male")
                .With(temp => temp.Country, null as Country)
                .Create();

            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(true);
            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            bool isDeleted = await _personsDeleterService.DeletePerson(person.PersonId);

            // Assert
            //Assert.True(isDeleted);
            isDeleted.Should().BeTrue();
        }

        // If you supply an invalid PersonId, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonId()
        {
            // Act
            bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());

            // Assert
            //Assert.False(isDeleted);
            isDeleted.Should().BeFalse();
        }
        #endregion
    }
}
