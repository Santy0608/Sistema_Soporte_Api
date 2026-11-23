using System.Text;
using Abstracciones.Tickets;

namespace Flujo.Tickets;

/// <summary>Genera el CSV a mano con StringBuilder (una de las tres formas de la presentación; las otras: CsvHelper y DataTable).</summary>
internal static class TicketCsv
{
    private static readonly char[] CaracteresEspeciales = { ',', '"', '\r', '\n' };

    private static readonly string[] Encabezados =
    {
        "Id", "Codigo", "Titulo", "Prioridad", "Estado", "Cliente", "AgenteId",
        "Reaperturas", "FechaCreacion", "Etiquetas"
    };

    public static byte[] Generar(IEnumerable<TicketDto> tickets)
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(',', Encabezados)).Append("\r\n");

        foreach (var t in tickets)
        {
            var campos = new[]
            {
                t.Id.ToString(),
                t.Codigo,
                t.Titulo,
                t.Prioridad.ToString(),
                t.Estado.ToString(),
                t.ClienteNombre ?? t.ClienteId.ToString(),
                t.AgenteId?.ToString() ?? "",
                t.Reaperturas.ToString(),
                t.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss"),
                string.Join(" | ", t.Etiquetas)
            };
            sb.Append(string.Join(',', campos.Select(Escapar))).Append("\r\n");
        }

        // UTF-8 CON BOM: sin él Excel abre el archivo en ANSI y las tildes salen rotas.
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return encoding.GetPreamble().Concat(encoding.GetBytes(sb.ToString())).ToArray();
    }

    /// <summary>
    /// 1) Si el campo trae coma, comillas o salto de línea, va entre comillas y las comillas se duplican.
    /// 2) Defensa contra "CSV injection": si empieza con = + - @ Excel lo ejecutaría como fórmula; se antepone una comilla simple.
    /// </summary>
    internal static string Escapar(string valor)
    {
        if (valor.Length > 0 && "=+-@".Contains(valor[0]))
            valor = "'" + valor;

        if (valor.IndexOfAny(CaracteresEspeciales) >= 0)
            return "\"" + valor.Replace("\"", "\"\"") + "\"";

        return valor;
    }
}
