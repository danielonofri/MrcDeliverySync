using System.Text.Json.Serialization;

namespace MrcDeliverySync.Models
{
    public class StoreStatusDto
    {
        [JsonPropertyName("platformKey")] public string PlatformKey { get; set; } = "PY";
        [JsonPropertyName("availabilityStates")] public List<string> AvailabilityStates { get; set; } = new();
        [JsonPropertyName("closingReasons")] public List<string> ClosingReasons { get; set; } = new();
        [JsonPropertyName("closingMinutes")] public List<int> ClosingMinutes { get; set; } = new();
        [JsonPropertyName("changeable")] public bool Changeable { get; set; }
        [JsonPropertyName("availabilityState")] public string AvailabilityState { get; set; } = string.Empty;
        [JsonPropertyName("platformRestaurantId")] public string PlatformRestaurantId { get; set; } = string.Empty;
        [JsonPropertyName("currentSlotEndAt")] public string? CurrentSlotEndAt { get; set; }
        [JsonPropertyName("closedUntil")] public string? ClosedUntil { get; set; } // <--- Agrega esta línea
        [JsonPropertyName("globalEntityId")] public string GlobalEntityId { get; set; } = string.Empty;
    }
    public class ClosureReasonDto
    {
        [JsonPropertyName("codigo")] public string Codigo { get; set; } = string.Empty;
        [JsonPropertyName("traduccion")] public string Traduccion { get; set; } = string.Empty;
    }

    public class StoreUpdateDto
    {
        public string availabilityState { get; set; } = "";
        public string closedReason { get; set; } = "";
        public int closingMinutes { get; set; } = 5;
    }

}