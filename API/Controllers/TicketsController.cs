using Abtracciones.Interfaces;
using Abtracciones.Interfaces.Tickets;
using Abtracciones.Modelos;
using DA;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {

        private readonly ITicketFlujo _ticketFlujo;

        public TicketsController(ITicketFlujo ticketFlujo) => _ticketFlujo = ticketFlujo;

        [HttpGet]
        public ActionResult<IEnumerable<Ticket>> GetTickets(
            [FromQuery] EstadoTicket? estado,
            [FromQuery] PrioridadTicket? prioridad,
            [FromQuery] int? clienteId,
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] string? q,
            [FromQuery] string orden = "-fecha",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamano = 10)
        {
            var (items, totalCount) = _ticketFlujo.ListarPaginado(
                estado, prioridad, clienteId, desde, hasta, q, orden, pagina, tamano
            );

            // Ajustar el tamaño efectivo respetando el tope de 20 de la Variante 0
            int tamanoEfectivo = Math.Min(tamano > 0 ? tamano : 10, 20);

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", pagina.ToString());
            Response.Headers.Append("X-Page-Size", tamanoEfectivo.ToString());
            Response.Headers.Append("Access-Control-Expose-Headers", "X-Total-Count, X-Page, X-Page-Size");

            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<Ticket> Obtener(int id)
        {
            Ticket ticket = _ticketFlujo.ObtenerPorId(id);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }


        [HttpPost]
        public ActionResult<Ticket> Agregar([FromBody] Ticket ticket)
        {
            Ticket creado = _ticketFlujo.guardarTicket(ticket);
            return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
        }

        [HttpGet("{codigo:ticketcode}")]
        public IActionResult ObtenerPorCodigo(string codigo)
        {
            var ticket = _ticketFlujo.ObtenerPorCodigo(codigo);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }

        [HttpPatch("{id}/estado")]
        public ActionResult<Ticket> CambiarEstado(int id, [FromBody] CambiarEstadoRequest request)
        {
            // El 400 BadRequest de validación es generado automáticamente por [ApiController] usando ValidationProblemDetails
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var ticketActualizado = _ticketFlujo.ActualizarEstado(id, request.Estado, request.AgenteId, request.Prioridad);
                return Ok(ticketActualizado);
            }
            catch (NotFoundException ex)
            {
                // 404 - RFC 9457 ProblemDetails
                return Problem(
                    title: "Ticket no encontrado",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status404NotFound
                );
            }
            catch (ReglaNegocioException ex)
            {
                // 409 - RFC 9457 ProblemDetails
                return Problem(
                    title: "Transición de estado no permitida",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict
                );
            }
        }

    }
}
