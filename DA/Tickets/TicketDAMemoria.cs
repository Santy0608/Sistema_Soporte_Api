using Abstracciones.Comunes;
using Abstracciones.Tickets;

namespace DA.Tickets;

/// <summary>
/// Implementación con una LISTA EN MEMORIA: es la que pide la Práctica 1 (al menos 25 tickets al arrancar).
/// Se activa con "Almacenamiento:Tickets": "Memoria" en appsettings.json.
/// Se registra como Singleton: si fuera Scoped, la lista se reiniciaría en cada petición.
/// Limitaciones: los datos se pierden al reiniciar y no soporta etiquetas (requieren base de datos).
/// </summary>
public class TicketDAMemoria : ITicketDA
{
    private readonly List<Ticket> _tickets = new();
    private readonly object _candado = new();
    private int _siguienteId = 1;

    public TicketDAMemoria(TimeProvider reloj)
    {
        Sembrar(reloj.GetUtcNow().UtcDateTime);
    }

    private void Sembrar(DateTime ahora)
    {
        var titulos = new[]
        {
            "No puedo iniciar sesión", "La impresora no responde", "Error al generar factura",
            "Solicitud de nuevo usuario", "Correo no sincroniza", "Lentitud en el sistema"
        };

        for (var i = 1; i <= 30; i++)
        {
            var creado = ahora.AddDays(-i).AddHours(-i % 7);
            var estado = (EstadoTicket)(i % 4);
            _tickets.Add(new Ticket
            {
                Id = _siguienteId,
                Codigo = FormatoCodigoTicket.Generar(creado.Year, _siguienteId),
                Titulo = $"{titulos[i % titulos.Length]} (caso {i})",
                Descripcion = "Ticket de ejemplo generado al arrancar el API para probar filtros y paginación.",
                Prioridad = (PrioridadTicket)(i % 4),
                Estado = estado,
                ClienteId = (i % 8) + 1,
                AgenteId = estado == EstadoTicket.Abierto ? null : (i % 3) + 1,
                FechaCreacion = creado,
                FechaActualizacion = creado
            });
            _siguienteId++;
        }
    }

    public Task<ResultadoPaginado<Ticket>> BuscarAsync(TicketFiltro filtro, CancellationToken ct)
    {
        List<Ticket> copia;
        lock (_candado) copia = _tickets.ToList();

        var consulta = TicketConsultas.AplicarFiltros(copia.AsQueryable(), filtro);

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var texto = filtro.Q.Trim();
            consulta = consulta.Where(t =>
                t.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                t.Descripcion.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        var total = consulta.Count();
        var items = TicketConsultas.Ordenar(consulta, filtro.Orden)
            .Skip((filtro.Pagina - 1) * filtro.Tamano)
            .Take(filtro.Tamano)
            .ToList();

        return Task.FromResult(new ResultadoPaginado<Ticket>
        {
            Items = items,
            Total = total,
            Pagina = filtro.Pagina,
            Tamano = filtro.Tamano
        });
    }

    public Task<Ticket?> ObtenerPorIdAsync(int id, CancellationToken ct)
    {
        lock (_candado) return Task.FromResult(_tickets.FirstOrDefault(t => t.Id == id));
    }

    public Task<Ticket?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct)
    {
        lock (_candado) return Task.FromResult(_tickets.FirstOrDefault(t => t.Codigo == codigo));
    }

    // En memoria el objeto devuelto ES el de la lista: lo que Flujo modifique queda guardado.
    public Task<Ticket?> ObtenerParaActualizarAsync(int id, CancellationToken ct) => ObtenerPorIdAsync(id, ct);

    public Task GuardarCambiosAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<int> ContarPorAnioAsync(int anio, CancellationToken ct)
    {
        lock (_candado) return Task.FromResult(_tickets.Count(t => t.FechaCreacion.Year == anio));
    }

    public Task<Ticket> AgregarAsync(Ticket ticket, CancellationToken ct)
    {
        lock (_candado)
        {
            ticket.Id = _siguienteId++;
            _tickets.Add(ticket);
        }
        return Task.FromResult(ticket);
    }

    public Task<Dictionary<EstadoTicket, int>> ContarPorEstadoAsync(CancellationToken ct)
    {
        lock (_candado)
            return Task.FromResult(_tickets.GroupBy(t => t.Estado).ToDictionary(g => g.Key, g => g.Count()));
    }

    public Task<Dictionary<PrioridadTicket, int>> ContarPorPrioridadAsync(CancellationToken ct)
    {
        lock (_candado)
            return Task.FromResult(_tickets.GroupBy(t => t.Prioridad).ToDictionary(g => g.Key, g => g.Count()));
    }

    public Task<bool> AsignarEtiquetaAsync(int ticketId, int etiquetaId, DateTime fecha, CancellationToken ct) =>
        throw new NotSupportedException("Las etiquetas requieren Almacenamiento:Tickets = Sql.");

    public Task<bool> QuitarEtiquetaAsync(int ticketId, int etiquetaId, DateTime fecha, CancellationToken ct) =>
        throw new NotSupportedException("Las etiquetas requieren Almacenamiento:Tickets = Sql.");
}
