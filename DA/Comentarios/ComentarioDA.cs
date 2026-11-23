using Abstracciones.Comentarios;
using DA.Contexto;
using Microsoft.EntityFrameworkCore;

namespace DA.Comentarios;

public class ComentarioDA : IComentarioDA
{
    private readonly SistemaSoporteContexto _ctx;

    public ComentarioDA(SistemaSoporteContexto ctx) => _ctx = ctx;

    public Task<List<Comentario>> ListarPorTicketAsync(int ticketId, CancellationToken ct) =>
        _ctx.Comentarios.AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.FechaCreacion).ThenBy(c => c.Id)
            .ToListAsync(ct);

    public async Task<Comentario> AgregarAsync(Comentario comentario, CancellationToken ct)
    {
        _ctx.Comentarios.Add(comentario);
        await _ctx.SaveChangesAsync(ct);
        return comentario;
    }
}
