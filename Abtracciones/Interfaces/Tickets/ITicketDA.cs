using Abtracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abtracciones.Interfaces.Tickets
{
    public interface ITicketDa
    {

        List<Ticket> Obtener();

        (List<Ticket> Items, int TotalCount) ListarPaginado(
            EstadoTicket? estado,
            PrioridadTicket? prioridad,
            int? clienteId,
            DateTime? desde,
            DateTime? hasta,
            string? q,
            string? orden,
            int pagina = 1,
            int tamano = 10);

        Ticket ObtenerPorId(int id);

        Ticket ObtenerPorCodigo(String codigo);

        Ticket guardarTicket(Ticket ticket);

        Ticket ActualizarEstado(int id, EstadoTicket nuevoEstado, int? nuevoAgenteId, PrioridadTicket? nuevaPrioridad);
        ResumenTicketsDto ObtenerResumen();
        Ticket ReabrirTicket(int id);
    }
}
