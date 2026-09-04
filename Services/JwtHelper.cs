using System;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MrcDeliverySync.Services
{
    public static class JwtHelper
    {
        public static string GetUsernameFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return string.Empty;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Se agrega ClaimTypes.NameIdentifier y "nameidentifier" a la búsqueda
                var nameClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == ClaimTypes.Name ||
                    c.Type == "nameidentifier" ||
                    c.Type == "unique_name" ||
                    c.Type == "sub" ||
                    c.Type == "username" ||
                    c.Type == "name")?.Value;

                return nameClaim ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        // public static string GetUsernameFromToken(string jwtToken)
        // {
        //     if (string.IsNullOrWhiteSpace(jwtToken))
        //         return string.Empty;

        //     try
        //     {
        //         var parts = jwtToken.Split('.');
        //         if (parts.Length < 2) return string.Empty;

        //         var payload = parts[1];

        //         // Ajustar el rellenado Base64 si es necesario
        //         switch (payload.Length % 4)
        //         {
        //             case 2: payload += "=="; break;
        //             case 3: payload += "="; break;
        //         }

        //         var jsonBytes = Convert.FromBase64String(payload);
        //         using var doc = JsonDocument.Parse(jsonBytes);
        //         var root = doc.RootElement;

        //         // Buscar el claim del usuario por las claves habituales de JWT
        //         if (root.TryGetProperty("unique_name", out var uniqueName)) return uniqueName.GetString() ?? string.Empty;
        //         if (root.TryGetProperty("name", out var name)) return name.GetString() ?? string.Empty;
        //         if (root.TryGetProperty("sub", out var sub)) return sub.GetString() ?? string.Empty;

        //         return string.Empty;
        //     }
        //     catch
        //     {
        //         return string.Empty;
        //     }
        // }

        // public static string GetUsernameFromToken(string token)
        // {
        //     if (string.IsNullOrWhiteSpace(token)) return string.Empty;

        //     try
        //     {
        //         var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

        //         // Verifica si empieza con el cabezal JWT estándar
        //         if (!token.StartsWith("eyJ"))
        //         {
        //             Console.WriteLine("[JwtHelper] El token está cifrado o no es un JWT válido.");
        //             return string.Empty;
        //         }

        //         var jwtToken = handler.ReadJwtToken(token);

        //         // Busca en los Claims más comunes de ASP.NET Core
        //         var nameClaim = jwtToken.Claims.FirstOrDefault(c =>
        //             c.Type == System.Security.Claims.ClaimTypes.Name ||
        //             c.Type == "unique_name" ||
        //             c.Type == "sub" ||
        //             c.Type == "username" ||
        //             c.Type == "name")?.Value;

        //         return nameClaim ?? string.Empty;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"[JwtHelper] Error: {ex.Message}");
        //         return string.Empty;
        //     }
        // }
    }
}