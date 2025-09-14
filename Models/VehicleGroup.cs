﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Models
{
    public class VehicleGroup
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        //public ICollection<Vehicle> Vehicles { get; set; }
    }
}
