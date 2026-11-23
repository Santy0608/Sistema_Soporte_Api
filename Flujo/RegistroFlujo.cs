using Abstracciones.Clientes;
using Abstracciones.Comentarios;
using Abstracciones.Etiquetas;
using Abstracciones.Tickets;
using Abstracciones.Seguridad;
using Flujo.Clientes;
using Flujo.Comentarios;
using Flujo.Etiquetas;
using Flujo.Tickets;
using Flujo.Seguridad;

using Microsoft.Extensions.DependencyInjection;

namespace Flujo;

/// <summary>La capa Flujo registra sus propios servicios. Es Scoped porque depende de los DA, que son Scoped.</summary>
public static class RegistroFlujo
{
    public static IServiceCollection AddFlujo(this IServiceCollection servicios)
    {
        servicios.AddSingleton(TimeProvider.System);

        servicios.AddScoped<IClienteFlujo, ClienteFlujo>();
        servicios.AddScoped<IEtiquetaFlujo, EtiquetaFlujo>();
        servicios.AddScoped<IComentarioFlujo, ComentarioFlujo>();
        servicios.AddScoped<ITicketFlujo, TicketFlujo>();
        servicios.AddScoped<IAutenticacionFlujo, AutenticacionFlujo>();
        return servicios;
    }
}
