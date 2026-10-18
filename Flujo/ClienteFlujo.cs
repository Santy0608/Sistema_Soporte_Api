using Abtracciones.Modelos;
using Abtracciones.Interfaces;

namespace Flujo
{
    public class ClienteFlujo : IClienteFlujo
    {
        private readonly IClienteDA _clienteDA;
        public ClienteFlujo(IClienteDA clienteDA) => _clienteDA = clienteDA;

        public Task<List<Cliente>> Obtener() => _clienteDA.Obtener();

        public Task<Cliente> ObtenerPorId(int id) => _clienteDA.ObtenerPorId(id);

        public Task<Cliente> Agregar(Cliente cliente)
        {
            cliente.Correo = cliente.Correo.Trim().ToLowerInvariant();
            return _clienteDA.Agregar(cliente);
        }

        public Task<bool> Actualizar(int id, Cliente cliente)
        {
            cliente.Correo = cliente.Correo.Trim().ToLowerInvariant();
            return _clienteDA.Actualizar(cliente, id);
        }

        public async Task<bool> Eliminar(int id, Cliente cliente)
        {
            return await _clienteDA.Eliminar(id, cliente);
        }
    }
}
