using Abtracciones.Interfaces;
using Abtracciones.Interfaces.Tickets;
using Abtracciones.Modelos;
using DA;
using DA.Contexto;
using Flujo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//Inyeccion de dependencias
builder.Services.AddScoped<IClienteDA, ClienteDA>();
builder.Services.AddScoped<IClienteFlujo, ClienteFlujo>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddProblemDetails();

var cadenaConexion = builder.Configuration.GetConnectionString("SistemaSoporte") ?? 
    throw new InvalidOperationException("Falta la cadena de conexión 'Sistema Soporte' en la configuración");

builder.Services.AddDbContext<SistemaSoporteContexto>(opciones =>
    opciones.UseSqlServer(cadenaConexion));

//Restricción de ruta personalizada para validar el formato del código de ticket
builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("ticketcode", typeof(TicketCodeRouteConstraint));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/min/clientes", (IClienteFlujo flujo) => Results.Ok(flujo.Obtener()));

app.MapGet("/min/clientes/{id}", (int id, IClienteFlujo flujo) =>
{    
    var cliente = flujo.ObtenerPorId(id);
    return cliente is null ? Results.Ok(cliente) : Results.NotFound();
    
});



app.MapPost("/min/clientes", (Cliente cliente, IClienteFlujo flujo) =>
{
    var resultado = flujo.Agregar(cliente);
    return Results.Created($"/min/clientes/{resultado.Id}", resultado);
});





app.MapGet("/min/tickets/resumen", ([FromServices] ITicketDa ticketDA) =>
{
    var resumen = ticketDA.ObtenerResumen();
    return Results.Ok(resumen); // 200 OK con estructura { total, porEstado, porPrioridad }
});


app.MapPost("/min/tickets/{id:int}/reabrir", (int id, [FromServices] ITicketDa ticketDA) =>
{
    try
    {
        var ticket = ticketDA.ReabrirTicket(id);
        return Results.Ok(ticket);
    }
    catch (NotFoundException ex)
    {
        // 404 - ProblemDetails
        return Results.Problem(
            title: "Recurso no encontrado",
            detail: ex.Message,
            statusCode: StatusCodes.Status404NotFound
        );
    }
    catch (ReglaNegocioException ex)
    {
        // 409 - ProblemDetails por regla de negocio
        return Results.Problem(
            title: "Conflicto de regla de negocio",
            detail: ex.Message,
            statusCode: StatusCodes.Status409Conflict
        );
    }
});


//Caché Condicional
app.MapGet("/api/tickets/{id:int}", (int id, ITicketDa ticketDA, HttpContext httpContext) =>
{
    var ticket = ticketDA.ObtenerPorId(id);

    if (ticket == null)
    {
        return Results.NotFound();
    }

    // 1. Calcular un ETag estable basado en el ID y su FechaActualizacion
    string etag = $"\"tk-{ticket.Id}-{ticket.FechaActualizacion.Ticks}\"";

    // 2. Comprobar si el cliente envió el header If-None-Match y coincide con el ETag actual
    if (httpContext.Request.Headers.TryGetValue("If-None-Match", out var incomingEtag) && incomingEtag == etag)
    {
        // 3. Si no ha cambiado, retornamos 304 sin cuerpo de respuesta
        return Results.StatusCode(StatusCodes.Status304NotModified);
    }

    // 4. Si es nuevo o cambió, enviamos el ETag en los headers de respuesta junto con el 200 OK
    httpContext.Response.Headers.Append("ETag", etag);

    return Results.Ok(ticket);
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//app.MapGet("/min/clientes/buscar", (string? nombre, int pagina, IClienteFlujo flujo) =>
//{
//    var clientes = flujo.Obtener().AsEnumerable();

//FIltro opcional por nombre , sin distinguir mayusculas
//   if (!string.IsNullOrWhiteSpace(nombre))
//   {
//     clientes = clientes.Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));

//Paginacion simple de 5 en 5; Math.Math protege contra páginas menos que 1.
//    return Results.Ok(clientes.Skip((Math.Max(pagina, 1) - 1) * 5).Take(5));
// }

//  return Results.Ok(clientes);
// });


//nos permite que "/" sirva wwwroot/index.html sin escribir el nombre del archivo
app.UseDefaultFiles();

//Habilita brindar (servir) al servir los archivos que viven en wwwrroot (WebRoot)
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
