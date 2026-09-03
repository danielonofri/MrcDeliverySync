using System.Threading.Tasks;

namespace MrcDeliverySync.Services
{
    public interface IAuthVaultService
    {
        Task SaveOwnerApiKeyAsync(string apiKey);
        Task<string?> GetOwnerApiKeyAsync();
        Task SaveAuthTokenAsync(string token, int rol, string licencia);
        Task<string?> GetAuthTokenAsync();
        Task<int> GetUserRoleAsync();
        Task ClearSessionAsync(); // Para Logout
    }
}