using System;
using System.Text.Json;

namespace MrcDeliverySync.Services
{
    public static class JwtHelper
    {
        public static string GetUsernameFromToken(string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
                return string.Empty;

            try
            {
                var parts = jwtToken.Split('.');
                if (parts.Length < 2) return string.Empty;

                var payload = parts[1];

                // Ajustar el rellenado Base64 si es necesario
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                using var doc = JsonDocument.Parse(jsonBytes);
                var root = doc.RootElement;

                // Buscar el claim del usuario por las claves habituales de JWT
                if (root.TryGetProperty("unique_name", out var uniqueName)) return uniqueName.GetString() ?? string.Empty;
                if (root.TryGetProperty("name", out var name)) return name.GetString() ?? string.Empty;
                if (root.TryGetProperty("sub", out var sub)) return sub.GetString() ?? string.Empty;

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}