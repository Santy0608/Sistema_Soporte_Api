using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Comentarios;

public class ComentarioCrearDto
{
    [Required(ErrorMessage = "El autor es obligatorio.")]
    [StringLength(100, MinimumLength = 2)]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "El texto es obligatorio.")]
    [StringLength(1000, MinimumLength = 2, ErrorMessage = "El texto debe tener entre 2 y 1000 caracteres.")]
    public string Texto { get; set; } = string.Empty;

    /// <summary>Los comentarios internos solo los ve el equipo de soporte.</summary>
    public bool EsInterno { get; set; }
}

public record ComentarioDto(int Id, int TicketId, string Autor, string Texto, bool EsInterno, DateTime FechaCreacion);
