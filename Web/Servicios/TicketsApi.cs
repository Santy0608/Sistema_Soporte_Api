using System.Net;
using System.Text.Json;
using Web.Models;

namespace Web.Servicios;

public class TicketsApi : ITicketsApi
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    // El HttpClient llega ya configurado (BaseAddress, Timeout y el handler del token) porque se registra
    // con AddHttpClient<ITicketsApi, TicketsApi> en Program.cs. No se crea a mano nunca.
    public TicketsApi(HttpClient http) => _http = http;

    public async Task<ListaTicketsVm> ListarAsync(string? estado, string? q, int pagina, int tamano, CancellationToken ct = default)
    {
        var partes = new List<string> { $"pagina={pagina}", $"tamano={tamano}" };
        if (!string.IsNullOrWhiteSpace(estado)) partes.Add($"estado={Uri.EscapeDataString(estado)}");
        if (!string.IsNullOrWhiteSpace(q)) partes.Add($"q={Uri.EscapeDataString(q.Trim())}");

        using var respuesta = await EnviarAsync(() => _http.GetAsync("api/tickets?" + string.Join("&", partes), ct), ct);
        await ApiException.LanzarSiFallaAsync(respuesta, ct);

        var items = await respuesta.Content.ReadFromJsonAsync<List<TicketVm>>(OpcionesJson, ct) ?? new();

        // El total real viene en un header, no en el cuerpo.
        var total = items.Count;
        if (respuesta.Headers.TryGetValues("X-Total-Count", out var valores)
            && int.TryParse(valores.FirstOrDefault(), out var totalHeader))
            total = totalHeader;

        return new ListaTicketsVm { Items = items, Estado = estado, Q = q, Pagina = pagina, Tamano = tamano, Total = total };
    }

    public async Task<TicketVm?> ObtenerAsync(int id, CancellationToken ct = default)
    {
        using var respuesta = await EnviarAsync(() => _http.GetAsync($"api/tickets/{id}", ct), ct);
        if (respuesta.StatusCode == HttpStatusCode.NotFound) return null;   // "no existe" es una respuesta normal

        await ApiException.LanzarSiFallaAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<TicketVm>(OpcionesJson, ct);
    }

    public async Task<TicketVm> CrearAsync(TicketCrearVm datos, CancellationToken ct = default)
    {
        // Se manda solo lo que el API espera. Prioridad viaja como texto ("Media"): el API acepta enums como string.
        var cuerpo = new { datos.Titulo, datos.Descripcion, datos.Prioridad, datos.ClienteId };
        using var respuesta = await EnviarAsync(() => _http.PostAsJsonAsync("api/tickets", cuerpo, OpcionesJson, ct), ct);
        await ApiException.LanzarSiFallaAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<TicketVm>(OpcionesJson, ct))!;
    }

    public async Task CambiarEstadoAsync(int id, string estado, int? agenteId, CancellationToken ct = default)
    {
        // PATCH con HttpRequestMessage: funciona en cualquier versión de .NET.
        using var peticion = new HttpRequestMessage(HttpMethod.Patch, $"api/tickets/{id}/estado")
        {
            Content = JsonContent.Create(new { estado, agenteId }, options: OpcionesJson)
        };
        using var respuesta = await EnviarAsync(() => _http.SendAsync(peticion, ct), ct);
        await ApiException.LanzarSiFallaAsync(respuesta, ct);
    }

    public async Task<byte[]> ExportarCsvAsync(string? estado, string? q, CancellationToken ct = default)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(estado)) partes.Add($"estado={Uri.EscapeDataString(estado)}");
        if (!string.IsNullOrWhiteSpace(q)) partes.Add($"q={Uri.EscapeDataString(q.Trim())}");

        using var respuesta = await EnviarAsync(() => _http.GetAsync("api/tickets/exportar?" + string.Join("&", partes), ct), ct);
        await ApiException.LanzarSiFallaAsync(respuesta, ct);
        return await respuesta.Content.ReadAsByteArrayAsync(ct);
    }

    // Un solo lugar donde los fallos de red se traducen a nuestra excepción propia. Sirve para cualquier verbo HTTP.
    private static async Task<HttpResponseMessage> EnviarAsync(Func<Task<HttpResponseMessage>> peticion, CancellationToken ct)
    {
        try
        {
            return await peticion();
        }
        catch (HttpRequestException)
        {
            throw new ApiException(null, "No se pudo conectar con el API",
                "Verifique que el API esté corriendo y que Api:UrlBase sea correcta.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiException(null, "El API tardó demasiado",
                "Si usa Azure SQL gratis, la base pudo estar pausada. Intente de nuevo.");
        }
    }
}
