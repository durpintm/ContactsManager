using ContactsManager.Core.Domain.IdentityEntities;
using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DatabaseContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        } 
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries");

            modelBuilder.Entity<Person>().ToTable("Persons");

            // Seed Data to Countries
            string countriesJson = File.ReadAllText("countries.json");

            List<Country>? countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);

            if (countries != null)
            {
                foreach (var country in countries)
                {
                    modelBuilder.Entity<Country>().HasData(country);
                }
            }

            // Seed Data for Persons
            string personsJson = File.ReadAllText("persons.json");
            List<Person>? persons = JsonSerializer.Deserialize<List<Person>>(personsJson);

            if (persons != null)
            {
                foreach (var person in persons)
                {
                    modelBuilder.Entity<Person>().HasData(person);
                }
            }

            // Fluent Api
            modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABCDEF12");

            //modelBuilder.Entity<Person>().HasIndex(temp => temp.TIN).IsUnique();

            //modelBuilder.Entity<Person>().HasCheckConstraint("CHK_TaxIdentificationNumber", "len([TaxIdentificationNumber]) = 8");

            modelBuilder.Entity<Person>().ToTable(t => t.HasCheckConstraint("CHK_TaxIdentificationNumber", "len([TaxIdentificationNumber]) = 8"));

            // Table Relations
            //modelBuilder.Entity<Person>(entity =>
            //{
            //    entity.HasOne<Country>(c => c.Country)
            //        .WithMany(p => p.Persons)
            //        .HasForeignKey(p => p.CountryId);
            //});
        }

        public List<Person> sp_GetAllPersons()
        {
            var data = Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
            return data;
        }

        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] sqlParameters = new SqlParameter[] {
            new SqlParameter("@PersonId", person.PersonId),
            new SqlParameter("@PersonName", person.PersonName),
            new SqlParameter("@Email", person.Email),
            new SqlParameter("@DateOfBirth", person.DateOfBirth),
            new SqlParameter("@Gender", person.Gender),
            new SqlParameter("@CountryId", person.CountryId),
            new SqlParameter("@Address", person.Address),
            new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters),
            };

            return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonId, @PersonName, @Email, @DateOfBirth, @Gender, @CountryId, @Address, @ReceiveNewsLetters", sqlParameters);
        }
    }
}
