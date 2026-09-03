using System;
using System.Text.Json.Serialization;

public class RobotStatusDto
{
    [JsonPropertyName("IdRobot")]
    public string IdRobot { get; set; } = string.Empty;

    [JsonPropertyName("UltimoLatido")]
    public DateTime UltimoLatido { get; set; }

    [JsonPropertyName("Estado")]
    public string Estado { get; set; } = string.Empty;

    [JsonPropertyName("UltimoMensaje")]
    public string UltimoMensaje { get; set; } = string.Empty;

    [JsonPropertyName("ServerTime")]
    public DateTimeOffset ServerTime { get; set; }

    // Evaluación directa de UTC contra UTC (3 minutos de tolerancia)
    public bool IsOnline(int toleranceMinutes = 3)
    {
        var latidoUtc = DateTime.SpecifyKind(UltimoLatido, DateTimeKind.Utc);
        double diferenciaMinutos = (DateTime.UtcNow - latidoUtc).TotalMinutes;

        // Está Online si la diferencia entre la hora actual UTC y el latido en UTC es menor al límite
        return Math.Abs(diferenciaMinutos) <= toleranceMinutes;
    }

    // Propiedad helper que convierte el latido UTC a la Hora Local del navegador/cliente
    public DateTime UltimoLatidoLocal
    {
        get
        {
            var latidoUtc = DateTime.SpecifyKind(UltimoLatido, DateTimeKind.Utc);
            return latidoUtc.ToLocalTime();
        }
    }
}