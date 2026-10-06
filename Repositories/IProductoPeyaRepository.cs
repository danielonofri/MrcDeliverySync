using System.Collections.Generic;
using System.Threading.Tasks;
using MrcDeliverySync.Models;

namespace MrcDeliverySync.Repositories
{
    public interface IProductoPeyaRepository
    {
        Task<IEnumerable<ArticuloPeyaDto>> GetArticulosPeyaAsync();
        Task<bool> UpdateItemAvailabilityAsync(string codigo, bool isAvailable);
    }
}