using Abstracciones.Comunes;

namespace Abstracciones.Clientes;

/// <summary>Contrato de la lógica de negocio de clientes. El API solo conoce esta interfaz.</summary>
public interface IClienteFlujo
{
    Task<ResultadoPaginado<ClienteDto>> ListarAsync(ClienteFiltro filtro, CancellationToken ct);
    Task<ClienteDto> ObtenerAsync(int id, CancellationToken ct);
    Task<ClienteDto> CrearAsync(ClienteCrearDto dto, CancellationToken ct);
    Task ActualizarAsync(int id, ClienteActualizarDto dto, CancellationToken ct);
    Task EliminarAsync(int id, CancellationToken ct);
    Task<List<ClienteDto>> BuscarPorNombreSpAsync(string? nombre, CancellationToken ct);
}
