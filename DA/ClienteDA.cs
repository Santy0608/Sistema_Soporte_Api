using Abtracciones.Interfaces;
using Abtracciones.Modelos;
using DA.Contexto;
using Microsoft.EntityFrameworkCore;

namespace DA
{
    public class ClienteDA : IClienteDA
    {

        private readonly SistemaSoporteContexto _contexto;

        public ClienteDA(SistemaSoporteContexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<Cliente>> Obtener() => await _contexto.Clientes.AsNoTracking().ToListAsync();

        public async Task<Cliente> ObtenerPorId(int id) => await _contexto.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Cliente> Agregar(Cliente cliente)
        {
            _contexto.Clientes.Add(cliente);
            _contexto.SaveChanges();
            return cliente;
        }

        public async Task<bool> Actualizar(Cliente cliente, int id)
        {
            var clienteExistente = await _contexto.Clientes.FindAsync(id);
            if (clienteExistente == null)
            {
                return false;
            }
            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Correo = cliente.Correo;
            clienteExistente.Telefono = cliente.Telefono;
            await _contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id, Cliente cliente)
        {
            var clienteExistente = await _contexto.Clientes.FindAsync(id);
            if (clienteExistente == null)
            {
                return false;
            }
            _contexto.Clientes.Remove(clienteExistente);
            await _contexto.SaveChangesAsync();
            return true;
        }

    }
}
