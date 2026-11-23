using Abstracciones.Tickets;

namespace Abstracciones.Clientes;

/// <summary>Entidad de dominio. Sin atributos de base de datos: eso se configura con Fluent API en DA.</summary>
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Empresa { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Navegación 1-N: un cliente tiene muchos tickets.
    public List<Ticket> Tickets { get; set; } = new();
}
