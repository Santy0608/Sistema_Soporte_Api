using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Servicios;

namespace Web.Controllers;

/// <summary>
/// Inicio y cierre de sesión del MVC. Pide el token al API y lo guarda DENTRO de la cookie de sesión.
/// </summary>
[AllowAnonymous]   // el login no puede exigir sesión: es justo donde se obtiene
public class CuentaController : Controller
{
    private readonly ICuentaApi _cuenta;

    public CuentaController(ICuentaApi cuenta) => _cuenta = cuenta;

    [HttpGet]
    public IActionResult Ingresar(string? returnUrl) => View(new LoginVm { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ingresar(LoginVm datos, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(datos);

        try
        {
            var sesion = await _cuenta.IniciarSesionAsync(datos, ct);

            // Estos claims son los de la COOKIE del MVC (distintos a los del token, aunque parecidos).
            // "api_token" es el pase para hablar con el API: lo lee TokenApiHandler en cada llamada.
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, sesion.Nombre),
                new(ClaimTypes.Email, sesion.Correo),
                new(ClaimTypes.Role, sesion.Rol),
                new("api_token", sesion.Token)
            };
            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                new AuthenticationProperties { ExpiresUtc = sesion.ExpiraEnUtc, IsPersistent = false });

            // IsLocalUrl evita que alguien arme un enlace que, después del login, te mande a un sitio malicioso.
            return Url.IsLocalUrl(datos.ReturnUrl) ? Redirect(datos.ReturnUrl!) : RedirectToAction("Index", "Tickets");
        }
        catch (ApiException ex) when (ex.Codigo == 401)
        {
            // Mismo mensaje genérico que el API: no se revela si falló el correo o la clave.
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(datos);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Ingresar));
    }

    /// <summary>Se muestra cuando el usuario está autenticado pero su rol no alcanza (403).</summary>
    public IActionResult Denegado() => View();
}
