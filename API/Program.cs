using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using API.Endpoints;
using API.Errores;
using API.Restricciones;
using API.Seguridad;
using Abstracciones.Comunes;
using Abstracciones.Seguridad;
using DA;
using DA.Semilla;
using Flujo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Servicios ----------
builder.Services
    .AddControllers()
    // Los enums viajan como texto ("EnProceso") en el JSON de los controladores.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Lo mismo para Minimal API (usa otra configuración de JSON).
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Restricción de ruta personalizada: {codigo:codigoTicket}
builder.Services.Configure<RouteOptions>(o => o.ConstraintMap.Add("codigoTicket", typeof(CodigoTicketConstraint)));

// Errores estándar (RFC 9457, application/problem+json) y manejador global de excepciones de negocio.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepcionesGlobal>();

builder.Services.AddOpenApi();

// Cada capa registra lo suyo. El API solo orquesta (es la "raíz de composición").
var cadenaConexion = builder.Configuration.GetConnectionString("SistemaSoporte")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:SistemaSoporte (appsettings o User Secrets).");

// "Memoria" = Práctica 1 (lista en memoria). "Sql" = Entity Framework Core (Semana 8 en adelante).
var ticketsEnMemoria = string.Equals(
    builder.Configuration["Almacenamiento:Tickets"], "Memoria", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDatos(cadenaConexion, ticketsEnMemoria);
builder.Services.AddFlujo();

// ---------- Seguridad: JWT y roles (Semana 11) ----------
builder.Services.Configure<OpcionesJwt>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();

var jwt = builder.Configuration.GetSection("Jwt").Get<OpcionesJwt>() ?? new OpcionesJwt();

// Interruptor Seguridad:Exigir. En false el API sigue abierto (el MVC aún no tiene login);
// en true TODO endpoint pide token, salvo los marcados con AllowAnonymous.
var exigirSeguridad = builder.Configuration.GetValue<bool>("Seguridad:Exigir");

// Si la seguridad está encendida y no hay clave, el API NO arranca. Mejor fallar fuerte
// al inicio que descubrir en producción que cualquiera puede entrar.
if (exigirSeguridad && jwt.Clave.Length < 32)
    throw new InvalidOperationException("Seguridad:Exigir está activo pero falta Jwt:Clave (mínimo 32 caracteres).");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           ValidIssuer = jwt.Emisor,
        ValidateAudience = true,         ValidAudience = jwt.Audiencia,
        ValidateLifetime = true,         ClockSkew = TimeSpan.FromMinutes(1),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = jwt.Clave.Length >= 32
            ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave))
            : null,
        RoleClaimType = ClaimTypes.Role,   // de qué claim sale el rol
        NameClaimType = ClaimTypes.Name
    });

builder.Services.AddAuthorization(o =>
{
    if (exigirSeguridad)
        o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var app = builder.Build();

// ---------- Migración automática (Semana 9) ----------
// Interruptor Base:MigrarAlIniciar en appsettings (true por defecto en el curso).
// Con LocalDB: clonar, correr y la base se crea sola, con el nombre que diga la cadena de conexión.
// En Azure: la variable de la Web App Base__MigrarAlIniciar decide si migra o no.
// Va ANTES del comando --seed para que la semilla siempre encuentre las tablas.
if (app.Configuration.GetValue<bool>("Base:MigrarAlIniciar"))
{
    try
    {
        await app.Services.AplicarMigracionesAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Fallo migracion al iniciar.Revisar String de conn");
    }
}
    

// ---------- Comando de semilla: dotnet run --project API -- --seed [--yes] ----------
if (args.Contains("--seed"))
{
    using var alcance = app.Services.CreateScope();
    // La clave inicial de los usuarios sale de configuración (User Secrets o variable Semilla__ClaveInicial), nunca del código.
    var claveInicial = app.Configuration["Semilla:ClaveInicial"];
    await alcance.ServiceProvider.GetRequiredService<SemillaDatos>().EjecutarAsync(args.Contains("--yes"), claveInicial);
    return;
}

// ---------- Pipeline ----------
app.UseExceptionHandler();   // atrapa excepciones y las pasa al ManejadorExcepcionesGlobal
app.UseStatusCodePages();    // convierte respuestas vacías (404, 405...) en problem+json

// Documentación (OpenAPI + Scalar): siempre en Development; en Azure solo si Documentacion__Habilitar = true.
// Un API público no debería regalar su documentación por defecto, pero en clase
// la prendemos a propósito para probar el API desplegado desde el navegador.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Documentacion:Habilitar"))
{
    // AllowAnonymous: con Seguridad:Exigir = true la documentación sigue abierta para poder probar el login.
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

// Endpoint de salud (Semana 9): 200 si la base contesta, 503 si no. Azure y las pruebas lo usan.
// Es lo PRIMERO que se abre después de un despliegue: si /salud da 200, la app y la base están vivas.
app.MapGet("/salud", async (ISaludBaseDatos salud, CancellationToken ct) =>
        await salud.EstaDisponibleAsync(ct)
            ? Results.Ok(new { estado = "ok", baseDeDatos = "disponible" })
            : Results.Json(new { estado = "degradado", baseDeDatos = "no disponible" },
                statusCode: StatusCodes.Status503ServiceUnavailable))
    .WithTags("Operación")
    .AllowAnonymous();   // el chequeo de salud nunca pide token (Azure lo consulta sin credenciales)

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();   // ¿quién eres? (lee el token)
app.UseAuthorization();    // ¿puedes entrar? (revisa roles). SIEMPRE después de UseAuthentication.

app.MapControllers();
app.MapClientesMinimal();
app.MapTicketsMinimal();
app.MapEtiquetas();

app.Run();
