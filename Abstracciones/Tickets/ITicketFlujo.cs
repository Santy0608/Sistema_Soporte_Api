using Abstracciones.Comunes;

namespace Abstracciones.Tickets;

public interface ITicketFlujo
{
    Task<ResultadoPaginado<TicketDto>> ListarAsync(TicketFiltro filtro, CancellationToken ct);
    Task<TicketDto> ObtenerAsync(int id, CancellationToken ct);
    Task<TicketDto> ObtenerPorCodigoAsync(string codigo, CancellationToken ct);
    Task<TicketDto> CrearAsync(TicketCrearDto dto, CancellationToken ct);
    Task<TicketDto> CambiarEstadoAsync(int id, TicketCambiarEstadoDto dto, CancellationToken ct);
    Task<TicketDto> ReabrirAsync(int id, CancellationToken ct);
    Task<TicketResumenDto> ResumenAsync(CancellationToken ct);
    Task AsignarEtiquetaAsync(int ticketId, int etiquetaId, CancellationToken ct);
    Task QuitarEtiquetaAsync(int ticketId, int etiquetaId, CancellationToken ct);

    /// <summary>Genera el CSV (UTF-8 con BOM, para que Excel respete las tildes) con los tickets que cumplan el filtro.</summary>
    Task<byte[]> ExportarCsvAsync(TicketFiltro filtro, CancellationToken ct);
}
