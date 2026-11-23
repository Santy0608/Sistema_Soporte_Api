using System.Net.Http.Headers;

namespace Web.Servicios;

/// <summary>
/// Antes de que salga cada petición hacia el API, le pega "Authorization: Bearer (token)"
/// con el token que quedó guardado en la cookie de sesión del usuario.
/// </summary>
public class TokenApiHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _contexto;

    public TokenApiHandler(IHttpContextAccessor contexto) => _contexto = contexto;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken ct)
    {
        // Los handlers se reutilizan entre peticiones y no pueden recibir nada Scoped, por eso se usa
        // IHttpContextAccessor para consultar la sesión de la petición actual.
        var token = _contexto.HttpContext?.User.FindFirst("api_token")?.Value;

        if (!string.IsNullOrEmpty(token))
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(peticion, ct);
    }
}
