using System.Text.Json;

namespace Web.Servicios
{
    /// <summary>
    /// Eror devuelto por el API o que no es posible llamarlo
    /// </summary>
    public class ApiException : Exception
    {
        private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

        public int? Codigo { get; }
        public string Titulo { get; }

        public IReadOnlyDictionary<string, string[]> Errores { get; }

        public ApiException (int? codigo, string titulo, string detalle, IReadOnlyDictionary<string, string[]>? errores = null) : base(detalle)
        {
            Codigo = codigo;
            Titulo = titulo;
            Errores = errores ?? new Dictionary<string, string[]>();
        }
        
        public static async Task LanzarSiFallaAsync(HttpResponseMessage respuesta, CancellationToken ct)
        {
            if (respuesta.IsSuccessStatusCode) return;

            ProblemaApi problema = null;

            try
            {
                problema = await respuesta.Content.ReadFromJsonAsync<ProblemaApi>(OpcionesJson, ct);
            }

            catch (JsonException) { }
            catch (NotSupportedException) { }

            throw new ApiException(
                (int)respuesta.StatusCode,
                problema?.Title ?? respuesta.ReasonPhrase ?? "Error del API",
                problema?.Detail ?? $"El API respondio {(int)respuesta.StatusCode}",
                problema?.Errors);
        }

        internal sealed class ProblemaApi
        {
            public string? Title { get; set; }

            public string? Detail {  get; set; }

            public Dictionary<string, string[]>? Errors { get; set; }
        }
    }
}
