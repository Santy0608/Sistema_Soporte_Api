namespace Web.Controllers
{
    using global::Web.Servicios;
    using Microsoft.AspNetCore.Mvc;
    using static global::Web.Models.Etiquetas;

    namespace Web.Controllers
    {
        public class EtiquetasController : Controller
        {
            private readonly EtiquetasApi _api;

            public EtiquetasController(EtiquetasApi api) => _api = api;

            // GET: /Etiquetas
            public async Task<IActionResult> Index(CancellationToken ct)
            {
                List<EtiquetaVm> etiquetas = await _api.ListarAsync(ct);
                return View(etiquetas);
            }

            // GET: /Etiquetas/Details/5
            public async Task<IActionResult> Details(int id, CancellationToken ct)
            {
                EtiquetaVm? etiqueta = await _api.ObtenerAsync(id, ct);
                if (etiqueta is null)
                    return NotFound();

                return View(etiqueta);
            }

            // GET: /Etiquetas/Create
            public IActionResult Create()
            {
                return View(new EtiquetaCrearVm());
            }

            // POST: /Etiquetas/Create (Post-Redirect-Get)
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(EtiquetaCrearVm vm, CancellationToken ct)
            {
                if (!ModelState.IsValid)
                    return View(vm);

                var (ok, _, error) = await _api.CrearAsync(vm, ct);

                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, error ?? "No se pudo crear la etiqueta.");
                    return View(vm);
                }

                TempData["MensajeExito"] = "Etiqueta creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
