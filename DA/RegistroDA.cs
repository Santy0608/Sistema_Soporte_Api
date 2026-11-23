using Abstracciones.Clientes;
using Abstracciones.Comentarios;
using Abstracciones.Comunes;
using Abstracciones.Etiquetas;
using Abstracciones.Seguridad;
using Abstracciones.Tickets;
using DA.Clientes;
using DA.Comentarios;
using DA.Comunes;
using DA.Contexto;
using DA.Etiquetas;
using DA.Seguridad;
using DA.Semilla;
using DA.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DA;

/// <summary>
/// La capa DA registra SUS propias dependencias. El API solo llama a AddDatos(...).
/// Así, agregar un módulo nuevo en DA no obliga a tocar el API.
/// </summary>
public static class RegistroDA
{
    public static IServiceCollection AddDatos(this IServiceCollection servicios, string cadenaConexion, bool ticketsEnMemoria)
    {
        servicios.AddDbContext<SistemaSoporteContexto>(o => o.UseSqlServer(
            cadenaConexion,
            // Semana 9: Azure SQL gratis es "serverless" y se PAUSA tras 15 minutos sin uso.
            // La primera conexión después de la pausa puede fallar o tardar. Con esto EF reintenta
            // (con esperas crecientes) en lugar de devolver un error al usuario.
            sql => sql.EnableRetryOnFailure(maxRetryCount: 5)));

        // Módulo Clientes
        servicios.AddScoped<IClienteDA, ClienteDA>();

        // Módulo Etiquetas
        servicios.AddScoped<IEtiquetaDA, EtiquetaDA>();

        // Módulo Comentarios
        servicios.AddScoped<IComentarioDA, ComentarioDA>();

        // Módulo Tickets (dos implementaciones intercambiables)
        if (ticketsEnMemoria)
            servicios.AddSingleton<ITicketDA, TicketDAMemoria>();
        else
            servicios.AddScoped<ITicketDA, TicketDA>();

        // Chequeo de salud para el endpoint /salud (Semana 9)
        servicios.AddScoped<ISaludBaseDatos, SaludBaseDatos>();

        servicios.AddScoped<SemillaDatos>();

        servicios.AddScoped<IUsuarioDA, UsuarioDA>();

        return servicios;
    }

    /// <summary>
    /// Semana 9: aplica las migraciones pendientes al arrancar (si la base no existe, EF la crea con el nombre
    /// que diga la cadena de conexión). Program.cs la llama solo si Base:MigrarAlIniciar = true.
    /// Sirve igual con LocalDB, otro nombre de base de datos o Azure SQL: solo cambia la cadena de conexión.
    /// En la Semana 8 dijimos que NO migrábamos al arrancar porque con User Secrets la
    /// conexión puede apuntar a Azure sin que nadie lo note. Hoy lo cambiamos con dos protecciones:
    /// (1) hay un interruptor en la configuración y (2) SIEMPRE se imprime a qué servidor y base se va a migrar.
    /// En una empresa grande esto se haría en el pipeline (migration bundle); para el curso, así es más simple.
    /// </summary>
    public static async Task AplicarMigracionesAsync(this IServiceProvider servicios, CancellationToken ct = default)
    {
        // El DbContext es Scoped; fuera de una petición HTTP hay que crear el alcance a mano.
        using var alcance = servicios.CreateScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<SistemaSoporteContexto>();
        var bitacora = alcance.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migraciones");

        var conexion = contexto.Database.GetDbConnection();
        bitacora.LogInformation("Aplicando migraciones -> Servidor: {Servidor} | Base: {Base}", conexion.DataSource, conexion.Database);

        // Si no hay migraciones generadas en el proyecto, esto NO crea ninguna tabla y no avisa:
        // por eso el README exige generar y subir la carpeta Migrations.
        var pendientes = (await contexto.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pendientes.Count == 0)
            bitacora.LogInformation("Sin migraciones pendientes.");
        else
            bitacora.LogInformation("Migraciones pendientes: {Lista}", string.Join(", ", pendientes));

        await contexto.Database.MigrateAsync(ct);
    }
}
