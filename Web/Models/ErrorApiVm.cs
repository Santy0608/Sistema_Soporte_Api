namespace Web.Models;

/// <summary>Lo que mostramos cuando el API no contesta o responde con un error.</summary>
public class ErrorApiVm
{
    public string Titulo { get; set; } = "No se pudo consultar el API";
    public string Detalle { get; set; } = string.Empty;
    public int? Codigo { get; set; }
}
