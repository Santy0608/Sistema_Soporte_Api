namespace Abstracciones.Etiquetas;

public class Etiqueta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#6c757d";

    public List<TicketEtiqueta> TicketEtiquetas { get; set; } = new();
}
