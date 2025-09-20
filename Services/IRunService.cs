using Mercurio.Driver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public interface IRunService
    {
        Task<VehicleRoute> GetActiveRunByDriverIdAsync(int driverId);
    }
}
