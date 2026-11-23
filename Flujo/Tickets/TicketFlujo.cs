using Abstracciones.Clientes;
using Abstracciones.Comunes;
using Abstracciones.Etiquetas;
using Abstracciones.Excepciones;
using Abstracciones.Tickets;

namespace Flujo.Tickets;

public class TicketFlujo : ITicketFlujo
{
    /// <summary>
    /// Tabla de transiciones válidas (igual para todas las variantes de la Práctica 1).
    /// Una tabla es más fácil de leer, de probar y de modificar que una cadena de if.
    /// </summary>
    private static readonly Dictionary<EstadoTicket, EstadoTicket[]> Transiciones = new()
    {
        [EstadoTicket.Abierto] = new[] { EstadoTicket.EnProceso },
        [EstadoTicket.EnProceso] = new[] { EstadoTicket.Resuelto },
        [EstadoTicket.Resuelto] = new[] { EstadoTicket.Cerrado, EstadoTicket.EnProceso },
        [EstadoTicket.Cerrado] = Array.Empty<EstadoTicket>()
    };

    private const int MaximoFilasCsv = 5000;

    private readonly ITicketDA _da;
    private readonly IClienteDA _clienteDA;
    private readonly IEtiquetaDA _etiquetaDA;
    private readonly TimeProvider _reloj;

    public TicketFlujo(ITicketDA da, IClienteDA clienteDA, IEtiquetaDA etiquetaDA, TimeProvider reloj)
    {
        _da = da;
        _clienteDA = clienteDA;
        _etiquetaDA = etiquetaDA;
        _reloj = reloj;
    }

    public async Task<ResultadoPaginado<TicketDto>> ListarAsync(TicketFiltro filtro, CancellationToken ct)
    {
        var pagina = await _da.BuscarAsync(filtro, ct);
        return new ResultadoPaginado<TicketDto>
        {
            Items = pagina.Items.Select(TicketMapeador.ADto).ToList(),
            Total = pagina.Total,
            Pagina = pagina.Pagina,
            Tamano = pagina.Tamano
        };
    }

