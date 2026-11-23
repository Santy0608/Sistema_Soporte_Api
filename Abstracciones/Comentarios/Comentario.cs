using Abstracciones.Tickets;

namespace Abstracciones.Comentarios;

public class Comentario
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Autor { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public bool EsInterno { get; set; }
    public DateTime FechaCreacion { get; set; }

    public Ticket? Ticket { get; set; }
}
