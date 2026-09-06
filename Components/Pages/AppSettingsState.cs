namespace MrcDeliverySync.Services
{
    public class AppSettingsState
    {
        public string PwaHost { get; set; } = Environment.MachineName;
        public string ApiHost { get; set; } = "Desconocido";
        public string SqlServer { get; set; } = "Desconocido";

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