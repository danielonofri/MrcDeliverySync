using MrcDeliverySync.Models; // o donde ubiques tus DTOs
using static MrcDeliverySync.Components.Store.StoreStatusDialog;
using static MrcDeliverySync.Components.Store.StoreStatusWidget;
namespace MrcDeliverySync.Repositories
{
    public interface IStoreRepository
    {
        Task<List<StoreStatusDto>> GetStoreStatusAsync();
        Task<List<ClosureReasonDto>> GetClosureReasonsAsync();
        Task<bool> UpdateStoreStatusAsync(string availabilityState, string? closedReason = null, int? closingMinutes = null);
    }
}
