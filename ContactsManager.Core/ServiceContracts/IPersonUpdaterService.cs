using DTO;
using Enums;

namespace CRUDServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonUpdaterService
    {
        /// <summary>
        /// Updates the specified person details based on the given person Id
        /// </summary>
        /// <param name="personUpdateRequest">Person details to update, including person Id</param>
        /// <returns>Returns the Person response object after the updation</returns>
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest personUpdateRequest);
    }
}
