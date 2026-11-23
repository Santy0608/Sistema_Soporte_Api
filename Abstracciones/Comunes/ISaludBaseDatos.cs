namespace Abstracciones.Comunes;

/// <summary>
/// Contrato para preguntar "¿la base de datos contesta?". Vive en Abstracciones para que el API
/// pueda exponer /salud sin conocer Entity Framework (la regla de capas sigue intacta).
/// Azure y cualquier balanceador usan un endpoint así para saber si la app está viva.
/// Lo hicimos a mano (una interfaz y un endpoint) en lugar de usar el paquete de HealthChecks para que
/// vean cada pieza; en un proyecto grande el paquete es la opción normal.
/// </summary>`
public interface ISaludBaseDatos
{
    Task<bool> EstaDisponibleAsync(CancellationToken ct = default);
}
