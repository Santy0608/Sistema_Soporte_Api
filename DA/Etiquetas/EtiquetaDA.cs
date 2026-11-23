using Abstracciones.Etiquetas;
using Abstracciones.Excepciones;
using DA.Comunes;
using DA.Contexto;
using Microsoft.EntityFrameworkCore;

namespace DA.Etiquetas;

public class EtiquetaDA : IEtiquetaDA
{
    private readonly SistemaSoporteContexto _ctx;

    public EtiquetaDA(SistemaSoporteContexto ctx) => _ctx = ctx;

    public Task<List<Etiqueta>> ListarAsync(CancellationToken ct) =>
        _ctx.Etiquetas.AsNoTracking().OrderBy(e => e.Nombre).ToListAsync(ct);

    public Task<Etiqueta?> ObtenerPorIdAsync(int id, CancellationToken ct) =>
        _ctx.Etiquetas.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<Etiqueta?> ObtenerPorNombreAsync(string nombre, CancellationToken ct) =>
        _ctx.Etiquetas.AsNoTracking().FirstOrDefaultAsync(e => e.Nombre == nombre, ct);

    public async Task<Etiqueta> AgregarAsync(Etiqueta etiqueta, CancellationToken ct)
    {
        _ctx.Etiquetas.Add(etiqueta);
        try
        {
            await _ctx.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ErroresSql.EsDuplicado(ex))
        {
            throw new ConflictoException($"Ya existe la etiqueta '{etiqueta.Nombre}'.");
        }
        return etiqueta;
    }
}
