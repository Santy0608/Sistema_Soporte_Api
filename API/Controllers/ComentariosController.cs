using Abstracciones.Comentarios;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>Recurso anidado: los comentarios no existen sin su ticket, por eso la ruta cuelga de /api/tickets/{ticketId}.</summary>
[ApiController]
[Route("api/tickets/{ticketId:int:min(1)}/comentarios")]
public class ComentariosController : ControllerBase
{
    private readonly IComentarioFlujo _flujo;

    public ComentariosController(IComentarioFlujo flujo) => _flujo = flujo;

    [HttpGet]
    [ProducesResponseType<List<ComentarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ComentarioDto>>> Listar(int ticketId, CancellationToken ct) =>
        Ok(await _flujo.ListarAsync(ticketId, ct));

    [HttpPost]
    [ProducesResponseType<ComentarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ComentarioDto>> Agregar(int ticketId, ComentarioCrearDto dto, CancellationToken ct)
    {
        var creado = await _flujo.AgregarAsync(ticketId, dto, ct);
        // El Location apunta a la lista de comentarios del ticket (no hay GET individual de comentario).
        return Created($"/api/tickets/{ticketId}/comentarios", creado);
    }
}
