using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using MrcDeliverySync.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using MrcDeliverySync.Services;

namespace MrcDeliverySync.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppSettingsState _appState;
        private readonly string _connectionString;
        private readonly HttpClient _http;
        private readonly IAuthVaultService _vaultService;
        private readonly ILogger<OrderRepository> _logger;
        public OrderRepository(IConfiguration configuration,
                                AppSettingsState appState,
                                HttpClient http,
                                IAuthVaultService vaultService,
                                ILogger<OrderRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new System.InvalidOperationException("Cadena de conexión 'DefaultConnection' no configurada.");
            _http = http;
            _vaultService = vaultService;
            _logger = logger;
            _appState = appState;
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetActiveOrdersConsolidatedAsyncDb(string sucursalId)
        {
            using var db = new SqlConnection(_connectionString);

            string sql = @"
                SELECT 
                    Id,
                    OrderId_PY,
                    Code,
                    RemoteId,
                    TRIM(Status) AS Status,
                    TotalAmount,
                    CustomerName,
                    CustomerPhone,
                    FechaInsercion,
                    ShortCode,
                    ExpeditionType,
                    SucursalId,
                    1 AS Operator
                FROM dbo.PedidosYa_Orders WITH (NOLOCK) 
                WHERE Status NOT IN ('DELIVERED', 'REJECTED', 'CANCELLED')
                  AND (@sucursalId = '' OR SucursalId = @sucursalId)";

            return await db.QueryAsync<OrderSummaryDto>(sql, new { sucursalId });
        }
        public async Task<OrderDetailDto?> GetOrderDetailAsync(int idOrder, DeliveryOperator deliveryOperator)
        {
            using var db = new SqlConnection(_connectionString);

            // 1. Obtener la cabecera del pedido por su Id interno
            string orderSql = @"SELECT * FROM dbo.PedidosYa_Orders WITH (NOLOCK) WHERE Id = @idOrder";
            var order = await db.QueryFirstOrDefaultAsync<OrderDetailDto>(orderSql, new { idOrder });

            if (order == null) return null;

            // 2. Obtener los ítems uniendo por OrderId_PY
            string itemsSql = @"
                                SELECT 
                                    Id,
                                    OrderId_PY,
                                    IntegrationCode,
                                    Quantity,
                                    UnitPrice,
                                    ProductName,
                                    Discount,
                                    type,
                                    Preferencias,
                                    ItemNo,
                                    Parent
                                FROM dbo.PedidosYa_OrderItems WITH (NOLOCK)
                                WHERE OrderId_PY = @OrderId_PY";

            var items = await db.QueryAsync<OrderItemDto>(itemsSql, new { OrderId_PY = order.OrderId_PY });
            order.Items = items.ToList();

            return order;
        }
        public async Task<bool> UpdateOrderStatusAsync(string orderCode, string newStatus, DeliveryOperator deliveryOperator)
        {
            try
            {
                // Ruta relativa según la dirección base de appsettings.json
                string endpoint = $"api/v1/pos-order-bridge/order/delivered/{orderCode}?useQueue=false";

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

                // Adjuntamos el Token Bearer desde IAuthVaultService[cite: 2]
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ [DELIVERED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return false;
                }

                _logger.LogInformation($"✅ [DELIVERED OK] Orden #{orderCode} marcada como DELIVERED.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [DELIVERED EXCEPTION] Error al procesar entrega de orden #{orderCode}: {ex.Message}");
                return false;
            }
        }
        public async Task<IEnumerable<RobotStatusDto>> GetRobotsStatusAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/v1/owner/auth/robots-status");
                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [ROBOTS FAIL] Status: {response.StatusCode} | Body: {errorContent}");

                    return new List<RobotStatusDto>
                                {
                                    new RobotStatusDto
                                    {
                                        IdRobot = "SERVER",
                                        Estado = "OFFLINE",
                                        UltimoMensaje = $"Error API ({(int)response.StatusCode})",
                                        UltimoLatido = DateTime.MinValue
                                    }
                                };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // 1. Deserializamos y convertimos a List para poder evaluar los elementos
                var rawList = await response.Content.ReadFromJsonAsync<IEnumerable<RobotStatusDto>>(options);
                var resultList = rawList?.ToList() ?? new List<RobotStatusDto>();

                // 2. Extraemos el SqlServerName del primer DTO que lo traiga y actualizamos el AppState
                var robotConServer = resultList.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.SqlServerName));
                if (robotConServer != null)
                {
                    _appState.SqlServer = robotConServer.SqlServerName;
                }

                return resultList;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"⚠️ ERROR AL OBTENER ROBOTS: {ex.Message}");

                return new List<RobotStatusDto>
                        {
                            new RobotStatusDto
                            {
                                IdRobot = "SERVER",
                                Estado = "OFFLINE",
                                UltimoMensaje = "Sin conexión con la API",
                                UltimoLatido = DateTime.MinValue
                            }
                        };
            }
        }
        public async Task<IEnumerable<RobotStatusDto>> GetRobotsStatusAsyncDb()
        {
            using var db = new SqlConnection(_connectionString);
            string sql = @"SELECT IdRobot, UltimoLatido, Estado, UltimoMensaje 
                           FROM dbo.Robot_Status WITH (NOLOCK)";
            return await db.QueryAsync<RobotStatusDto>(sql);
        }
        public async Task<IEnumerable<RobotStatusDto>> GetRobotsStatusAsyncOld()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Usa la BaseAddress del appsettings automáticamente
                var result = await _http.GetFromJsonAsync<IEnumerable<RobotStatusDto>>(
                    "api/v1/owner/auth/robots-status",
                    options
                );

                return result ?? new List<RobotStatusDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"⚠️ ERROR AL OBTENER ROBOTS: {ex.Message}" + Environment.NewLine + _http.BaseAddress + "api/v1/owner/auth/robots-status");
                Console.WriteLine($"⚠️ ERROR AL OBTENER ROBOTS: {ex.Message}" + Environment.NewLine + _http.BaseAddress + "api/v1/owner/auth/robots-status");
                return new List<RobotStatusDto>();
            }
        }
        public async Task<bool> AcceptOrderAsync(string orderCode, int? preparationMinutes = null)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/v1/pos-order-bridge/order/accept/{orderCode}");

                // Leemos el token de la sesión activa
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("🔑 [AUTH] Token Bearer adjuntado a la petición.");
                }
                else
                {
                    Console.WriteLine("⚠️ [AUTH WARN] GetAuthTokenAsync() devolvió nulo o vacío.");
                }

                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [ACCEPTED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    Console.WriteLine($"❌ [ACCEPTED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return false;
                }

                _logger.LogDebug($"✅ [ACCEPTED OK] Orden #{orderCode} marcada como ACCEPTED.");
                // Console.WriteLine($"✅ [ACCEPTED OK] Orden #{code} marcada como ACCEPTED.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [ACCEPTED EXCEPTION]: {ex.Message}");
                // Console.WriteLine($"❌ [PREPARED EXCEPTION]: {ex.Message}"); 
                return false;
            }

            //try
            //{
            //    var url = $"api/v1/pos-order-bridge/order/accept/{orderCode}";
            //    if (preparationMinutes.HasValue && preparationMinutes.Value > 0)
            //    {
            //        url += $"?preparationMinutes={preparationMinutes.Value}";
            //    }

            //    var response = await _http.PostAsync(url, null);
            //    return response.IsSuccessStatusCode;
            //}
            //catch
            //{
            //    return false;
            //}
        }
        public async Task<bool> RejectOrderAsync(string orderCode)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/v1/pos-order-bridge/order/reject/{orderCode}");

                // Leemos el token de la sesión activa
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("🔑 [AUTH] Token Bearer adjuntado a la petición.");
                }
                else
                {
                    Console.WriteLine("⚠️ [AUTH WARN] GetAuthTokenAsync() devolvió nulo o vacío.");
                }

                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [REJECTED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    Console.WriteLine($"❌ [REJECTED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return false;
                }

                _logger.LogDebug($"✅ [REJECTED OK] Orden #{orderCode} marcada como REJECTED.");
                // Console.WriteLine($"✅ [PREPARED OK] Orden #{code} marcada como REJECTED.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [REJECTED EXCEPTION]: {ex.Message}");
                // Console.WriteLine($"❌ [PREPARED EXCEPTION]: {ex.Message}"); 
                return false;
            }

            //try
            //{
            //    var response = await _http.PostAsync($"api/v1/pos-order-bridge/order/reject/{orderCode}", null);
            //    return response.IsSuccessStatusCode;
            //}
            //catch
            //{
            //    return false;
            //}
        }
        public async Task<bool> MarkAsPreparedAsyncX(string orderCode)
        {
            try
            {
                var response = await _http.PostAsync($"api/v1/pos-order-bridge/order/prepared/{orderCode}", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> MarkAsPreparedAsync(string code)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/v1/pos-order-bridge/order/prepared/{code}");

                // Leemos el token de la sesión activa
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Console.WriteLine("🔑 [AUTH] Token Bearer adjuntado a la petición.");
                    _logger.LogDebug("🔑 [AUTH] Token Bearer adjuntado a la petición.");
                }
                else
                {
                    Console.WriteLine("⚠️ [AUTH WARN] GetAuthTokenAsync() devolvió nulo o vacío.");
                    _logger.LogWarning("⚠️ [AUTH WARN] GetAuthTokenAsync() devolvió nulo o vacío.");
                }

                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ [PREPARED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    Console.WriteLine($"❌ [PREPARED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return false;
                }

                _logger.LogError($"✅ [PREPARED OK] Orden #{code} marcada como PREPARED.");
                // Console.WriteLine($"✅ [PREPARED OK] Orden #{code} marcada como PREPARED.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [PREPARED EXCEPTION]: {ex.Message}");
                // Console.WriteLine($"❌ [PREPARED EXCEPTION]: {ex.Message}"); 
                return false;
            }
        }
        public async Task<bool> MarkAsPreparedAsyncx(string code)
        {
            try
            {
                var response = await _http.PostAsync($"api/v1/pos-order-bridge/order/prepared/{code}", null);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [PREPARED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    // Console.WriteLine($"❌ [PREPARED FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return false;
                }

                _logger.LogDebug($"✅ [PREPARED OK] Orden #{code} marcada como PREPARED.");
                // Console.WriteLine($"✅ [PREPARED OK] Orden #{code} marcada como PREPARED.");     
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [PREPARED EXCEPTION]: {ex.Message}");
                Console.WriteLine($"❌ [PREPARED EXCEPTION]: {ex.Message}");
                return false;
            }
        }
        #region Nuevos Metodos para obtener pedidos activos consolidados desde la API
        public async Task<IEnumerable<OrderSummaryDto>> GetActiveOrdersConsolidatedAsyncDeprecated(string sucursalId)
        {
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/v1/owner/orders/active-consolidated");

                // Adjuntamos el Token Bearer del usuario activo
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ [ACTIVE ORDERS FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return new List<OrderSummaryDto>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = await response.Content.ReadFromJsonAsync<IEnumerable<OrderSummaryDto>>(options);
                return result ?? new List<OrderSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [ACTIVE ORDERS EXCEPTION]: {ex.Message}");
                return new List<OrderSummaryDto>();
            }
        }
        public async Task<IEnumerable<OrderSummaryDto>> GetActiveOrdersConsolidatedAsync(string sucursalId)
        {
            var consolidatedList = new List<OrderSummaryDto>();

            try
            {
                // 1. Crear las tareas en paralelo para cada operador
                var pedidosYaTask = GetVigentesPedidosYaAsync();
                // var uberEatsTask = GetVigentesUberEatsAsync(); // Futuro
                // var rappiTask = GetVigentesRappiAsync();       // Futuro

                // 2. Esperar a que todas las APIs respondan
                await Task.WhenAll(pedidosYaTask /*, uberEatsTask, rappiTask */);

                // 3. Unir los resultados normalizados
                if (pedidosYaTask.Result != null)
                {
                    consolidatedList.AddRange(pedidosYaTask.Result);
                }

                // if (uberEatsTask.Result != null) consolidatedList.AddRange(uberEatsTask.Result);
                // if (rappiTask.Result != null) consolidatedList.AddRange(rappiTask.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [CONSOLIDATED EXCEPTION]: {ex.Message}");
            }

            return consolidatedList;
        }
        public async Task<IEnumerable<OrderSummaryDto>> GetVigentesPedidosYaAsync()
        {
            try
            {
                // Ruta relativa usando el BaseUrl configurado en Program.cs
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/v1/pos-order-bridge/order/pedidosya/getVigentes");

                // Adjuntamos el token Bearer desde el Vault
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [GET VIGENTES FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return new List<OrderSummaryDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<OrderSummaryDto>>();
                return result ?? new List<OrderSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [GET VIGENTES EXCEPTION]: {ex.Message}");
                return new List<OrderSummaryDto>();
            }
        }
        public async Task<IEnumerable<OrderSummaryDto>> GetVigentesUberEatsAsync()
        {
            try
            {
                // Ruta relativa usando el BaseUrl configurado en Program.cs
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/v1/pos-order-bridge/order/ubereats/getVigentes");

                // Adjuntamos el token Bearer desde el Vault
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [GET VIGENTES FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return new List<OrderSummaryDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<OrderSummaryDto>>();
                return result ?? new List<OrderSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [GET VIGENTES EXCEPTION]: {ex.Message}");
                return new List<OrderSummaryDto>();
            }
        }
        public async Task<IEnumerable<OrderSummaryDto>> GetVigentesRappiAsync()
        {
            try
            {
                // Ruta relativa usando el BaseUrl configurado en Program.cs
                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/v1/pos-order-bridge/order/rappi/getVigentes");

                // Adjuntamos el token Bearer desde el Vault
                var token = await _vaultService.GetAuthTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"❌ [GET VIGENTES FAIL] Status: {response.StatusCode} | Body: {errorContent}");
                    return new List<OrderSummaryDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<OrderSummaryDto>>();
                return result ?? new List<OrderSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ [GET VIGENTES EXCEPTION]: {ex.Message}");
                return new List<OrderSummaryDto>();
            }
        }
        #endregion Nuevos Metodos para obtener pedidos activos consolidados desde la API
    }
}