using System;
using System.Collections.Generic;

namespace MrcDeliverySync.Models
{
    public class OrderDetailDto : OrderSummaryDto
    {
        public string? UrlAccepted { get; set; }
        public string? UrlRejected { get; set; }
        public string? UrlPrepared { get; set; }
        public string? Token { get; set; }
        public string? RawJson { get; set; }

    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string IntegrationCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Preferencias { get; set; }
        public int ItemNo { get; set; }
        public int Parent { get; set; }
    }
}