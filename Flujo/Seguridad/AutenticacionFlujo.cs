using Abstracciones.Seguridad;

namespace Flujo.Seguridad
{
    public class AutenticacionFlujo : IAutenticacionFlujo
    {
        private readonly IUsuarioDA _usuarios;
        private readonly IGeneradorToken _generador;

        public AutenticacionFlujo(IUsuarioDA usuarios, IGeneradorToken generador)
        {
            _usuarios = usuarios;
            _generador = generador;
        }

        public async Task<SesionDto> IniciarSesionAsync(LoginDto dto, CancellationToken ct = default)
        {
            var usuario = await _usuarios.ObtenerPorCorreoAsync(dto.Correo.Trim().ToLowerInvariant(), ct);

            if (usuario is null || !usuario.Activo || !HasherPassword.Verificar(dto.Password, usuario.ClaveHash))
                throw new CredencialesInvalidasException();

            var (token, expira) = _generador.Generar(usuario);
            return new SesionDto(token, expira, usuario.Nombre, usuario.Correo, usuario.Rol.ToString());
        }
    }
}
