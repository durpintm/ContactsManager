using ServiceContracts;
using Services.Helper;
using DTO;
using Entities;
using Exceptions;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Serilog;

namespace Services
{
    public class PersonUpdaterService : IPersonUpdaterService
    {
        private readonly IPersonsRepository _personsRepository;

        private readonly ILogger<PersonUpdaterService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;
        public PersonUpdaterService(IPersonsRepository personsRepository, ILogger<PersonUpdaterService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest personUpdateRequest)
        {
            if (personUpdateRequest == null) throw new ArgumentNullException(nameof(Person));

            // Validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            // Get matching person object to update
            Person? matchingPerson = await _personsRepository.GetPersonById(personUpdateRequest.PersonId);

            if (matchingPerson == null)
            {
                throw new InvalidPersonIdException("Given person Id doesn't exist");
            }

            // update all details
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryId = personUpdateRequest.CountryId;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _personsRepository.UpdatePerson(matchingPerson);
            return matchingPerson.ToPersonResponse();
        }
    }
}
