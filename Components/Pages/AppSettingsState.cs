using Microsoft.AspNetCore.Components;

namespace MrcDeliverySync.Services
{
    public class AppSettingsState
    {
        public string PwaHost { get; set; } = "Desconocido";
        public string ApiHost { get; set; } = "Desconocido";
        public string SqlServer { get; set; } = "Desconocido";
        public void SetPwaUrl(string rawUrl)
        {
            if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
            {
                // Opciones disponibles:
                // uri.Host      -> "midominio.com"
                // uri.Authority -> "midominio.com:5001" (incluye puerto si lo hay)
                // uri.ToString() -> "https://midominio.com/"

                PwaHost = uri.Authority;
            }
            else
            {
                PwaHost = rawUrl;
            }
        }
        public void SetApiUrl(string rawUrl)
        {
            if (Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
            {
                ApiHost = uri.Host;
            }
            else
            {
                ApiHost = rawUrl;
            }
        }
    }
}