using MrcDeliverySync.Repositories;
using MrcDeliverySync.Services;
using static MrcDeliverySync.Components.Store.StoreStatusDialog;
using static MrcDeliverySync.Components.Store.StoreStatusWidget;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MrcDeliverySync.Models;
namespace MrcDeliverySync.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly HttpClient _http;
        private readonly IAuthVaultService _vaultService;
        private readonly ILogger<StoreRepository> _logger;

        public StoreRepository(HttpClient http, IAuthVaultService vaultService, ILogger<StoreRepository> logger)
        {
            _http = http;
            _vaultService = vaultService;
            _logger = logger;
        }

        public async Task<List<StoreStatusDto>> GetStoreStatusAsync(string ownerApiKey)
        {                       
            try
            {
                string endpoint = $"api/v1/store/status?ownerApiKey={Uri.EscapeDataString(ownerApiKey)}";
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ [STORE STATUS FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<StoreStatusDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [STORE STATUS EXCEPTION] Error al obtener estado de tienda: {ex.Message}");
                return new();
            }
        }

        public async Task<List<ClosureReasonDto>> GetClosureReasonsAsync()
        {
            try
            {
                string endpoint = "api/v1/store/closure-reasons";
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ [CLOSURE REASONS FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<ClosureReasonDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [CLOSURE REASONS EXCEPTION] Error al obtener razones de cierre: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> UpdateStoreStatusAsync(string availabilityState, string? closedReason = null, int? closingMinutes = null)
        {
            try
            {
                string endpoint = $"api/v1/store/status/update?availabilityState={availabilityState}";

                if (availabilityState == "CLOSED_UNTIL")
                {
                    endpoint += $"&closedReason={closedReason}&closingMinutes={closingMinutes}";
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Content = new StringContent(string.Empty);

                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ [UPDATE STORE STATUS FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return false;
                }

                _logger.LogInformation($"✅ [UPDATE STORE STATUS OK] Estado de tienda actualizado a {availabilityState}.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [UPDATE STORE STATUS EXCEPTION] Error al actualizar estado de tienda: {ex.Message}");
                return false;
            }
        }
        public async Task<List<StoreStatusDto>> GetStoreStatusAsync()
        {
            try
            {
                // Obtenemos la OwnerApiKey directamente del Vault, igual que el token
                var ownerApiKey = await _vaultService.GetOwnerApiKeyAsync() ?? "{333}";

                string endpoint = $"api/v1/store/status?ownerApiKey={Uri.EscapeDataString(ownerApiKey)}";
                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<StoreStatusDto>>() ?? new();
            }
            catch
            {
                return new();
            }
        }
    }
}