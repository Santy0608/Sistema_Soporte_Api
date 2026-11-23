using Abstracciones.Seguridad;
using DA.Contexto;
using Microsoft.EntityFrameworkCore;

namespace DA.Seguridad
{
    public class UsuarioDA : IUsuarioDA
    {
        private readonly SistemaSoporteContexto _contexto;
        public UsuarioDA(SistemaSoporteContexto contexto) => _contexto = contexto;

        public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default) =>
        _contexto.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Correo == correo, ct);
    }
}
