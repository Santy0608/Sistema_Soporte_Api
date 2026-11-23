using System.ComponentModel.DataAnnotations;
using Abstracciones.Comunes;

namespace Abstracciones.Clientes;

/// <summary>
/// Entrada para crear (y también para reemplazar con PUT) un cliente.
/// </summary>
public class ClienteCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos.")]
    public string Telefono { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Empresa { get; set; }
}

public class ClienteActualizarDto : ClienteCrearDto { }

/// <summary>Salida hacia el cliente HTTP. Nunca se expone la entidad directamente.</summary>
public record ClienteDto(
    int Id,
    string Nombre,
    string Correo,
    string Telefono,
    string? Empresa,
    DateTime FechaRegistro);

public class ClienteFiltro : ParametrosPaginacion
{
    /// <summary>Texto buscado en nombre, correo o empresa.</summary>
    [StringLength(100)]
    public string? Q { get; set; }
}
