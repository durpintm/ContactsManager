using DTO;
using CRUDServiceContracts.Enums;

namespace CRUDServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonDeleterService
    {
        /// <summary>
        /// Deletes a person based on the given person id
        /// </summary>
        /// <param name="personId">Person Id to delete</param>
        /// <returns>Returns true if the deletion is successful; else false</returns>
        Task<bool> DeletePerson(Guid? personId);
    }
}
