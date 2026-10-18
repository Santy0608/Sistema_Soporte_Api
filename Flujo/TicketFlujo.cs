using Abtracciones.Interfaces.Tickets;
using Abtracciones.Modelos;
using DA;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class TicketFlujo : ITicketFlujo
    {

        public readonly TicketDA _ticketDa;

        public TicketFlujo(TicketDA ticketDa)
        {
            _ticketDa = ticketDa ?? throw new ArgumentNullException(nameof(ticketDa));
        }

        public Ticket ActualizarEstado(int id, EstadoTicket nuevoEstado, int? nuevoAgenteId, PrioridadTicket? nuevaPrioridad)
        {
            Ticket ticket = _ticketDa.ActualizarEstado(id, nuevoEstado, nuevoAgenteId, nuevaPrioridad);
            return ticket;
        }

        public Ticket guardarTicket(Ticket ticket)
        {
            Ticket ticketAGuardar = _ticketDa.guardarTicket(ticket);
            return ticketAGuardar;
        }

        public (List<Ticket> Items, int TotalCount) ListarPaginado(EstadoTicket? estado, PrioridadTicket? prioridad, int? clienteId, DateTime? desde, DateTime? hasta, string? q, string? orden, int pagina = 1, int tamano = 10)
        {
            return _ticketDa.ListarPaginado(estado, prioridad, clienteId, desde, hasta, q, orden, pagina, tamano);
        }

        public List<Ticket> Obtener()
        {
            List<Ticket> tickets = _ticketDa.Obtener();
            return tickets;
        }

        public Ticket ObtenerPorCodigo(string codigo)
        {
            Ticket codigoAObtener = _ticketDa.ObtenerPorCodigo(codigo);
            return codigoAObtener;
        }

        public Ticket ObtenerPorId(int id)
        {
            Ticket ticket = _ticketDa.ObtenerPorId(id);
            return ticket;
        }

        public ResumenTicketsDto ObtenerResumen()
        {
            ResumenTicketsDto resumen = _ticketDa.ObtenerResumen();
            return resumen;
        }

        public Ticket ReabrirTicket(int id)
        {
            Ticket tickeet = _ticketDa.ReabrirTicket(id);
            return tickeet;
        }
    }
}
