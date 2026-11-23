using Abstracciones.Clientes;

namespace API.Endpoints;

public static class ClientesMinimalEndpoints
{
    /// <summary>
    /// Versión corregida de los ejemplos /min/clientes de clases anteriores.
    /// Error que tenían: llamaban a un método async SIN await; el resultado era un Task y no el cliente,
    /// y "creado.Id" devolvía el Id de la TAREA (Task tiene una propiedad Id), no el del cliente.
    /// </summary>
    public static IEndpointRouteBuilder MapClientesMinimal(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/min/clientes").WithTags("Clientes (Minimal API)");

        grupo.MapGet("/{id:int:min(1)}", async (int id, IClienteFlujo flujo, CancellationToken ct) =>
                TypedResults.Ok(await flujo.ObtenerAsync(id, ct)))
            .WithName("MinObtenerCliente")
            .Produces<ClienteDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async (ClienteCrearDto dto, IClienteFlujo flujo, CancellationToken ct) =>
            {
                // Minimal API no valida las anotaciones sola: se hace a mano y se responde 400 con el mismo formato.
                if (!ValidacionHelper.EsValido(dto, out var errores))
                    return Results.ValidationProblem(errores);

                var creado = await flujo.CrearAsync(dto, ct); // OJO al await
                return Results.Created($"/min/clientes/{creado.Id}", creado);
            })
            .WithName("MinCrearCliente")
            .Produces<ClienteDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
