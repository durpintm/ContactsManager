using ServiceContracts;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Serilog;

namespace Services
{
    public class PersonDeleterService : IPersonDeleterService
    {
        private readonly IPersonsRepository _personsRepository;

        public PersonDeleterService(IPersonsRepository personsRepository)
        {
            _personsRepository = personsRepository;
        }

        public async Task<bool> DeletePerson(Guid? personId)
        {
            if (personId == null)
            {
                return false;
            }

            Person? person = await _personsRepository.GetPersonById(personId.Value);

            if (person == null) { return false; }

            await _personsRepository.DeletePersonByPersonId(personId.Value);

            return true;
        }

    }
}
