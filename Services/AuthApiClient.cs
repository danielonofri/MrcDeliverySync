using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MrcDeliverySync.Models;

namespace MrcDeliverySync.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 1. Validar OwnerApiKey
        public async Task<ValidateKeyResponse?> ValidateKeyAsync(string apiKey)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/owner/auth/validate-key", new { OwnerApiKey = apiKey });
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidateKeyResponse>();
            }
            return null;
        }

        public async Task<LoginResponse?> LoginAsync(string licencia, string username, string password, string ptoVta, string apiKey)
        {
            try
            {
                var payload = new
                {
                    Licencia = licencia,
                    Username = username,
                    Password = password,
                    PtvoVta = ptoVta,
                    OwnerApiKey = apiKey
                };

                // Agregar cabecera para saltear el aviso de advertencia de DevTunnels/ngrok
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/owner/auth/login");
                request.Headers.Add("X-Handshake-Bypass-Header", "true");
                request.Content = JsonContent.Create(payload);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LoginResponse>();
                }

                // Si devuelve 401, 400 o 500, leemos el mensaje exacto para depurar
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"⚠️ Auth API Error ({response.StatusCode}): {errorBody}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Excepción al conectar con Auth API: {ex.Message}");
                return null;
            }
        }

    }

    public class ValidateKeyResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Licencia { get; set; } = string.Empty;
        public string PtoVta { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int Rol { get; set; }
    }
}