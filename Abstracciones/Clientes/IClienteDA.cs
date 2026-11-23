using Abstracciones.Comunes;

namespace Abstracciones.Clientes;

/// <summary>Contrato de acceso a datos de clientes. Solo DA sabe cómo se implementa.</summary>
public interface IClienteDA
{
    Task<ResultadoPaginado<Cliente>> BuscarAsync(ClienteFiltro filtro, CancellationToken ct);
    Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken ct);
    Task<Cliente?> ObtenerPorCorreoAsync(string correo, CancellationToken ct);
    Task<Cliente> AgregarAsync(Cliente cliente, CancellationToken ct);

    /// <summary>Actualiza Nombre, Correo, Telefono y Empresa. Devuelve false si el cliente no existe.</summary>
    Task<bool> ActualizarAsync(Cliente datos, CancellationToken ct);

    Task<bool> EliminarAsync(int id, CancellationToken ct);
    Task<bool> TieneTicketsAsync(int clienteId, CancellationToken ct);

    /// <summary>Ejemplo didáctico de procedimiento almacenado con parámetros (sin concatenar SQL).</summary>
    Task<List<Cliente>> BuscarPorNombreSpAsync(string? nombre, CancellationToken ct);
}
