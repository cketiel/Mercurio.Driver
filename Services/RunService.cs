using System.Net.Http.Json;
using System.Diagnostics;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Services
{
    public class RunService : IRunService
    {
        private readonly HttpClient _httpClient;

        public RunService()
        {
            var baseUrl = Preferences.Get("ApiBaseUrl", "https://krasnovbw-001-site1.rtempurl.com/");
            baseUrl = "https://krasnovbw-001-site1.rtempurl.com/";

            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<VehicleRoute> GetActiveRunByDriverIdAsync(int driverId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/runs/driver/{driverId}");
                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return null;
                    }
                    return await response.Content.ReadFromJsonAsync<VehicleRoute>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching active run: {ex.Message}");
            }
            return null;
        }
    }
}
