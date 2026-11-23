using Abstracciones.Tickets;

namespace Abstracciones.Etiquetas;

/// <summary>
/// Entidad intermedia de la relación N-M entre Ticket y Etiqueta.
/// Tiene datos propios (FechaAsignacion): por eso es una entidad explícita y no un simple skip navigation.
/// La llave primaria es COMPUESTA (TicketId, EtiquetaId), se configura en DA con Fluent API.
/// </summary>
public class TicketEtiqueta
{
    public int TicketId { get; set; }
    public int EtiquetaId { get; set; }
    public DateTime FechaAsignacion { get; set; }

    public Ticket? Ticket { get; set; }
    public Etiqueta? Etiqueta { get; set; }
}
