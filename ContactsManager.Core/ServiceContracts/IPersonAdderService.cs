using DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonAdderService
    {
        /// <summary>
        /// Add a new person into the list of persons
        /// </summary>
        /// <param name="personAddRequest"></param>
        /// <returns>Returns the same person details, aloing with newly generated PersonId</returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);
    }
}
