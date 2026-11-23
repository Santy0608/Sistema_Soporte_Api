using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Web.Models;
using Web.Servicios;

namespace Web.Filtros;

/// <summary>
/// Convierte los ApiException en una respuesta del MVC. Ahora además cierra el ciclo de seguridad:
/// 401 = el token venció o no sirve, se cierra la sesión y se manda al login; 403 = falta el rol.
/// </summary>
public class ApiExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext contexto)
    {
        if (contexto.Exception is not ApiException ex) return;   // otras excepciones no son de este filtro

        // El token vive 60 minutos. Si venció, el API responde 401: se cierra la sesión del MVC y se manda al login.
        if (ex.Codigo == 401)
        {
            await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            contexto.Result = new RedirectToActionResult("Ingresar", "Cuenta", null);
            contexto.ExceptionHandled = true;
            return;
        }

        // 403: el usuario está autenticado, pero su rol no puede hacer esto en el API.
        if (ex.Codigo == 403)
        {
            contexto.Result = new RedirectToActionResult("Denegado", "Cuenta", null);
            contexto.ExceptionHandled = true;
            return;
        }

        var modelo = new ErrorApiVm { Titulo = ex.Titulo, Detalle = ex.Message, Codigo = ex.Codigo };

        contexto.Result = new ViewResult
        {
            ViewName = "ErrorApi",
            ViewData = new ViewDataDictionary<ErrorApiVm>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = modelo,
            }
        };
        contexto.ExceptionHandled = true;
    }
}