    public async Task<TicketDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var ticket = await _da.ObtenerPorIdAsync(id, ct) ?? throw new NoEncontradoException("Ticket", id);
        return TicketMapeador.ADto(ticket);
    }

    public async Task<TicketDto> ObtenerPorCodigoAsync(string codigo, CancellationToken ct)
    {
        var ticket = await _da.ObtenerPorCodigoAsync(codigo, ct) ?? throw new NoEncontradoException("Ticket", codigo);
        return TicketMapeador.ADto(ticket);
    }

    public async Task<TicketDto> CrearAsync(TicketCrearDto dto, CancellationToken ct)
    {
        // 422 y no 404: el recurso de la URL (/api/tickets) sí existe; lo inválido es una referencia dentro del cuerpo.
        if (await _clienteDA.ObtenerPorIdAsync(dto.ClienteId, ct) is null)
            throw new ValidacionNegocioException($"El cliente {dto.ClienteId} no existe.");

        var ahora = _reloj.GetUtcNow().UtcDateTime;
        var secuencia = await _da.ContarPorAnioAsync(ahora.Year, ct) + 1;

        var ticket = new Ticket
        {
            // El código lo genera el sistema; el cliente HTTP nunca lo envía.
            Codigo = FormatoCodigoTicket.Generar(ahora.Year, secuencia),
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            Prioridad = dto.Prioridad,
            Estado = EstadoTicket.Abierto,
            ClienteId = dto.ClienteId,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        await _da.AgregarAsync(ticket, ct);
        return await ObtenerAsync(ticket.Id, ct);
    }

    public async Task<TicketDto> CambiarEstadoAsync(int id, TicketCambiarEstadoDto dto, CancellationToken ct)
    {
        var ticket = await _da.ObtenerParaActualizarAsync(id, ct) ?? throw new NoEncontradoException("Ticket", id);

        var nuevoEstado = dto.Estado ?? throw new ValidacionNegocioException("El estado es obligatorio.");

        if (!Transiciones[ticket.Estado].Contains(nuevoEstado))
            throw new ConflictoException($"No se puede pasar un ticket de {ticket.Estado} a {nuevoEstado}.");

        var agente = dto.AgenteId ?? ticket.AgenteId;
        var prioridad = dto.Prioridad ?? ticket.Prioridad;

        // Regla especial (variante 0): un ticket Crítico no se puede resolver sin agente.
        // Se valida con los valores FINALES (los del body si vienen, si no los actuales).
        if (nuevoEstado == EstadoTicket.Resuelto && prioridad == PrioridadTicket.Critica && agente is null)
            throw new ConflictoException("Un ticket de prioridad Crítica no puede pasar a Resuelto sin agente asignado.");

        // Todas las validaciones pasaron: recién ahora se modifica el objeto.
        if (ticket.Estado == EstadoTicket.Resuelto && nuevoEstado == EstadoTicket.EnProceso)
            ticket.Reaperturas++;

        ticket.Estado = nuevoEstado;
        ticket.AgenteId = agente;
        ticket.Prioridad = prioridad;
        ticket.FechaActualizacion = _reloj.GetUtcNow().UtcDateTime;

        await _da.GuardarCambiosAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<TicketDto> ReabrirAsync(int id, CancellationToken ct)
    {
        var ticket = await _da.ObtenerPorIdAsync(id, ct) ?? throw new NoEncontradoException("Ticket", id);

        // Reabrir SOLO aplica a un ticket Resuelto (pasar Abierto a EnProceso no es una reapertura).
        if (ticket.Estado != EstadoTicket.Resuelto)
            throw new ConflictoException($"Solo se puede reabrir un ticket Resuelto; este está {ticket.Estado}.");

        return await CambiarEstadoAsync(id, new TicketCambiarEstadoDto { Estado = EstadoTicket.EnProceso }, ct);
    }

    public async Task<TicketResumenDto> ResumenAsync(CancellationToken ct)
    {
        var porEstado = await _da.ContarPorEstadoAsync(ct);
        var porPrioridad = await _da.ContarPorPrioridadAsync(ct);

        // Se incluyen TODOS los valores del enum (con 0) para que el cliente no tenga que adivinar claves ausentes.
        var estados = Enum.GetValues<EstadoTicket>()
            .ToDictionary(e => e.ToString(), e => porEstado.GetValueOrDefault(e));
        var prioridades = Enum.GetValues<PrioridadTicket>()
            .ToDictionary(p => p.ToString(), p => porPrioridad.GetValueOrDefault(p));

        return new TicketResumenDto(estados.Values.Sum(), estados, prioridades);
    }

    public async Task AsignarEtiquetaAsync(int ticketId, int etiquetaId, CancellationToken ct)
    {
        var ticket = await _da.ObtenerPorIdAsync(ticketId, ct) ?? throw new NoEncontradoException("Ticket", ticketId);
        _ = await _etiquetaDA.ObtenerPorIdAsync(etiquetaId, ct) ?? throw new NoEncontradoException("Etiqueta", etiquetaId);

        if (ticket.Estado == EstadoTicket.Cerrado)
            throw new ConflictoException("El ticket está cerrado y no admite cambios de etiquetas.");

        var asignada = await _da.AsignarEtiquetaAsync(ticketId, etiquetaId, _reloj.GetUtcNow().UtcDateTime, ct);
        if (!asignada)
            throw new ConflictoException("La etiqueta ya estaba asignada a este ticket.");
    }

    public async Task QuitarEtiquetaAsync(int ticketId, int etiquetaId, CancellationToken ct)
    {
        var ticket = await _da.ObtenerPorIdAsync(ticketId, ct) ?? throw new NoEncontradoException("Ticket", ticketId);

        if (ticket.Estado == EstadoTicket.Cerrado)
            throw new ConflictoException("El ticket está cerrado y no admite cambios de etiquetas.");

        var quitada = await _da.QuitarEtiquetaAsync(ticketId, etiquetaId, _reloj.GetUtcNow().UtcDateTime, ct);
        if (!quitada)
            throw new NoEncontradoException("Asignación de etiqueta", $"{ticketId}/{etiquetaId}");
    }

    public async Task<byte[]> ExportarCsvAsync(TicketFiltro filtro, CancellationToken ct)
    {
        // Se exporta TODO lo que cumpla el filtro (sin el límite de 50 de la paginación), con un tope de seguridad.
        var todo = new TicketFiltro
        {
            Pagina = 1,
            Tamano = MaximoFilasCsv,
            Estado = filtro.Estado,
            Prioridad = filtro.Prioridad,
            ClienteId = filtro.ClienteId,
            Desde = filtro.Desde,
            Hasta = filtro.Hasta,
            Q = filtro.Q,
            Orden = filtro.Orden
        };

        var pagina = await _da.BuscarAsync(todo, ct);
        return TicketCsv.Generar(pagina.Items.Select(TicketMapeador.ADto));
    }
}
