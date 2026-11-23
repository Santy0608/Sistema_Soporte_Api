using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Servicios;

namespace Web.Controllers;

/// <summary>
/// Controlador MVC de tickets: lista, detalle, creación y cambio de estado. Todo el HTTP vive en ITicketsApi;
/// los errores del API los atrapa ApiExceptionFilter. Aquí solo se decide qué respuesta darle al navegador.
/// </summary>
public class TicketsController : Controller
{
    private const int TamanoPagina = 10;

    private readonly ITicketsApi _api;

    public TicketsController(ITicketsApi api) => _api = api;

    // ---------------- Lectura ----------------

    public async Task<IActionResult> Index(string? estado, string? q, int pagina = 1, CancellationToken ct = default) =>
        View(await _api.ListarAsync(estado, q, Math.Max(pagina, 1), TamanoPagina, ct));

    public async Task<IActionResult> Detalle(int id, CancellationToken ct = default)
    {
        var ticket = await _api.ObtenerAsync(id, ct);
        return ticket is null ? NotFound() : View(ticket);
    }

    // ---------------- Tipos de resultado (demostración) ----------------

    // ContentResult: texto plano sin vista.
    public IActionResult Version() => Content("Sistema de Soporte, Web v1", "text/plain");

    // JsonResult: pregunta al API si está vivo y lo reenvía en JSON.
    // [FromServices] pide el servicio solo para ESTA acción, sin ensuciar el constructor.
    public async Task<IActionResult> Estado([FromServices] ITicketsApi api, CancellationToken ct)
    {
        var lista = await api.ListarAsync(null, null, 1, 1, ct);
        return Json(new { apiVivo = true, totalTickets = lista.Total });
    }

    // FileResult: exporta el CSV que el API ya sabe generar. Solo el rol Admin.
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Exportar(string? estado, string? q, CancellationToken ct)
    {
        var bytes = await _api.ExportarCsvAsync(estado, q, ct);
        return File(bytes, "text/csv; charset=utf-8", $"tickets-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    // ---------------- Formularios ----------------

    [HttpGet]
    public IActionResult Crear() => View(new TicketCrearVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(TicketCrearVm datos, CancellationToken ct)
    {
        // 1) El propio MVC valida las anotaciones del ViewModel.
        if (!ModelState.IsValid) return View(datos);

        try
        {
            var creado = await _api.CrearAsync(datos, ct);
            TempData["Exito"] = $"Ticket {creado.Codigo} creado.";

            // Post-Redirect-Get: si se devolviera View(...) aquí, F5 reenviaría el formulario y crearía
            // el ticket dos veces. Con la redirección, F5 solo repite un GET.
            return RedirectToAction(nameof(Detalle), new { id = creado.Id });
        }
        // 2) Si el API rechaza (400, 409 o 422), el mensaje vuelve al formulario.
        catch (ApiException ex) when (ex.Codigo is 400 or 409 or 422)
        {
            AgregarErroresDelApi(ex);
            return View(datos);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string estado, CancellationToken ct)
    {
        try
        {
            await _api.CambiarEstadoAsync(id, estado, null, ct);
            TempData["Exito"] = "Estado actualizado.";
        }
        catch (ApiException ex) when (ex.Codigo is 400 or 409 or 422)
        {
            // La regla "no se puede reabrir un ticket cerrado" es del API. El MVC solo muestra lo que el API diga.
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Detalle), new { id });
    }

    private void AgregarErroresDelApi(ApiException ex)
    {
        // Sin errores por campo (409 o 422): un mensaje general. Con errores por campo (400): uno por campo.
        if (ex.Errores.Count == 0) ModelState.AddModelError(string.Empty, ex.Message);
        foreach (var (campo, mensajes) in ex.Errores)
            foreach (var mensaje in mensajes)
                ModelState.AddModelError(campo, mensaje);
    }
}
