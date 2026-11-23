using System.Security.Claims;
using System.Text;
using Abstracciones.Seguridad;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace API.Seguridad;

/// <summary>Valores de la sección "Jwt" de la configuración. La Clave NUNCA va en appsettings.json (User Secrets o variable Jwt__Clave).</summary>
public class OpcionesJwt
{
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int Minutos { get; set; } = 60;
    public string Clave { get; set; } = string.Empty;
}

/// <summary>
/// Fabrica el token JWT firmado de un usuario. Flujo solo conoce la interfaz IGeneradorToken;
/// la librería de tokens vive aquí, en la capa API.
/// </summary>
public class GeneradorTokenJwt : IGeneradorToken
{
    private readonly OpcionesJwt _opciones;

    public GeneradorTokenJwt(IOptions<OpcionesJwt> opciones) => _opciones = opciones.Value;

    public (string Token, DateTime ExpiraEnUtc) Generar(Usuario usuario)
    {
        // Mínimo 32 caracteres: HMAC-SHA256 pide una clave de al menos 256 bits.
        if (_opciones.Clave.Length < 32)
            throw new InvalidOperationException(
                "Falta Jwt:Clave (mínimo 32 caracteres). Use User Secrets en desarrollo o la variable Jwt__Clave en Azure.");

        var expira = DateTime.UtcNow.AddMinutes(_opciones.Minutos);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _opciones.Emisor,
            Audience = _opciones.Audiencia,
            Expires = expira,

            // Los claims son lo que el token "dice" de la persona. El de Role es el que
            // [Authorize(Roles = "...")] consulta después. Nunca se mete la clave ni el hash dentro del token.
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            }),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave)),
                SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
