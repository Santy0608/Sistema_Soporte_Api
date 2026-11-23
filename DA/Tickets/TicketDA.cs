using Abstracciones.Comunes;
using Abstracciones.Etiquetas;
using Abstracciones.Excepciones;
using Abstracciones.Tickets;
using DA.Comunes;
using DA.Contexto;
using Microsoft.EntityFrameworkCore;

namespace DA.Tickets;

/// <summary>Implementación con Entity Framework Core (Semana 8).</summary>
public class TicketDA : ITicketDA
{
    private readonly SistemaSoporteContexto _ctx;

    public TicketDA(SistemaSoporteContexto ctx) => _ctx = ctx;

    public async Task<ResultadoPaginado<Ticket>> BuscarAsync(TicketFiltro filtro, CancellationToken ct)
    {
        var consulta = TicketConsultas.AplicarFiltros(_ctx.Tickets.AsNoTracking(), filtro);

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            // Contains se traduce a LIKE con los comodines escapados. En SQL Server la comparación
            // no distingue mayúsculas por la collation por defecto.
            var texto = filtro.Q.Trim();
            consulta = consulta.Where(t => t.Titulo.Contains(texto) || t.Descripcion.Contains(texto));
        }

        var total = await consulta.CountAsync(ct);

        var items = await TicketConsultas.Ordenar(consulta, filtro.Orden)
            .Include(t => t.Cliente)
            .Include(t => t.TicketEtiquetas).ThenInclude(te => te.Etiqueta)
            .Skip((filtro.Pagina - 1) * filtro.Tamano)
            .Take(filtro.Tamano)
            .ToListAsync(ct);

        return new ResultadoPaginado<Ticket>
        {
            Items = items,
            Total = total,
            Pagina = filtro.Pagina,
            Tamano = filtro.Tamano
        };
    }

    public Task<Ticket?> ObtenerPorIdAsync(int id, CancellationToken ct) =>
        ConDetalle().FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Ticket?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct) =>
        ConDetalle().FirstOrDefaultAsync(t => t.Codigo == codigo, ct);

    public Task<Ticket?> ObtenerParaActualizarAsync(int id, CancellationToken ct) =>
        _ctx.Tickets.FirstOrDefaultAsync(t => t.Id == id, ct); // CON seguimiento a propósito

    public Task GuardarCambiosAsync(CancellationToken ct) => _ctx.SaveChangesAsync(ct);

    public Task<int> ContarPorAnioAsync(int anio, CancellationToken ct) =>
        _ctx.Tickets.CountAsync(t => t.FechaCreacion.Year == anio, ct);

    public async Task<Ticket> AgregarAsync(Ticket ticket, CancellationToken ct)
    {
        _ctx.Tickets.Add(ticket);
        try
        {
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ErroresSql.EsDuplicado(ex))
        {
            // Dos tickets creados al mismo instante calcularon la misma secuencia.
            throw new ConflictoException("No se pudo generar un código único para el ticket. Intente de nuevo.");
        }
        return ticket;
    }

    public async Task<Dictionary<EstadoTicket, int>> ContarPorEstadoAsync(CancellationToken ct) =>
        await _ctx.Tickets.AsNoTracking()
            .GroupBy(t => t.Estado)
            .Select(g => new { Clave = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Clave, x => x.Cantidad, ct);

    public async Task<Dictionary<PrioridadTicket, int>> ContarPorPrioridadAsync(CancellationToken ct) =>
        await _ctx.Tickets.AsNoTracking()
            .GroupBy(t => t.Prioridad)
            .Select(g => new { Clave = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Clave, x => x.Cantidad, ct);

    public async Task<bool> AsignarEtiquetaAsync(int ticketId, int etiquetaId, DateTime fecha, CancellationToken ct)
    {
        var yaAsignada = await _ctx.TicketEtiquetas
            .AnyAsync(x => x.TicketId == ticketId && x.EtiquetaId == etiquetaId, ct);
        if (yaAsignada) return false;

        var ticket = await _ctx.Tickets.FirstAsync(t => t.Id == ticketId, ct);
        ticket.FechaActualizacion = fecha; // cambia el ETag del ticket

        _ctx.TicketEtiquetas.Add(new TicketEtiqueta
        {
            TicketId = ticketId,
            EtiquetaId = etiquetaId,
            FechaAsignacion = fecha
        });

        try
        {
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ErroresSql.EsDuplicado(ex))
        {
            return false; // otra petición la asignó justo antes
        }
        return true;
    }

    public async Task<bool> QuitarEtiquetaAsync(int ticketId, int etiquetaId, DateTime fecha, CancellationToken ct)
    {
        var asignacion = await _ctx.TicketEtiquetas
            .FirstOrDefaultAsync(x => x.TicketId == ticketId && x.EtiquetaId == etiquetaId, ct);
        if (asignacion is null) return false;

        var ticket = await _ctx.Tickets.FirstAsync(t => t.Id == ticketId, ct);
        ticket.FechaActualizacion = fecha;

        _ctx.TicketEtiquetas.Remove(asignacion);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    // Lectura con todo lo necesario para armar un TicketDto.
    private IQueryable<Ticket> ConDetalle() =>
        _ctx.Tickets.AsNoTracking()
            .Include(t => t.Cliente)
            .Include(t => t.TicketEtiquetas).ThenInclude(te => te.Etiqueta);
}
