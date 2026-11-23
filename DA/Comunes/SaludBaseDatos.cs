using Abstracciones.Comunes;
using DA.Contexto;
using Microsoft.EntityFrameworkCore;

namespace DA.Comunes;

/// <summary>Implementación EF Core del chequeo de salud. Solo DA conoce el DbContext.</summary>
public class SaludBaseDatos : ISaludBaseDatos
{
    private readonly SistemaSoporteContexto _contexto;

    public SaludBaseDatos(SistemaSoporteContexto contexto) => _contexto = contexto;

    // CanConnectAsync NO lanza excepción si falla: devuelve false. Ideal para un endpoint de salud.
    // Con Azure SQL gratis (serverless) la primera llamada después de una pausa puede tardar: es normal.
    public Task<bool> EstaDisponibleAsync(CancellationToken ct = default) =>
        _contexto.Database.CanConnectAsync(ct);
}
