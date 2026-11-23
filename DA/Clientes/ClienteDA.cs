using Abstracciones.Clientes;
using Abstracciones.Comunes;
using Abstracciones.Excepciones;
using DA.Comunes;
using DA.Contexto;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DA.Clientes;

public class ClienteDA : IClienteDA
{
    private readonly SistemaSoporteContexto _ctx;

    public ClienteDA(SistemaSoporteContexto ctx) => _ctx = ctx;

    public async Task<ResultadoPaginado<Cliente>> BuscarAsync(ClienteFiltro filtro, CancellationToken ct)
    {
        // AsNoTracking: solo leemos, EF no necesita vigilar los objetos.
        IQueryable<Cliente> consulta = _ctx.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var texto = filtro.Q.Trim();
            consulta = consulta.Where(c =>
                c.Nombre.Contains(texto) ||
                c.Correo.Contains(texto) ||
                (c.Empresa != null && c.Empresa.Contains(texto)));
        }

        var total = await consulta.CountAsync(ct);

        // Orden estable (Nombre, luego Id) para que la paginación no repita ni se salte filas.
        var items = await consulta
            .OrderBy(c => c.Nombre).ThenBy(c => c.Id)
            .Skip((filtro.Pagina - 1) * filtro.Tamano)
            .Take(filtro.Tamano)
            .ToListAsync(ct);

        return new ResultadoPaginado<Cliente>
        {
            Items = items,
            Total = total,
            Pagina = filtro.Pagina,
            Tamano = filtro.Tamano
        };
    }

    public Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken ct) =>
        _ctx.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cliente?> ObtenerPorCorreoAsync(string correo, CancellationToken ct) =>
        _ctx.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Correo == correo, ct);

    public async Task<Cliente> AgregarAsync(Cliente cliente, CancellationToken ct)
    {
        _ctx.Clientes.Add(cliente);
        try
        {
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ErroresSql.EsDuplicado(ex))
        {
            throw new ConflictoException($"Ya existe un cliente con el correo '{cliente.Correo}'.");
        }
        return cliente;
    }

    public async Task<bool> ActualizarAsync(Cliente datos, CancellationToken ct)
    {
        // Entidad rastreada: EF detecta SOLO las propiedades que cambiaron y genera un UPDATE mínimo.
        var existente = await _ctx.Clientes.FirstOrDefaultAsync(c => c.Id == datos.Id, ct);
        if (existente is null) return false;

        existente.Nombre = datos.Nombre;
        existente.Correo = datos.Correo;
        existente.Telefono = datos.Telefono;
        existente.Empresa = datos.Empresa;

        try
        {
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ErroresSql.EsDuplicado(ex))
        {
            throw new ConflictoException($"Ya existe otro cliente con el correo '{datos.Correo}'.");
        }
        return true;
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken ct)
    {
        var existente = await _ctx.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existente is null) return false;

        _ctx.Clientes.Remove(existente);
        try
        {
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ErroresSql.EsLlaveForanea(ex))
        {
            throw new ConflictoException("El cliente tiene tickets asociados y no se puede eliminar.");
        }
        return true;
    }

    public Task<bool> TieneTicketsAsync(int clienteId, CancellationToken ct) =>
        _ctx.Tickets.AnyAsync(t => t.ClienteId == clienteId, ct);

    public async Task<List<Cliente>> BuscarPorNombreSpAsync(string? nombre, CancellationToken ct)
    {
        // Parámetro SQL: el valor viaja separado del texto SQL, así NO se puede inyectar código.
        var parametro = new SqlParameter("@Nombre", (object?)nombre ?? DBNull.Value);
        return await _ctx.Clientes
            .FromSqlRaw("EXEC dbo.sp_BuscarCliente @Nombre", parametro)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
