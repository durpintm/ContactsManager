using Enums;
using Entities;
using System.ComponentModel.DataAnnotations;

namespace DTO
{
    /// <summary>
    /// Acts as a DTO for updating an existing Person
    /// </summary>
    public class PersonUpdateRequest
    {
        [Required(ErrorMessage = "Person Id can't be blank")]
        public Guid PersonId { get; set; }
        [Required(ErrorMessage = "Person Name cannot be blank")]
        public string? PersonName { get; set; }
        [Required(ErrorMessage = "Email cannot be blank"), EmailAddress(ErrorMessage = "Email is not valid")]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public GenderOptions? Gender { get; set; }
        public Guid? CountryId { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }


        /// <summary>
        /// Converts the current object of PersonAddRequest into a new object of Person type
        /// </summary>
        /// <returns>Returns Person object</returns>
        public Person ToPerson()
        {
            return new Person() { PersonId = PersonId, PersonName = PersonName, Email = Email, DateOfBirth = DateOfBirth, Gender = Gender.ToString(), CountryId = CountryId, };
        }
    }
}
