using static Web.Models.Etiquetas;

namespace Web.Servicios
{
    public interface IEtiquetasApi
    {
        Task<List<EtiquetaVm>> ListarAsync(CancellationToken ct = default);
        Task<EtiquetaVm?> ObtenerAsync(int id, CancellationToken ct = default);
        Task<(bool Ok, EtiquetaVm? Creada, string? Error)> CrearAsync(
            EtiquetaCrearVm vm, CancellationToken ct = default);
    }
}
