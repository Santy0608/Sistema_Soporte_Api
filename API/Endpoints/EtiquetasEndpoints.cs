using Abstracciones.Etiquetas;

namespace API.Endpoints;

public static class EtiquetasEndpoints
{
    /// <summary>
    /// El módulo Etiquetas se expone SOLO con Minimal API (los de Clientes y Comentarios usan Controllers).
    /// </summary>
    public static IEndpointRouteBuilder MapEtiquetas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/etiquetas").WithTags("Etiquetas");

        grupo.MapGet("/", async (IEtiquetaFlujo flujo, CancellationToken ct) =>
                TypedResults.Ok(await flujo.ListarAsync(ct)))
            .WithName("ListarEtiquetas")
            .Produces<List<EtiquetaDto>>(StatusCodes.Status200OK);

        grupo.MapGet("/{id:int:min(1)}", async (int id, IEtiquetaFlujo flujo, CancellationToken ct) =>
                TypedResults.Ok(await flujo.ObtenerAsync(id, ct)))
            .WithName("ObtenerEtiqueta")
            .Produces<EtiquetaDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async (EtiquetaCrearDto dto, IEtiquetaFlujo flujo, CancellationToken ct) =>
            {
                if (!ValidacionHelper.EsValido(dto, out var errores))
                    return Results.ValidationProblem(errores);

                var creada = await flujo.CrearAsync(dto, ct);
                return Results.Created($"/api/etiquetas/{creada.Id}", creada);
            })
            .WithName("CrearEtiqueta")
            .Produces<EtiquetaDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
