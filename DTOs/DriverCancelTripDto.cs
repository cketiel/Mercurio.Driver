﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.DTOs
{
    public class DriverCancelTripDto
    {
        [Required]
        public string Reason { get; set; }
    }
}
