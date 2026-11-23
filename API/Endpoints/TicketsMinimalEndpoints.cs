using Abstracciones.Tickets;

namespace API.Endpoints;

public static class TicketsMinimalEndpoints
{
    /// <summary>
    /// Endpoints de tickets con Minimal API. Misma regla que los controladores: SIN lógica de negocio,
    /// solo recibir, delegar a Flujo y devolver. Los errores los traduce el manejador global.
    /// </summary>
    public static IEndpointRouteBuilder MapTicketsMinimal(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/min/tickets").WithTags("Tickets (Minimal API)");

        grupo.MapGet("/resumen", async (ITicketFlujo flujo, CancellationToken ct) =>
                TypedResults.Ok(await flujo.ResumenAsync(ct)))
            .WithName("ResumenTickets")
            .WithSummary("Cantidad de tickets por estado y por prioridad.")
            .Produces<TicketResumenDto>(StatusCodes.Status200OK);

        grupo.MapPost("/{id:int:min(1)}/reabrir", async (int id, ITicketFlujo flujo, CancellationToken ct) =>
                TypedResults.Ok(await flujo.ReabrirAsync(id, ct)))
            .WithName("ReabrirTicket")
            .WithSummary("Pasa un ticket Resuelto a EnProceso e incrementa Reaperturas.")
            .Produces<TicketDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
