using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Models
{
    public class GPS
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdVehicleRoute { get; set; }
        [ForeignKey("IdVehicleRoute")]
        public VehicleRoute VehicleRoute { get; set; }

        public DateTime DateTime { get; set; }

        public double Speed { get; set; }

        public string? Address { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public string? Direction { get; set; }
    }
}
