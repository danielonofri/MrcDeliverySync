public class OrderItemDto
{
    public int Quantity { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Preferencias { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

public class OrderFullDto
{
    public int Id { get; set; }
    public string OrderId_PY { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string RemoteId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime FechaInsercion { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string ExpeditionType { get; set; } = string.Empty;
    public string? Order_GUID { get; set; }
    public string SucursalId { get; set; } = string.Empty;
    public int Operator { get; set; }
    public int MinutesElapsed { get; set; }

    // Propiedad añadida para incluir la lista de ítems de una sola vez
    public List<OrderItemDto> Items { get; set; } = new();

    public bool IsDelayed(int thresholdMinutes) => MinutesElapsed >= thresholdMinutes;
}