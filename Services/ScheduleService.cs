﻿

using System.Text;
using System.Text.Json;
using System.Web;
using Mercurio.Driver.DTOs;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        public ScheduleService()
        {
            // The base URL of your API. It should be in a centralized place, like Preferences or a config file.
            var baseUrl = Preferences.Get("ApiBaseUrl", "https://localhost:7244/");

            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Important to match property names
            };
        }

        public async Task<List<ScheduleDto>> GetSchedulesByRunAsync(string runLogin, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(runLogin))
                return new List<ScheduleDto>(); 

            var dateString = date.ToString("yyyy-MM-dd");
            dateString = "2025-04-30"; // esto es para probar
            var encodedRunLogin = HttpUtility.UrlEncode(runLogin);

            var requestUri = $"api/Schedules/by-run-login?runLogin={encodedRunLogin}&date={dateString}";

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var schedules = JsonSerializer.Deserialize<List<ScheduleDto>>(content, _serializerOptions);
                    return schedules ?? new List<ScheduleDto>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching schedule: {response.StatusCode}");
                    return new List<ScheduleDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetSchedulesByRunAsync: {ex.Message}");              
                throw;
            }
        }

        public async Task<bool> UpdateScheduleAsync(ScheduleDto scheduleToUpdate)
        {
            if (scheduleToUpdate == null)
                return false;

            // Prepares JSON content to send in the request body
            var jsonContent = JsonSerializer.Serialize(scheduleToUpdate, _serializerOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
         
            var requestUri = $"api/Schedules/{scheduleToUpdate.Id}";

            try
            {
                // The HTTP PUT verb is used, which is the standard for full updates to a resource
                var response = await _httpClient.PutAsync(requestUri, content);

                if (!response.IsSuccessStatusCode)
                {                   
                    var errorBody = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error updating schedule: {response.StatusCode}. Body: {errorBody}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdateScheduleAsync: {ex.Message}");
                return false;
            }
        }

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
