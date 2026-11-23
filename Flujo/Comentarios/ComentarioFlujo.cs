using Abstracciones.Comentarios;
using Abstracciones.Excepciones;
using Abstracciones.Tickets;

namespace Flujo.Comentarios;

public class ComentarioFlujo : IComentarioFlujo
{
    private readonly IComentarioDA _da;
    private readonly ITicketDA _ticketDA;
    private readonly TimeProvider _reloj;

    public ComentarioFlujo(IComentarioDA da, ITicketDA ticketDA, TimeProvider reloj)
    {
        _da = da;
        _ticketDA = ticketDA;
        _reloj = reloj;
    }

    public async Task<List<ComentarioDto>> ListarAsync(int ticketId, CancellationToken ct)
    {
        _ = await _ticketDA.ObtenerPorIdAsync(ticketId, ct) ?? throw new NoEncontradoException("Ticket", ticketId);

        var comentarios = await _da.ListarPorTicketAsync(ticketId, ct);
        return comentarios.Select(ADto).ToList();
    }

    public async Task<ComentarioDto> AgregarAsync(int ticketId, ComentarioCrearDto dto, CancellationToken ct)
    {
        var ticket = await _ticketDA.ObtenerPorIdAsync(ticketId, ct)
            ?? throw new NoEncontradoException("Ticket", ticketId);

        // Regla de negocio: un ticket cerrado ya no recibe comentarios.
        if (ticket.Estado == EstadoTicket.Cerrado)
            throw new ConflictoException("El ticket está cerrado y no admite más comentarios.");

        var comentario = await _da.AgregarAsync(new Comentario
        {
            TicketId = ticketId,
            Autor = dto.Autor.Trim(),
            Texto = dto.Texto.Trim(),
            EsInterno = dto.EsInterno,
            FechaCreacion = _reloj.GetUtcNow().UtcDateTime
        }, ct);

        return ADto(comentario);
    }

    private static ComentarioDto ADto(Comentario c) =>
        new(c.Id, c.TicketId, c.Autor, c.Texto, c.EsInterno, c.FechaCreacion);
}
