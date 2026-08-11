using Raphael.Driver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raphael.Driver.Services
{
    public interface IRunService
    {
        Task<VehicleRoute> GetActiveRunByDriverIdAsync(int driverId);
    }
}
