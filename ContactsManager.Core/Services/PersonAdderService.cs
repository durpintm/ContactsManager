using ServiceContracts;
using Services.Helper;
using DTO;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Serilog;

namespace Services
{
    public class PersonAdderService : IPersonAdderService
    {
        private readonly IPersonsRepository _personsRepository;

        private readonly ILogger<PersonAdderService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;
        public PersonAdderService(IPersonsRepository personsRepository, ILogger<PersonAdderService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            if (personAddRequest == null) throw new ArgumentNullException(nameof(PersonAddRequest));

            // Validate PersonName
            //if (string.IsNullOrEmpty(personAddRequest.PersonName))
            //{
            //    throw new ArgumentException("Person Name cannot be blank");
            //}

            // Model Validations
            ValidationHelper.ModelValidation(personAddRequest);

            // Convert personAddRequest into Person type
            Person person = personAddRequest.ToPerson();

            person.PersonId = Guid.NewGuid();

            await _personsRepository.AddPerson(person);

            //_db.sp_InsertPerson(person);

            // COnvert the person object into PersonResponse type
            PersonResponse personResponse = person.ToPersonResponse();

            return personResponse;

        }
    }
}
