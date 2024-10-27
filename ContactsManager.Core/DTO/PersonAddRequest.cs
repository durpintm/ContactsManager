using CRUDServiceContracts.Enums;
using Entities;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    /// <summary>
    /// Acts as a DTO for inserting a new Person
    /// </summary>
    public class PersonAddRequest
    {
        [Required(ErrorMessage = "Person Name cannot be blank")]
        public string? PersonName { get; set; }
        [Required(ErrorMessage = "Email cannot be blank"), EmailAddress(ErrorMessage = "Email is not valid")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender cannot be blank")]
        public GenderOptions? Gender { get; set; }

        [Required(ErrorMessage = "Please select a country")]
        public Guid? CountryId { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        /// <summary>
        /// Converts the current object of PersonAddRequest into a new object of Person type
        /// </summary>
        /// <returns>Returns Person object</returns>
        public Person ToPerson()
        {
            return new Person() { PersonName = PersonName, Email = Email, DateOfBirth = DateOfBirth, Gender = Gender.ToString(), CountryId = CountryId, Address = Address, ReceiveNewsLetters = ReceiveNewsLetters };
        }
    }
}
