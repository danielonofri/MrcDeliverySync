namespace MrcDeliverySync.Models
{
    public enum DeliveryOperator
    {
        PedidosYa = 1,
        UberEats = 2,
        Rappi = 3,
        MercadoPago = 4,
        DiDiFood = 5
    }

    public enum ViewMode
    {
        Cards,  // Layout de Fichas Elocuentes (Mobile / Kanban)
        Table   // Layout de Tabla Densa (Monitor Despacho)
    }
}