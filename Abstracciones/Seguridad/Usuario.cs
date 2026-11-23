
namespace Abstracciones.Seguridad
{

    public enum RolUsuario { Agente, Admin }

    public class Usuario
    {
        public int Id { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        public string ClaveHash { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }
        public bool Activo { get; set; } = true;
    }

}
