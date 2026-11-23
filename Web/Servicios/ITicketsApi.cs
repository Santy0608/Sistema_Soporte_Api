using Web.Models;

namespace Web.Servicios;

/// <summary>
/// Lo que el MVC sabe hacer con los tickets del API. El controlador depende de esta interfaz (no de HttpClient):
/// así se puede cambiar el HTTP sin tocar controladores.
/// </summary>
public interface ITicketsApi
{
    Task<ListaTicketsVm> ListarAsync(string? estado, string? q, int pagina, int tamano, CancellationToken ct = default);

    /// <summary>Devuelve null si el ticket no existe (404). Cualquier otro error lanza ApiException.</summary>
    Task<TicketVm?> ObtenerAsync(int id, CancellationToken ct = default);

    Task<TicketVm> CrearAsync(TicketCrearVm datos, CancellationToken ct = default);
    Task CambiarEstadoAsync(int id, string estado, int? agenteId, CancellationToken ct = default);
    Task<byte[]> ExportarCsvAsync(string? estado, string? q, CancellationToken ct = default);
}
