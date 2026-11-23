using Abstracciones.Tickets;

namespace API.Restricciones;

/// <summary>
/// Restricción de ruta personalizada: {codigo:codigoTicket}.
/// Si el valor no cumple el formato, la RUTA no coincide y la petición ni siquiera llega al método (404).
/// Se registra en Program.cs con RouteOptions.ConstraintMap.
/// </summary>
public class CodigoTicketConstraint : IRouteConstraint
{
    public bool Match(HttpContext? httpContext, IRouter? route, string routeKey,
        RouteValueDictionary values, RouteDirection routeDirection)
    {
        return values.TryGetValue(routeKey, out var valor)
            && FormatoCodigoTicket.EsValido(Convert.ToString(valor, System.Globalization.CultureInfo.InvariantCulture));
    }
}
