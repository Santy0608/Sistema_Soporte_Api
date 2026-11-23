using System.Text.RegularExpressions;

namespace Abstracciones.Tickets;

/// <summary>
/// Formato del código del ticket (variante 0 de la Práctica 1): TK-AAAA-NNNN, ej. TK-2026-0007.
/// Vive en Abstracciones porque lo usan Flujo (para generarlo) y API (restricción de ruta).
/// </summary>
public static partial class FormatoCodigoTicket
{
    public const string Patron = @"^TK-\d{4}-\d{4}$";

    [GeneratedRegex(Patron)]
    private static partial Regex Expresion();

    public static bool EsValido(string? codigo) => codigo is not null && Expresion().IsMatch(codigo);

    public static string Generar(int anio, int secuencia) => $"TK-{anio:D4}-{secuencia:D4}";
}
