using Abstracciones.Comunes;

namespace Abstracciones.Tickets;

/// <summary>
/// Contrato de datos de tickets. Hay DOS implementaciones intercambiables en DA:
/// TicketDAMemoria (Práctica 1, lista en memoria) y TicketDA (Entity Framework Core, Semana 8).
/// Flujo y API no cambian ni una línea al cambiar de una a otra: ese es el valor de las capas.
/// </summary>
public interface ITicketDA
{
    Task<ResultadoPaginado<Ticket>> BuscarAsync(TicketFiltro filtro, CancellationToken ct);

    /// <summary>Lectura para mostrar (sin seguimiento de cambios). Incluye cliente y etiquetas.</summary>
    Task<Ticket?> ObtenerPorIdAsync(int id, CancellationToken ct);

    Task<Ticket?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct);

    /// <summary>Lectura para modificar: la entidad queda rastreada y los cambios se guardan con GuardarCambiosAsync.</summary>
    Task<Ticket?> ObtenerParaActualizarAsync(int id, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);

    Task<int> ContarPorAnioAsync(int anio, CancellationToken ct);
    Task<Ticket> AgregarAsync(Ticket ticket, CancellationToken ct);

    Task<Dictionary<EstadoTicket, int>> ContarPorEstadoAsync(CancellationToken ct);
    Task<Dictionary<PrioridadTicket, int>> ContarPorPrioridadAsync(CancellationToken ct);

    /// <summary>Devuelve false si la etiqueta ya estaba asignada.</summary>
    Task<bool> AsignarEtiquetaAsync(int ticketId, int etiquetaId, DateTime fecha, CancellationToken ct);

    /// <summary>Devuelve false si la etiqueta no estaba asignada.</summary>
    Task<bool> QuitarEtiquetaAsync(int ticketId, int etiquetaId, DateTime fecha, CancellationToken ct);
}
