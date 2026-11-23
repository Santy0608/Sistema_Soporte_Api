using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Etiquetas;

public class EtiquetaCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(40, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "El color debe ser hexadecimal, ej. #ff8800.")]
    public string Color { get; set; } = "#6c757d";
}

public record EtiquetaDto(int Id, string Nombre, string Color);
