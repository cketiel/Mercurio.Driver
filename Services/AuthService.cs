using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
            
            try
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
            catch (UriFormatException ex)
            {               
                System.Diagnostics.Debug.WriteLine($"Error setting BaseAddress: {ex.Message}");              
                throw new InvalidOperationException("The API base URL is invalid.", ex);
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (_httpClient.BaseAddress == null)
            {             
                // If BaseAddress could not be set in the constructor.
                throw new ApiException("The authentication service configuration is incorrect (invalid base URL).");
            }
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    //throw await CreateApiException(response, "Authentication error");
                    //throw await CreateApiException(response, $"Authentication error ({(int)response.StatusCode})");
                    return new LoginResponse { IsSuccess = false, Message = "Login Failed: Invalid credentials. Incorrect username or password." };
                }
                // Trying to deserialize, could fail if the JSON is not as expected.
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse == null)
                {
                    throw new ApiException("Unexpected response from the server after login.");
                }
                return loginResponse;

                //return await response.Content.ReadFromJsonAsync<LoginResponse>();               
            }             
            catch (HttpRequestException ex) // Network errors, DNS, server not available, etc.
            {             
                throw new ApiException("Server connection error. Check your internet connection.", ex);
            }
            catch (JsonException ex) // Error deserializing JSON response
            {
                throw new ApiException("Error processing server response.", ex);
            }
       
            catch (Exception ex)
            {
                throw new ApiException("An unexpected error occurred during login.", ex);
            }

        }

        private async Task<ApiException> CreateApiException(HttpResponseMessage response, string context)
        {
            try
            {
                // Try to read ProblemDetails if available (common in ASP.NET Core APIs)
                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                return new ApiException(
                    message: $"{context}: {problemDetails?.Title ?? "Unknown error"}",
                    statusCode: response.StatusCode,
                    details: problemDetails?.Detail);
            }
            catch (JsonException) // If the content is not ProblemDetails or is not JSON
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Limit the length of the errorContent so as not to show too much in the UI
                if (errorContent.Length > 200) errorContent = errorContent.Substring(0, 200) + "...";
                return new ApiException(
                    message: $"{context}",
                    statusCode: response.StatusCode,
                    details: string.IsNullOrWhiteSpace(errorContent) ? "The server did not provide additional details." : errorContent);
            }
            catch (Exception ex) // Another error processing the error response
            {
                return new ApiException(
                   message: $"{context}: Could not process the server error response.",
                   statusCode: response.StatusCode,
                   details: ex.Message);
            }           
        }
    }
}
