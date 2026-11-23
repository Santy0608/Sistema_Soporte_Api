using System.ComponentModel.DataAnnotations;
using Abstracciones.Comunes;

namespace Abstracciones.Tickets;

public class TicketCrearDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 120 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [EnumDataType(typeof(PrioridadTicket))]
    public PrioridadTicket Prioridad { get; set; } = PrioridadTicket.Media;

    [Range(1, int.MaxValue, ErrorMessage = "ClienteId debe ser mayor o igual a 1.")]
    public int ClienteId { get; set; }
}

public class TicketCambiarEstadoDto
{
    // Nullable + Required: si no viene en el JSON, falla con 400 (un enum normal valdría 0 = Abierto sin avisar).
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [EnumDataType(typeof(EstadoTicket))]
    public EstadoTicket? Estado { get; set; }

    [Range(1, int.MaxValue)]
    public int? AgenteId { get; set; }

    [EnumDataType(typeof(PrioridadTicket))]
    public PrioridadTicket? Prioridad { get; set; }
}

public record TicketDto(
    int Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    PrioridadTicket Prioridad,
    EstadoTicket Estado,
    int ClienteId,
    string? ClienteNombre,
    int? AgenteId,
    int Reaperturas,
    DateTime FechaCreacion,
    DateTime FechaActualizacion,
    IReadOnlyList<string> Etiquetas);

public record TicketResumenDto(
    int Total,
    Dictionary<string, int> PorEstado,
    Dictionary<string, int> PorPrioridad);

public class TicketFiltro : ParametrosPaginacion, IValidatableObject
{
    public EstadoTicket? Estado { get; set; }
    public PrioridadTicket? Prioridad { get; set; }

    [Range(1, int.MaxValue)]
    public int? ClienteId { get; set; }

    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    /// <summary>Texto buscado en título y descripción.</summary>
    [StringLength(100)]
    public string? Q { get; set; }

    /// <summary>fecha, -fecha, prioridad, -prioridad. Por defecto -fecha.</summary>
    [RegularExpression("^-?(fecha|prioridad)$", ErrorMessage = "Orden permitido: fecha, -fecha, prioridad, -prioridad.")]
    public string Orden { get; set; } = "-fecha";

    // Validación que involucra DOS campos: no se puede con un solo atributo.
    public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
    {
        if (Desde.HasValue && Hasta.HasValue && Desde > Hasta)
        {
            yield return new ValidationResult(
                "'Desde' no puede ser posterior a 'Hasta'.",
                new[] { nameof(Desde), nameof(Hasta) });
        }
    }
}
