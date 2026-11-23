using System.Linq.Expressions;
using Abstracciones.Tickets;

namespace DA.Tickets;

/// <summary>
/// Filtros y orden compartidos por TicketDA (EF Core) y TicketDAMemoria (lista en memoria).
/// Usa solo expresiones que EF sabe traducir a SQL y que LINQ to Objects también ejecuta.
/// </summary>
internal static class TicketConsultas
{
    // Peso numérico de la prioridad: como se guarda como texto, ordenar por la columna sería alfabético.
    private static readonly Expression<Func<Ticket, int>> PesoPrioridad = t =>
        t.Prioridad == PrioridadTicket.Critica ? 3 :
        t.Prioridad == PrioridadTicket.Alta ? 2 :
        t.Prioridad == PrioridadTicket.Media ? 1 : 0;

    public static IQueryable<Ticket> AplicarFiltros(IQueryable<Ticket> q, TicketFiltro f)
    {
        if (f.Estado.HasValue)
        {
            var estado = f.Estado.Value;
            q = q.Where(t => t.Estado == estado);
        }
        if (f.Prioridad.HasValue)
        {
            var prioridad = f.Prioridad.Value;
            q = q.Where(t => t.Prioridad == prioridad);
        }
        if (f.ClienteId.HasValue)
        {
            var clienteId = f.ClienteId.Value;
            q = q.Where(t => t.ClienteId == clienteId);
        }
        if (f.Desde.HasValue)
        {
            var desde = f.Desde.Value.Date;
            q = q.Where(t => t.FechaCreacion >= desde);
        }
        if (f.Hasta.HasValue)
        {
            // Hasta incluye TODO el día indicado: se compara contra el inicio del día siguiente.
            var hastaExclusivo = f.Hasta.Value.Date.AddDays(1);
            q = q.Where(t => t.FechaCreacion < hastaExclusivo);
        }
        return q;
    }

    public static IQueryable<Ticket> Ordenar(IQueryable<Ticket> q, string? orden) => orden switch
    {
        "fecha" => q.OrderBy(t => t.FechaCreacion).ThenBy(t => t.Id),
        "prioridad" => q.OrderBy(PesoPrioridad).ThenBy(t => t.Id),
        "-prioridad" => q.OrderByDescending(PesoPrioridad).ThenByDescending(t => t.Id),
        _ => q.OrderByDescending(t => t.FechaCreacion).ThenByDescending(t => t.Id) // "-fecha" por defecto
    };
}
