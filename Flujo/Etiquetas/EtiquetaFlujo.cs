using Abstracciones.Etiquetas;
using Abstracciones.Excepciones;

namespace Flujo.Etiquetas;

public class EtiquetaFlujo : IEtiquetaFlujo
{
    private readonly IEtiquetaDA _da;

    public EtiquetaFlujo(IEtiquetaDA da) => _da = da;

    public async Task<List<EtiquetaDto>> ListarAsync(CancellationToken ct)
    {
        var etiquetas = await _da.ListarAsync(ct);
        return etiquetas.Select(ADto).ToList();
    }

    public async Task<EtiquetaDto> ObtenerAsync(int id, CancellationToken ct)
    {
        var etiqueta = await _da.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("Etiqueta", id);
        return ADto(etiqueta);
    }

    public async Task<EtiquetaDto> CrearAsync(EtiquetaCrearDto dto, CancellationToken ct)
    {
        var nombre = dto.Nombre.Trim();

        if (await _da.ObtenerPorNombreAsync(nombre, ct) is not null)
            throw new ConflictoException($"Ya existe la etiqueta '{nombre}'.");

        var etiqueta = await _da.AgregarAsync(new Etiqueta
        {
            Nombre = nombre,
            Color = dto.Color.ToLowerInvariant()
        }, ct);

        return ADto(etiqueta);
    }

    private static EtiquetaDto ADto(Etiqueta e) => new(e.Id, e.Nombre, e.Color);
}
