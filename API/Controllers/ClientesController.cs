using Abtracciones.Modelos;
using Abtracciones.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteFlujo _clienteFlujo;

        public ClientesController(IClienteFlujo clienteFlujo) => _clienteFlujo = clienteFlujo;

        [HttpGet]
        public async Task<IActionResult> Obtener() => Ok(await _clienteFlujo.Obtener());
    

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cliente = await _clienteFlujo.ObtenerPorId(id);
            return cliente is null ? NotFound() : Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] Cliente cliente)
        {
            //[ApiController] ya valida el modelo antes de llegar aquí.
            var creado = await _clienteFlujo.Agregar(cliente);
            return CreatedAtAction(nameof(ObtenerPorId), new {id = creado.Id}, creado);
        }

        [HttpPut("id:int")]
        public async Task<IActionResult> Actualizar(int id, Cliente dato) => await _clienteFlujo.Actualizar(id, dato) ? NoContent() : NotFound();


        [HttpDelete("id:int")]
        public async Task<IActionResult> Eliminar(int id, Cliente dato) => await _clienteFlujo.Eliminar(id, dato) ? NoContent() : NotFound();


    }
}
