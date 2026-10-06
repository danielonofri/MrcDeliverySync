using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MrcDeliverySync.Models;
using MrcDeliverySync.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MrcDeliverySync.Repositories
{
    public class ProductoPeyaRepository : IProductoPeyaRepository
    {
        private readonly HttpClient _http;
        private readonly IAuthVaultService _vaultService;
        private readonly ILogger<ProductoPeyaRepository> _logger;

        public ProductoPeyaRepository(
            HttpClient http,
            IAuthVaultService vaultService,
            ILogger<ProductoPeyaRepository> logger)
        {
            _http = http;
            _vaultService = vaultService;
            _logger = logger;
        }

        public async Task<IEnumerable<ArticuloPeyaDto>> GetArticulosPeyaAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/v1/pos-order-bridge/articulos/peya");

                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [GET ARTICULOS PEYA FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return new List<ArticuloPeyaDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<ArticuloPeyaDto>>();
                return result ?? new List<ArticuloPeyaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [GET ARTICULOS PEYA EXCEPTION]: {ex.Message}");
                return new List<ArticuloPeyaDto>();
            }
        }

        public async Task<bool> UpdateItemAvailabilityAsync(string codigo, bool isAvailable)
        {
            try
            {
                string endpoint = "api/v1/pos-order-bridge/catalog/items/availability";
                using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);

                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var payload = new
                {
                    globalEntityId = "PY_AR",
                    items = new[] { codigo },
                    type = "ITEM",
                    isAvailable = isAvailable
                };

                request.Content = JsonContent.Create(payload);

                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ [AVAILABILITY FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return false;
                }

                _logger.LogInformation($"✅ [AVAILABILITY OK] Artículo {codigo} actualizado a isAvailable={isAvailable}.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [AVAILABILITY EXCEPTION] Error al actualizar artículo {codigo}: {ex.Message}");
                return false;
            }
        }
    }
}