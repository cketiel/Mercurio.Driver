using Mercurio.Driver.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public interface ISessionManagerService
    {
        Task CheckAndResumeGpsTrackingAsync(ObservableCollection<ScheduleDto> pendingEvents);
    }
}
