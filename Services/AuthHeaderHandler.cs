using System.Net.Http.Headers;
using System.Net;

namespace Mercurio.Driver.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Obtener el token de las preferencias
        var token = Preferences.Get("AuthToken", string.Empty);

        // 2. Si existe el token, agregarlo a la cabecera Authorization
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // 3. Ejecutar la petición original
        var response = await base.SendAsync(request, cancellationToken);

        // 4. Si el servidor responde 401 (No autorizado/Expirado)
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Limpiar datos de sesión
            Preferences.Remove("AuthToken");

            // Forzar el regreso al Login en el hilo principal
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Solo redirigir si no estamos ya en el Login
                if (Shell.Current.CurrentPage is not Views.LoginPage)
                {
                    await Shell.Current.GoToAsync("///LoginPage");
                }
            });
        }

        return response;
    }
}