using Abstracciones.Tickets;

namespace Flujo.Tickets;

internal static class TicketMapeador
{
    public static TicketDto ADto(Ticket t) => new(
        t.Id,
        t.Codigo,
        t.Titulo,
        t.Descripcion,
        t.Prioridad,
        t.Estado,
        t.ClienteId,
        t.Cliente?.Nombre,
        t.AgenteId,
        t.Reaperturas,
        t.FechaCreacion,
        t.FechaActualizacion,
        t.TicketEtiquetas
            .Where(te => te.Etiqueta is not null)
            .Select(te => te.Etiqueta!.Nombre)
            .OrderBy(n => n)
            .ToList());
}
