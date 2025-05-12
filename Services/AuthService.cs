using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Mercurio.Driver.Exceptions;
using Mercurio.Driver.Models;

namespace Mercurio.Driver.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        //private string URI = App.Configuration["ApiAddress:ApiTest"];

        public AuthService()
        {
            //var baseUrl = Preferences.Get("ApiBaseUrl", string.Empty);
            var baseUrl = "http://cketiel-001-site1.ntempurl.com/";

            if (string.IsNullOrEmpty(baseUrl))
            {
                //ErrorMessage = "API URL is not configured.";
                return;
            }

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    //throw await CreateApiException(response, "Authentication error");
                    return new LoginResponse { IsSuccess = false, Message = "Invalid login" };
                }

                return await response.Content.ReadFromJsonAsync<LoginResponse>();               
            }
            catch (HttpRequestException ex)
            {
                throw new ApiException("Server connection error", ex);
            }
            
        }

        private async Task<ApiException> CreateApiException(HttpResponseMessage response, string context)
        {
            try
            {
                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                return new ApiException(
                    message: $"{context}: {problemDetails?.Title}",
                    statusCode: response.StatusCode,
                    details: problemDetails?.Detail);
            }
            catch
            {
                var content = await response.Content.ReadAsStringAsync();
                return new ApiException(
                    message: $"{context}: Error no especificado",
                    statusCode: response.StatusCode,
                    details: content);
            }
        }
    }
}
