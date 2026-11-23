using System.Text.Json;
using Web.Models;

namespace Web.Servicios;

public class CuentaApi : ICuentaApi
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public CuentaApi(HttpClient http) => _http = http;

    public async Task<SesionApi> IniciarSesionAsync(LoginVm datos, CancellationToken ct = default)
    {
        try
        {
            // El API espera { correo, password } (LoginDto).
            using var respuesta = await _http.PostAsJsonAsync("api/auth/login",
                new { correo = datos.Correo, password = datos.Password }, OpcionesJson, ct);

            // Un 401 llega como ApiException con Codigo = 401.
            await ApiException.LanzarSiFallaAsync(respuesta, ct);
            return (await respuesta.Content.ReadFromJsonAsync<SesionApi>(OpcionesJson, ct))!;
        }
        catch (HttpRequestException)
        {
            throw new ApiException(null, "No se pudo conectar con el API",
                "Verifique que el API esté corriendo y que Api:UrlBase sea correcta.");
        }
    }
}
