namespace Abstracciones.Etiquetas;

public interface IEtiquetaFlujo
{
    Task<List<EtiquetaDto>> ListarAsync(CancellationToken ct);
    Task<EtiquetaDto> ObtenerAsync(int id, CancellationToken ct);
    Task<EtiquetaDto> CrearAsync(EtiquetaCrearDto dto, CancellationToken ct);
}
