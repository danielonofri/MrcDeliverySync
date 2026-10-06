namespace MrcDeliverySync.Models
{
    public class ArticuloPeyaDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descrip { get; set; } = string.Empty;
        public string Codigoerp { get; set; } = string.Empty;
        public bool? Sinstock { get; set; }
    }
}
