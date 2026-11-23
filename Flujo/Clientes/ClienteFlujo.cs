using Abstracciones.Clientes;
using Abstracciones.Comunes;
using Abstracciones.Excepciones;

namespace Flujo.Clientes;

public class ClienteFlujo : IClienteFlujo
{
    private readonly IClienteDA _da;
    private readonly TimeProvider _reloj;

    public ClienteFlujo(IClienteDA da, TimeProvider reloj)
    {
        _da = da;
        _reloj = reloj;
    }

    public async Task<ResultadoPaginado<ClienteDto>> ListarAsync(ClienteFiltro filtro, CancellationToken ct)
    {
        var pagina = await _da.BuscarAsync(filtro, ct);
        return new ResultadoPaginado<ClienteDto>
        {
            Items = pagina.Items.Select(ClienteMapeador.ADto).ToList(),
            Total = pagina.Total,
            Pagina = pagina.Pagina,
            Tamano = pagina.Tamano
        };
    }

    public async Task<ClienteDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var cliente = await _da.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("Cliente", id);
        return ClienteMapeador.ADto(cliente);
    }

    public async Task<ClienteDto> CrearAsync(ClienteCrearDto dto, CancellationToken ct)
    {
        var correo = NormalizarCorreo(dto.Correo);

        if (await _da.ObtenerPorCorreoAsync(correo, ct) is not null)
            throw new ConflictoException($"Ya existe un cliente con el correo '{correo}'.");

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            Telefono = dto.Telefono.Trim(),
            Empresa = LimpiarOpcional(dto.Empresa),
            FechaRegistro = _reloj.GetUtcNow().UtcDateTime
        };

        await _da.AgregarAsync(cliente, ct);
        return ClienteMapeador.ADto(cliente);
    }

    public async Task ActualizarAsync(int id, ClienteActualizarDto dto, CancellationToken ct)
    {
        var correo = NormalizarCorreo(dto.Correo);

        _ = await _da.ObtenerPorIdAsync(id, ct) ?? throw new NoEncontradoException("Cliente", id);

        // El correo puede repetirse solo si pertenece al MISMO cliente que se está editando.
        var otro = await _da.ObtenerPorCorreoAsync(correo, ct);
        if (otro is not null && otro.Id != id)
            throw new ConflictoException($"Ya existe otro cliente con el correo '{correo}'.");

        var actualizado = await _da.ActualizarAsync(new Cliente
        {
            Id = id,
            Nombre = dto.Nombre.Trim(),
            Correo = correo,
            Telefono = dto.Telefono.Trim(),
            Empresa = LimpiarOpcional(dto.Empresa)
        }, ct);

        if (!actualizado) throw new NoEncontradoException("Cliente", id);
    }

    public async Task EliminarAsync(int id, CancellationToken ct)
    {
        _ = await _da.ObtenerPorIdAsync(id, ct) ?? throw new NoEncontradoException("Cliente", id);

        if (await _da.TieneTicketsAsync(id, ct))
            throw new ConflictoException("El cliente tiene tickets asociados y no se puede eliminar.");

        if (!await _da.EliminarAsync(id, ct))
            throw new NoEncontradoException("Cliente", id);
    }

    public async Task<List<ClienteDto>> BuscarPorNombreSpAsync(string? nombre, CancellationToken ct)
    {
        var clientes = await _da.BuscarPorNombreSpAsync(nombre?.Trim(), ct);
        return clientes.Select(ClienteMapeador.ADto).ToList();
    }

    // UNA sola regla de normalización para TODOS los caminos de escritura.
    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    private static string? LimpiarOpcional(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
