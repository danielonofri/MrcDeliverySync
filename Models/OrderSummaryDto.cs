using System;

namespace MrcDeliverySync.Models
{
    public class OrderSummaryDto
    {
        public int Id { get; set; }
        public string OrderId_PY { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? RemoteId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime FechaInsercion { get; set; }
        public string? ShortCode { get; set; }
        public string? ExpeditionType { get; set; }
        public string SucursalId { get; set; } = string.Empty;
        public DeliveryOperator Operator { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public int MinutesElapsed
        {
            get
            {
                // Normalizar FechaInsercion a UTC según si viene especificada o no
                var utcFechaInsercion = FechaInsercion.Kind switch
                {
                    DateTimeKind.Utc => FechaInsercion,
                    DateTimeKind.Local => FechaInsercion.ToUniversalTime(),
                    _ => DateTime.SpecifyKind(FechaInsercion, DateTimeKind.Utc) // Fallback si viene de BD sin Kind
                };

                var minutes = (DateTime.UtcNow - utcFechaInsercion).TotalMinutes;
                return minutes > 0 ? (int)minutes : 0;
            }
        }
        public bool IsDelayed(int thresholdMinutes) => MinutesElapsed >= thresholdMinutes;
    }
}