using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MrcDeliverySync.Services
{
    public class AuthVaultService : IAuthVaultService
    {
        private readonly ProtectedLocalStorage _localStorage;

        public AuthVaultService(ProtectedLocalStorage localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SaveOwnerApiKeyAsync(string apiKey)
        {
            await _localStorage.SetAsync("mrc_owner_api_key", apiKey);
        }

        public async Task<string?> GetOwnerApiKeyAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<string>("mrc_owner_api_key");
                return result.Success ? result.Value : null;
            }
            catch (CryptographicException)
            {
                // Si la clave de DataProtection del servidor cambió, borra la clave corrupta
                await _localStorage.DeleteAsync("mrc_owner_api_key");
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveAuthTokenAsync(string token, int rol, string licencia)
        {
            await _localStorage.SetAsync("mrc_jwt_token", token);
            await _localStorage.SetAsync("mrc_user_role", rol);
            await _localStorage.SetAsync("mrc_licencia", licencia);
        }

        public async Task<string?> GetAuthTokenAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<string>("mrc_jwt_token");
                return result.Success ? result.Value : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> GetUserRoleAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<int>("mrc_user_role");
                return result.Success ? result.Value : 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task ClearSessionAsync()
        {
            try
            {
                await _localStorage.DeleteAsync("mrc_jwt_token");
                await _localStorage.DeleteAsync("mrc_user_role");
            }
            catch
            {
                // Manejo silencioso en caso de desconexión
            }
        }
    }
}