using System.Collections.Generic;
using System.Threading.Tasks;
using MrcDeliverySync.Models;

namespace MrcDeliverySync.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummaryDto>> GetActiveOrdersConsolidatedAsync(string sucursalId);
        Task<IEnumerable<OrderSummaryDto>> GetVigentesPedidosYaAsync();
        Task<OrderDetailDto?> GetOrderDetailAsync(int idOrder, DeliveryOperator deliveryOperator);
        Task<bool> UpdateOrderStatusAsync(string orderCode, string newStatus, DeliveryOperator deliveryOperator);
        Task<IEnumerable<RobotStatusDto>> GetRobotsStatusAsync();
        Task<bool> AcceptOrderAsync(string orderCode, int? preparationMinutes = null);
        Task<bool> RejectOrderAsync(string orderCode);
        Task<bool> MarkAsPreparedAsync(string orderCode);
    }
}