namespace Abstracciones.Comentarios;

public interface IComentarioFlujo
{
    Task<List<ComentarioDto>> ListarAsync(int ticketId, CancellationToken ct);
    Task<ComentarioDto> AgregarAsync(int ticketId, ComentarioCrearDto dto, CancellationToken ct);
}
