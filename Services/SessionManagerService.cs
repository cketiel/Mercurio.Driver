using Mercurio.Driver.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercurio.Driver.Services
{
    public class SessionManagerService : ISessionManagerService
    {
        private readonly IScheduleService _scheduleService;
        private readonly IGpsService _gpsService;
        
        // private readonly IAuthService _authService;

        public SessionManagerService(/*IScheduleService scheduleService,*/ IGpsService gpsService)
        {
            //_scheduleService = scheduleService;
            _gpsService = gpsService;
        }

        public async Task CheckAndResumeGpsTrackingAsync(ObservableCollection<ScheduleDto> pendingEvents)
        {


            // If the service is already running, we don't need to do anything.
            if (_gpsService.IsTracking)
            {
                Debug.WriteLine("SessionManager: GPS tracking is already active.");
                return;
            }

#if IOS
    var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
    if (status != PermissionStatus.Granted)
    {
        // Request "When used" permission first
        var whenInUseStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (whenInUseStatus == PermissionStatus.Granted)
        {
            // Now request "Always" permission
            status = await Permissions.RequestAsync<Permissions.LocationAlways>();
        }
    }

    if (status != PermissionStatus.Granted)
    {
        Debug.WriteLine("SessionManager: 'Location Always' permission was not granted. Cannot start background tracking.");
        // Show an alert to the user.
        await Shell.Current.DisplayAlert("Permission Required", "For route tracking, please enable location permission to 'Always' in the app settings.", "OK");
        return;
    }
#endif

            Debug.WriteLine("SessionManager: Checking conditions to resume GPS tracking...");

            try
            {
                
                var todaySchedule = pendingEvents;//await _scheduleService.GetSchedulesAsync(); 
                if (todaySchedule == null || !todaySchedule.Any())
                {
                    Debug.WriteLine("SessionManager: No schedule found for today.");
                    return;
                }

                // Paso 3: Encontrar los eventos Pull-out y Pull-in.
                var pullOutEvent = todaySchedule.FirstOrDefault(e => e.Name == "Pull-out");
                var pullInEvent = todaySchedule.FirstOrDefault(e => e.Name == "Pull-in");

                /*if (pullOutEvent == null || pullInEvent == null)
                {
                    Debug.WriteLine("SessionManager: Pull-out or Pull-in event not found in today's schedule.");
                    return;
                }*/

               
                //bool hasPulledOut = pullOutEvent.Performed;
                bool hasPulledIn = pullInEvent.Performed;

                if (pullOutEvent == null && !hasPulledIn)
                {
                    
                    Debug.WriteLine($"SessionManager: Conditions met. Starting GPS tracking for VehicleRouteId: {pullOutEvent.VehicleRouteId}");
                    _gpsService.StartTracking(pullOutEvent.VehicleRouteId);
                }
                else
                {
                    Debug.WriteLine("SessionManager: Conditions not met. GPS tracking will not be started.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SessionManager: An error occurred while checking GPS status. {ex.Message}");
            }
        }
    }
}
