using Abstracciones.Clientes;
using Abstracciones.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteFlujo _flujo;

    public ClientesController(IClienteFlujo flujo) => _flujo = flujo;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClienteDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> Listar([FromQuery] ClienteFiltro filtro, CancellationToken ct)
    {
        var resultado = await _flujo.ListarAsync(filtro, ct);
        Response.Headers["X-Total-Count"] = resultado.Total.ToString();
        Response.Headers["X-Page"] = resultado.Pagina.ToString();
        Response.Headers["X-Page-Size"] = resultado.Tamano.ToString();
        return Ok(resultado.Items);
    }

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await _flujo.ObtenerAsync(id, ct));

    [HttpPost]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteCrearDto dto, CancellationToken ct)
    {
        var creado = await _flujo.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, ClienteActualizarDto dto, CancellationToken ct)
    {
        await _flujo.ActualizarAsync(id, dto, ct);
        return NoContent();
    }

    // Solo el rol Admin puede borrar. Con el token de un Agente el resultado es 403 (sé quién eres, pero no puedes).
    // Para varios roles: [Authorize(Roles = "Admin,Agente")].
    [Authorize(Roles = nameof(RolUsuario.Admin))]
    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _flujo.EliminarAsync(id, ct);
        return NoContent();
    }

    /// <summary>Ejemplo de procedimiento almacenado con parámetros. Sustituye al antiguo alta por SP.</summary>
    [HttpGet("buscar-sp")]
    [ProducesResponseType<List<ClienteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClienteDto>>> BuscarSp([FromQuery] string? nombre, CancellationToken ct) =>
        Ok(await _flujo.BuscarPorNombreSpAsync(nombre, ct));
}
