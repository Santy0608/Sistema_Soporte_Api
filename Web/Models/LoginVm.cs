using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>Datos del formulario de inicio de sesión.</summary>
public class LoginVm
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Escriba un correo válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>A dónde volver después del login (la página que el usuario quería abrir).</summary>
    public string? ReturnUrl { get; set; }
}
