namespace Abstracciones.Comentarios;

public interface IComentarioDA
{
    Task<List<Comentario>> ListarPorTicketAsync(int ticketId, CancellationToken ct);
    Task<Comentario> AgregarAsync(Comentario comentario, CancellationToken ct);
}
