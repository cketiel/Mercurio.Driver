using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public interface IGpsService
    {
        bool IsTracking { get; }
        void StartTracking(int vehicleRouteId);
        void StopTracking();
        Task<Location?> GetCurrentLocationAsync();
    }
}
