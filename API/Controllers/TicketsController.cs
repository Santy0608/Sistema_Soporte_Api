using Abstracciones.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketFlujo _flujo;

    public TicketsController(ITicketFlujo flujo) => _flujo = flujo;

    /// <summary>
    /// Lista paginada con filtros. Una página fuera de rango devuelve 200 con lista vacía (y el total en los headers);
    /// no es un error: simplemente no hay filas ahí.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TicketDto>>> Listar([FromQuery] TicketFiltro filtro, CancellationToken ct)
    {
        var resultado = await _flujo.ListarAsync(filtro, ct);
        Response.Headers["X-Total-Count"] = resultado.Total.ToString();
        Response.Headers["X-Page"] = resultado.Pagina.ToString();
        Response.Headers["X-Page-Size"] = resultado.Tamano.ToString();
        return Ok(resultado.Items);
    }

    /// <summary>
    /// ETag débil calculado con Id + FechaActualizacion: cambia cada vez que el ticket cambia.
    /// Si el cliente manda If-None-Match con el ETag vigente, se responde 304 sin cuerpo.
    /// Limitación: no detecta cambios hechos directamente en la base de datos sin tocar FechaActualizacion.
    /// </summary>
    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType<TicketDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> Obtener(int id, CancellationToken ct)
    {
        var ticket = await _flujo.ObtenerAsync(id, ct);

        var etag = $"W/\"{ticket.Id}-{ticket.FechaActualizacion.Ticks}\"";
        Response.Headers.ETag = etag;

        if (Request.GetTypedHeaders().IfNoneMatch.Any(e => e.ToString() == etag))
            return StatusCode(StatusCodes.Status304NotModified);

        return Ok(ticket);
    }

    [HttpGet("codigo/{codigo:codigoTicket}")]
    [ProducesResponseType<TicketDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDto>> ObtenerPorCodigo(string codigo, CancellationToken ct) =>
        Ok(await _flujo.ObtenerPorCodigoAsync(codigo, ct));

    [HttpPost]
    [ProducesResponseType<TicketDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TicketDto>> Crear(TicketCrearDto dto, CancellationToken ct)
    {
        var creado = await _flujo.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPatch("{id:int:min(1)}/estado")]
    [ProducesResponseType<TicketDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDto>> CambiarEstado(int id, TicketCambiarEstadoDto dto, CancellationToken ct) =>
        Ok(await _flujo.CambiarEstadoAsync(id, dto, ct));

    /// <summary>PUT porque es idempotente: asignar dos veces la misma etiqueta no debería cambiar nada más (la segunda responde 409).</summary>
    [HttpPut("{id:int:min(1)}/etiquetas/{etiquetaId:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarEtiqueta(int id, int etiquetaId, CancellationToken ct)
    {
        await _flujo.AsignarEtiquetaAsync(id, etiquetaId, ct);
        return NoContent();
    }

    [HttpDelete("{id:int:min(1)}/etiquetas/{etiquetaId:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> QuitarEtiqueta(int id, int etiquetaId, CancellationToken ct)
    {
        await _flujo.QuitarEtiquetaAsync(id, etiquetaId, ct);
        return NoContent();
    }

    /// <summary>Descarga un CSV con los tickets que cumplan los mismos filtros de la lista.</summary>
    [HttpGet("exportar")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Exportar([FromQuery] TicketFiltro filtro, CancellationToken ct)
    {
        var bytes = await _flujo.ExportarCsvAsync(filtro, ct);
        return File(bytes, "text/csv; charset=utf-8", $"tickets-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv");
    }
}
