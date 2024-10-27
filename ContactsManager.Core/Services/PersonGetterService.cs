using ServiceContracts;
using DTO;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using SerilogTimings;
using System.Globalization;

namespace Services
{
    public class PersonGetterService : IPersonGetterService
    {
        private readonly IPersonsRepository _personsRepository;

        private readonly ILogger<PersonGetterService> _logger;

        private readonly IDiagnosticContext _diagnosticContext;
        public PersonGetterService(IPersonsRepository personsRepository, ILogger<PersonGetterService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public virtual async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonService");

            var persons = await _personsRepository.GetAllPersons();

            return persons.Select(temp => temp.ToPersonResponse()).ToList();

            //return _db.sp_GetAllPersons().Select(temp => ConvertPersonIntoPersonResponse(temp)).ToList();
        }

        public virtual async Task<PersonResponse?> GetPersonById(Guid? personId)
        {
            if (personId == null)
            {
                return null;
            }

            Person? person = await _personsRepository.GetPersonById(personId.Value);

            if (person == null)
            {
                return null;
            }

            return person.ToPersonResponse();

        }

        public virtual async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons of PersonService");
            List<Person> persons;
            using (Operation.Time("Time for Filtered Persons from Database"))
            {

                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) =>
                       await _personsRepository.GetFilteredPersons(temp => temp.PersonName.Contains(searchString)),

                    nameof(PersonResponse.Email) =>
                        await _personsRepository.GetFilteredPersons(temp => (!string.IsNullOrEmpty(temp.Email) ? temp.Email.Contains(searchString) : true)),

                    nameof(PersonResponse.DateOfBirth) =>
                         await _personsRepository.GetFilteredPersons(temp => (temp.DateOfBirth != null) ? temp.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString) : true),

                    nameof(PersonResponse.Gender) =>
                       await _personsRepository.GetFilteredPersons(temp => (!string.IsNullOrEmpty(temp.Gender) ? temp.Gender.Contains(searchString) : true)),

                    nameof(PersonResponse.CountryId) =>
                        await _personsRepository.GetFilteredPersons(temp => temp.Country != null && temp.Country.CountryName != null && temp.Country.CountryName.Contains(searchString)),

                    _ => await _personsRepository.GetAllPersons()
                };
            }

            _diagnosticContext.Set("Persons", persons);

            return persons.Select(temp => temp.ToPersonResponse()).ToList();
        }

        public virtual async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration);

            //csvWriter.WriteHeader<PersonResponse>(); // PersonId, PersonName etc..
            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
            csvWriter.NextRecord();

            List<PersonResponse> persons = await GetAllPersons();

            foreach (var person in persons)
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);
                csvWriter.WriteField(person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.ReceiveNewsLetters);
                csvWriter.NextRecord();
                csvWriter.Flush();
            }

            //await csvWriter.WriteRecordsAsync(persons);

            memoryStream.Position = 0;
            return memoryStream;
        }

        public virtual async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");
                worksheet.Cells["A1"].Value = "Person Name";
                worksheet.Cells["B1"].Value = "Email";
                worksheet.Cells["C1"].Value = "DateOfBirth";
                worksheet.Cells["D1"].Value = "Age";
                worksheet.Cells["E1"].Value = "Gender";
                worksheet.Cells["F1"].Value = "Country";
                worksheet.Cells["G1"].Value = "Address";
                worksheet.Cells["H1"].Value = "ReceiveNewsLetters";

                using (ExcelRange headerCells = worksheet.Cells["A1:H1"])
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> persons = await GetAllPersons();

                foreach (var person in persons)
                {
                    worksheet.Cells[row, 1].Value = person.PersonName;
                    worksheet.Cells[row, 2].Value = person.Email;
                    if (person.DateOfBirth.HasValue)
                    {
                        worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("yyyy-MM-dd");

                    }
                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.Country;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;
                    row++;
                }
                worksheet.Cells[$"A1:H{row}"].AutoFitColumns();
                await excelPackage.SaveAsync();
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
