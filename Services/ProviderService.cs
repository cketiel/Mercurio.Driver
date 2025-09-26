using Mercurio.Driver.DTOs;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Mercurio.Driver.Services
{
    public class ProviderService : IProviderService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;

        public ProviderService()
        {           
            var baseUrl = Preferences.Get("ApiBaseUrl", "https://krasnovbw-001-site1.rtempurl.com/");
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<ProviderDto?> GetContactProviderAsync()
        {
            var requestUri = "api/Providers/contact";
            try
            {
                var response = await _httpClient.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ProviderDto>(content, _serializerOptions);
                }
                else
                {
                    Debug.WriteLine($"Error fetching provider info: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetContactProviderAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateContactProviderAsync(ProviderDto provider)
        {
            var requestUri = "api/Providers/contact";
            var jsonContent = JsonSerializer.Serialize(provider, _serializerOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync(requestUri, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in UpdateContactProviderAsync: {ex.Message}");
                return false;
            }
        }
    }
}