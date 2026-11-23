using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Web.Filtros;
using Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(opciones => opciones.Filters.Add<ApiExceptionFilter>());

// ---------- Seguridad del MVC: la cookie guarda la sesión y dentro lleva el token del API ----------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Cuenta/Ingresar";          // sin sesión, el MVC manda aquí
        o.AccessDeniedPath = "/Cuenta/Denegado";   // con sesión pero sin el rol, manda aquí
        o.ExpireTimeSpan = TimeSpan.FromMinutes(60);   // igual que Jwt:Minutos del API
        o.SlidingExpiration = false;
        o.Cookie.HttpOnly = true;                  // JavaScript no puede leer la cookie
    });

// Todo el MVC exige sesión, salvo lo marcado con [AllowAnonymous] (el controlador de cuenta).
builder.Services.AddAuthorization(o =>
    o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddHttpContextAccessor();         // para que TokenApiHandler vea la sesión actual
builder.Services.AddTransient<TokenApiHandler>();

// ---------- Clientes HTTP hacia el API ----------
// Cliente de tickets: SÍ lleva el handler, así cada llamada va con "Authorization: Bearer (token)".
builder.Services.AddHttpClient<ITicketsApi, TicketsApi>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenApiHandler>();

// Cliente de cuenta: NO lleva el handler (para hacer login todavía no hay token).
builder.Services.AddHttpClient<ICuentaApi, CuentaApi>(ConfigurarCliente);

// Cliente de etiquetas: lleva TokenApiHandler para enviar el Bearer token al API.
builder.Services.AddHttpClient<IEtiquetasApi, EtiquetasApi>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenApiHandler>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// El orden importa: primero ¿quién eres? (autenticación), después ¿puedes entrar? (autorización).
app.UseAuthentication();
app.UseAuthorization();

// La página de inicio es la lista de tickets.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=Index}/{id?}");

app.Run();

// La URL del API sale de Api:UrlBase (appsettings o variable Api__UrlBase). Nunca new HttpClient() a mano.
void ConfigurarCliente(HttpClient cliente)
{
    var urlBase = builder.Configuration["Api:UrlBase"]
        ?? throw new InvalidOperationException("Falta Api:UrlBase");
    cliente.BaseAddress = new Uri(urlBase);
    cliente.Timeout = TimeSpan.FromSeconds(60);
}
