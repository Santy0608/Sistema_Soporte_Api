using Abtracciones.Interfaces.Tickets;
using Abtracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace DA
{
    public class TicketDA : ITicketDa
    {

        private static List<Ticket> tickets = new List<Ticket>()
        {
            new Ticket
            {
                Id = 1,
                Codigo = "TK-2026-0001",
                Titulo = "Error en el inicio de sesión",
                Descripcion = "Los usuarios reportan que no pueden iniciar sesión con sus credenciales.",
                Prioridad = PrioridadTicket.Critica,
                Estado = EstadoTicket.Abierto,
                ClienteId = 101,
                AgenteId = null,
                Reaperturas = 0,
                FechaCreacion = DateTime.Now.AddDays(-2),
                FechaActualizacion = DateTime.Now.AddDays(-2)
            },
            new Ticket
            {
                Id = 2,
                Codigo = "TK-2026-0002",
                Titulo = "Falla en pasarela de pagos",
                Descripcion = "Las tarjetas de crédito de la región norte dan error.",
                Prioridad = PrioridadTicket.Alta,
                Estado = EstadoTicket.EnProceso,
                ClienteId = 102,
                AgenteId = 5,
                Reaperturas = 0,
                FechaCreacion = DateTime.Now.AddDays(-1),
                FechaActualizacion = DateTime.Now.AddHours(-4)
            },
            new Ticket
            {
                Id = 3,
                Codigo = "TK-2026-0003",
                Titulo = "Actualizar logo corporativo",
                Descripcion = "Cambiar el banner principal en el portal web.",
                Prioridad = PrioridadTicket.Baja,
                Estado = EstadoTicket.Resuelto,
                ClienteId = 103,
                AgenteId = 2,
                Reaperturas = 1,
                FechaCreacion = DateTime.Now.AddDays(-5),
                FechaActualizacion = DateTime.Now.AddDays(-1)
            }
        };

        public TicketDA()
        {
          
        }

        public Ticket ActualizarEstado(int id, EstadoTicket nuevoEstado, int? nuevoAgenteId, PrioridadTicket? nuevaPrioridad)
        {
            var ticketExistente = tickets.FirstOrDefault(t => t.Id == id);

            // 🔒 Bloqueamos la entidad individual para evitar condiciones de carrera en reglas de negocio

            if (ticketExistente == null)
            {
                throw new NotFoundException($"El ticket con ID {id} no fue encontrado.");
            }

            lock (ticketExistente)
            {
                // Determinar cuál será la prioridad y el AgenteId final tras aplicar el cambio
                var prioridadFinal = nuevaPrioridad ?? ticketExistente.Prioridad;
                var agenteIdFinal = nuevoAgenteId ?? ticketExistente.AgenteId;

                // 🚨 REGLA DE NEGOCIO VARIANTE 0:
                // Un ticket de prioridad Crítica no puede pasar a Resuelto sin tener AgenteId asignado
                if (nuevoEstado == EstadoTicket.Resuelto &&
                    prioridadFinal == PrioridadTicket.Critica &&
                    !agenteIdFinal.HasValue)
                {
                    throw new ReglaNegocioException("Un ticket de prioridad Crítica no puede pasar a Resuelto sin tener un Agente asignado.");
                }

                // Aplicar cambios si la validación pasa
                ticketExistente.Estado = nuevoEstado;
                if (nuevoAgenteId.HasValue) ticketExistente.AgenteId = nuevoAgenteId.Value;
                if (nuevaPrioridad.HasValue) ticketExistente.Prioridad = nuevaPrioridad.Value;

                return ticketExistente;
            }
        }

        private void ValidarTransicionEstado(EstadoTicket estadoActual, EstadoTicket estadoNuevo)
        {
            if (estadoActual == estadoNuevo) return;

            // Reglas de transición permitidas:
            // Abierto   -> EnProceso, Cancelado/Cerrado
            // EnProceso -> Resuelto, Abierto
            // Resuelto  -> Cerrado, EnProceso
            // Cerrado   -> (No permite cambios)

            bool esValido = (estadoActual, estadoNuevo) switch
            {
                (EstadoTicket.Abierto, EstadoTicket.EnProceso) => true,
                (EstadoTicket.Abierto, EstadoTicket.Cerrado) => true,
                (EstadoTicket.EnProceso, EstadoTicket.Resuelto) => true,
                (EstadoTicket.EnProceso, EstadoTicket.Abierto) => true,
                (EstadoTicket.Resuelto, EstadoTicket.Cerrado) => true,
                (EstadoTicket.Resuelto, EstadoTicket.EnProceso) => true,
                _ => false
            };

            if (!esValido)
            {
                throw new ReglaNegocioException($"No está permitida la transición de estado de '{estadoActual}' a '{estadoNuevo}'.");
            }
        }

        public Ticket guardarTicket(Ticket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentException("Datos de Ticket Inválido");
            }

            ticket.Id = tickets.Count + 1;
            tickets.Add(ticket);
            return ticket;
        }

        public List<Ticket> Obtener()
        {
            return tickets;
        }

        public Ticket ObtenerPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            return tickets.FirstOrDefault(t => t.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
        }

        public Ticket ObtenerPorId(int id)
        {
            Ticket ticket = tickets.Find(t => t.Id == id);
            return ticket;           
        }

        public ResumenTicketsDto ObtenerResumen()
        {
            var listaTickets = tickets.ToList();

            // Agrupar por Estado garantizando que todos los estados del Enum figuren
            var porEstado = Enum.GetValues<EstadoTicket>()
                .ToDictionary(
                    e => e.ToString(),
                    e => listaTickets.Count(t => t.Estado == e)
                );

            // Agrupar por Prioridad
            var porPrioridad = Enum.GetValues<PrioridadTicket>()
                .ToDictionary(
                    p => p.ToString(),
                    p => listaTickets.Count(t => t.Prioridad == p)
                );

            return new ResumenTicketsDto(
                Total: listaTickets.Count,
                PorEstado: porEstado,
                PorPrioridad: porPrioridad
            );
        }

        public Ticket ReabrirTicket(int id)
        {
            // Reemplazo de TryGetValue con List<Ticket>:
            var ticket = tickets.FirstOrDefault(t => t.Id == id);

            if (ticket == null)
            {
                // 404
                throw new NotFoundException($"El ticket con ID {id} no fue encontrado.");
            }

            lock (ticket)
            {
                if (ticket.Estado != EstadoTicket.Resuelto)
                {
                    throw new ReglaNegocioException($"No se puede reabrir el ticket porque su estado actual es '{ticket.Estado}'. Debe estar en 'Resuelto'.");
                }

                ticket.Estado = EstadoTicket.EnProceso;
                ticket.Reaperturas++;

                return ticket;
            }
        }

        private int _secuenciaActual = 1;

        private string generarCodigoTicket()
        {
            int anioActual = DateTime.Now.Year;
            string codigo = $"TK-{anioActual}-{_secuenciaActual:D4}"; // :D4 rellena con ceros a la izquierda (ej: 0001)
            _secuenciaActual++;
            return codigo;
        }

        public (List<Ticket> Items, int TotalCount) ListarPaginado(
            EstadoTicket? estado,
            PrioridadTicket? prioridad,
            int? clienteId,
            DateTime? desde,
            DateTime? hasta,
            string? q,
            string? orden,
            int pagina = 1,
            int tamano = 10)
                {
                    int tamanoMaximoVariante = 20;
                    if (tamano <= 0) tamano = 10;
                    if (tamano > tamanoMaximoVariante) tamano = tamanoMaximoVariante;

                    if (pagina < 1) pagina = 1;

                    IEnumerable<Ticket> query = tickets;

                    if (estado.HasValue)
                        query = query.Where(t => t.Estado == estado.Value);

                    if (prioridad.HasValue)
                        query = query.Where(t => t.Prioridad == prioridad.Value);

                    if (clienteId.HasValue)
                        query = query.Where(t => t.ClienteId == clienteId.Value);

                    if (desde.HasValue)
                        query = query.Where(t => t.FechaCreacion >= desde.Value);

                    if (hasta.HasValue)
                        query = query.Where(t => t.FechaCreacion <= hasta.Value);

                    if (!string.IsNullOrWhiteSpace(q))
                    {
                        string busqueda = q.Trim();
                        query = query.Where(t =>
                            t.Titulo.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                            t.Descripcion.Contains(busqueda, StringComparison.OrdinalIgnoreCase));
                    }

                    int totalCount = query.Count();

                    orden = orden?.ToLower().Trim();
                    query = orden switch
                    {
                        "fecha" => query.OrderBy(t => t.FechaCreacion),
                        "-fecha" => query.OrderByDescending(t => t.FechaCreacion),
                        "prioridad" => query.OrderBy(t => t.Prioridad),
                        "-prioridad" => query.OrderByDescending(t => t.Prioridad),
                        _ => query.OrderByDescending(t => t.FechaCreacion) // Por defecto -fecha
                    };

                    var itemsPaginados = query
                        .Skip((pagina - 1) * tamano)
                        .Take(tamano)
                        .ToList();

                    return (itemsPaginados, totalCount);
        }

    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string message) : base(message) { }
}
