using System;
using System.Collections.Generic;
using System.Text;

namespace Abtracciones.Modelos
{

    public enum PrioridadTicket { Baja, Media, Alta, Critica }
    public enum EstadoTicket { Abierto, EnProceso, Resuelto, Cerrado }

    public record ResumenTicketsDto(
        int Total,
        Dictionary<string, int> PorEstado,
        Dictionary<string, int> PorPrioridad
    );

    public class Ticket
    {

        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public PrioridadTicket Prioridad { get; set; }
        public EstadoTicket Estado { get; set; }
        public int ClienteId { get; set; }
        public int? AgenteId { get; set; }
        public int Reaperturas { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
