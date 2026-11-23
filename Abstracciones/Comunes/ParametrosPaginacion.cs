using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Comunes;

/// <summary>
/// Base de todos los filtros de listas. Las anotaciones hacen que [ApiController]
/// responda 400 automáticamente si la página o el tamaño son absurdos.
/// </summary>
public class ParametrosPaginacion
{
    public const int TamanoMaximo = 50;

    [Range(1, 100000, ErrorMessage = "La página debe estar entre 1 y 100000.")]
    public int Pagina { get; set; } = 1;

    [Range(1, TamanoMaximo, ErrorMessage = "El tamaño debe estar entre 1 y 50.")]
    public int Tamano { get; set; } = 10;
}
