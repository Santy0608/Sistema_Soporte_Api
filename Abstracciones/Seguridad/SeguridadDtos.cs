using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Seguridad
{
    public class LoginDto
    {
        [Required, EmailAddress] public string Correo { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public record SesionDto(string Token, DateTime ExpiraEnUtc, string Nombre, string Correo, string Rol);
}
