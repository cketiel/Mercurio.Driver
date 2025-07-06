

using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Services
{
    public class ScheduleService : IScheduleService
    {
        public Task<List<ScheduleDto>> GetTodayScheduleAsync()
        {

            // Por ahora, usar datos de prueba.

            var mockSchedules = new List<ScheduleDto>
            {
                // Caso 1: Pickup Appointment (Verde)
                new ScheduleDto {
                    Id = 1, SpaceType = "AMB", FundingSource = "PRIVADO", ETA = new TimeSpan(12, 0, 0),
                    EventType = ScheduleEventType.Pickup, TripType = "Appointment", Patient = "test, system",
                    Pickup = new TimeSpan(12, 0, 0), Address = "405 SE VAN LOON TERRACE, CAPE CORAL, FL, 33990"
                },
                // Caso 2: Dropoff Appointment (Rojo)
                new ScheduleDto {
                    Id = 2, SpaceType = "AMB", FundingSource = "PRIVADO", ETA = new TimeSpan(12, 30, 0),
                    EventType = ScheduleEventType.Dropoff, TripType = "Appointment", Patient = "test, system",
                    Appt = new TimeSpan(12, 30, 0), Address = "525 SE VAN LOON TERRACE, CAPE CORAL, FL, 33990"
                },
                 // Caso 3: Pickup Return (Azul)
                new ScheduleDto {
                    Id = 3, SpaceType = "WCH", FundingSource = "MEDICAID", ETA = new TimeSpan(14, 0, 0),
                    EventType = ScheduleEventType.Pickup, TripType = "Return", Patient = "Doe, Jane",
                    Pickup = new TimeSpan(14, 0, 0), Address = "101 PINE AVE, MIAMI, FL, 33101"
                },
                // Caso 4: Dropoff Return (Morado)
                new ScheduleDto {
                    Id = 4, SpaceType = "WCH", FundingSource = "MEDICAID", ETA = new TimeSpan(14, 45, 0),
                    EventType = ScheduleEventType.Dropoff, TripType = "Return", Patient = "Doe, Jane",
                    Appt = new TimeSpan(14, 45, 0), Address = "202 OAK ST, MIAMI, FL, 33101"
                },
                // Caso 5: Pull-in (Negro)
                new ScheduleDto {
                    Id = 5, Name = "Pull-in", SpaceType = "VEHICLE", FundingSource = "INTERNAL", ETA = new TimeSpan(17, 0, 0),
                    EventType = null, TripType = "Internal", Patient = "Driver Return",
                    Pickup = new TimeSpan(17, 0, 0), Address = "COMPANY BASE, NAPLES, FL, 34101"
                }
            };

            return Task.FromResult(mockSchedules);
        }
    }
}
