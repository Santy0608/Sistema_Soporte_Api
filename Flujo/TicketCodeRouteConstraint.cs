using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Flujo
{
    public class TicketCodeRouteConstraint
    {
            
        private static readonly Regex _regex = new Regex(@"^TK-\d{4}-\d{4}$", RegexOptions.Compiled);

        public bool Match(
        HttpContext? httpContext,
        IRouter? route,
        string routeKey,
        RouteValueDictionary values,
        RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out var value) && value is string codigo)
            {
                return _regex.IsMatch(codigo);
            }
            return false;
        }


    }
}
