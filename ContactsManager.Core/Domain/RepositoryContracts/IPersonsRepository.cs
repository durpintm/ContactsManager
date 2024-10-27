using Entities;
using System.Linq.Expressions;

namespace RepositoryContracts
{
    public interface IPersonsRepository
    {
        Task<Person> AddPerson(Person person);
        Task<List<Person>> GetAllPersons();
        Task<Person?> GetPersonById(Guid personId);
        Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate);
        Task<bool> DeletePersonByPersonId(Guid personId);
        Task<Person> UpdatePerson(Person person);
    }
}
