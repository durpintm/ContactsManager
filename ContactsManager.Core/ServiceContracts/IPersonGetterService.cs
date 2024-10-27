using DTO;
using Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonGetterService
    {
        /// <summary>
        /// Returns all persons
        /// </summary>
        /// <returns>Returns a list of objects of PersonResponse type</returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns the person object on the given person id
        /// </summary>
        /// <param name="personId">Person id to search</param>
        /// <returns>Returns the matching person object</returns>
        Task<PersonResponse?> GetPersonById(Guid? personId);

        /// <summary>
        /// Returns all person object that matches with the given search field and search string
        /// </summary>
        /// <param name="searchBy">Search field to search</param>
        /// <param name="searchString">Search string to search</param>
        /// <returns>Returns all matching persons based on the given search field and search string</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string searchString);

        /// <summary>
        /// Returns the person as csv
        /// </summary>
        /// <returns>Returns the memory stream with csv data</returns>
        Task<MemoryStream> GetPersonsCSV();

        /// <summary>
        /// Returns the person as excel
        /// </summary>
        /// <returns>Returns the memory stream with excel data</returns>
        Task<MemoryStream> GetPersonsExcel();

    }
}
