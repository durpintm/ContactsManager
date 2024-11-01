﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Domain Model for Country
    /// </summary>
    public class Country
    {
        [Key]
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
        public virtual ICollection<Person>? Persons { get; set; }
    }
}
