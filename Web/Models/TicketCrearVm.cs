using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>
/// Datos del formulario "Nuevo ticket". Las mismas reglas que tiene el API, repetidas aquí a propósito:
/// el MVC valida primero para no hacer un viaje de red inútil, pero el API SIEMPRE vuelve a validar,
/// porque a un API puede llamarlo cualquiera, no solo este MVC.
/// </summary>
public class TicketCrearVm
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 120 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 2000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Elija una prioridad.")]
    public string Prioridad { get; set; } = "Media";

    [Range(1, int.MaxValue, ErrorMessage = "Indique el identificador del cliente.")]
    public int ClienteId { get; set; }
}
