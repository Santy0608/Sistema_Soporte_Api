using Abstracciones.Excepciones;
using Abstracciones.Seguridad;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Errores;

/// <summary>
/// Único lugar donde las excepciones de negocio se convierten en códigos HTTP.
/// Flujo no sabe de HTTP: lanza NoEncontradoException; aquí se vuelve 404 application/problem+json (RFC 9457).
/// Por eso los controladores quedan sin try/catch y sin ifs de error.
///   NoEncontrado -> 404 | Conflicto -> 409 | ValidacionNegocio -> 422 | No Autorizado -> 401 | cualquier otra -> 500 sin filtrar detalles internos.
/// </summary> 
public class ManejadorExcepcionesGlobal : IExceptionHandler
{
    private readonly IProblemDetailsService _problemas;
    private readonly ILogger<ManejadorExcepcionesGlobal> _logger;

    public ManejadorExcepcionesGlobal(IProblemDetailsService problemas, ILogger<ManejadorExcepcionesGlobal> logger)
    {
        _problemas = problemas;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        var (estado, titulo) = excepcion switch
        {
            NoEncontradoException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictoException => (StatusCodes.Status409Conflict, "Conflicto con el estado actual"),
            CredencialesInvalidasException => (StatusCodes.Status401Unauthorized, "Credenciales inválidas"),
            ValidacionNegocioException => (StatusCodes.Status422UnprocessableEntity, "No se puede procesar la solicitud"),
            _ => (0, string.Empty)
        };

        // Excepción inesperada: se registra y se devuelve false para que el middleware responda un 500 genérico.
        if (estado == 0)
        {
            _logger.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
            return false;
        }

        contexto.Response.StatusCode = estado;
        return await _problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            Exception = excepcion,
            ProblemDetails = new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                Detail = excepcion.Message
            }
        });
    }
}
