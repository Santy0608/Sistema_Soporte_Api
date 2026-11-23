using Abstracciones.Excepciones;

namespace Abstracciones.Seguridad
{
    public interface IUsuarioDA
    {
        Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default);
    }

    public interface IGeneradorToken
    {
        (string Token, DateTime ExpiraEnUtc) Generar(Usuario usuario);
    }

    public interface IAutenticacionFlujo
    {
        Task<SesionDto> IniciarSesionAsync(LoginDto dto, CancellationToken ct = default);
    }

    public class CredencialesInvalidasException : ExcepcionDeNegocio
    {
        // Mensaje único: no se revela si lo incorrecto fue el correo o la clave.
        public CredencialesInvalidasException() : base("Correo o clave incorrectos.") { }
    }
}
