namespace Abstracciones.Etiquetas;

public interface IEtiquetaDA
{
    Task<List<Etiqueta>> ListarAsync(CancellationToken ct);
    Task<Etiqueta?> ObtenerPorIdAsync(int id, CancellationToken ct);
    Task<Etiqueta?> ObtenerPorNombreAsync(string nombre, CancellationToken ct);
    Task<Etiqueta> AgregarAsync(Etiqueta etiqueta, CancellationToken ct);
}
