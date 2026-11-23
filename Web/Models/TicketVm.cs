namespace Web.Models;

/// <summary>
/// ViewModel de una fila de la lista. Es la forma que ESTE proyecto necesita del JSON del API.
/// Se parece a TicketDto del API, pero es OTRA clase, escrita aquí a propósito.
/// El contrato entre los dos proyectos es el JSON, no una clase compartida: así el MVC puede
/// vivir en otra solución, otro repositorio o incluso otro servidor sin romper la arquitectura.
/// Prioridad y Estado son string (el API manda "EnProceso", no un número): no necesitamos el enum.
/// </summary>
public class TicketVm
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string? ClienteNombre { get; set; }
    public int? AgenteId { get; set; }
    public int Reaperturas { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public List<string> Etiquetas { get; set; } = new();
}
