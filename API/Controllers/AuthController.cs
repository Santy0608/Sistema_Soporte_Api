using Abstracciones.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Único endpoint público de seguridad: recibe correo y password y devuelve el token.
/// Sin try/catch: una CredencialesInvalidasException la traduce el ManejadorExcepcionesGlobal a 401.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]   // el login no puede pedir token: es justo donde se obtiene
public class AuthController : ControllerBase
{
    private readonly IAutenticacionFlujo _flujo;

    public AuthController(IAutenticacionFlujo flujo) => _flujo = flujo;

    [HttpPost("login")]
    [ProducesResponseType<SesionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SesionDto>> Login(LoginDto dto, CancellationToken ct) =>
        Ok(await _flujo.IniciarSesionAsync(dto, ct));
}
