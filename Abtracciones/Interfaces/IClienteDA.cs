

using Abtracciones.Modelos;

namespace Abtracciones.Interfaces
{
    public interface IClienteDA
    {
        Task<List<Cliente>> Obtener();
       
        Task<Cliente> ObtenerPorId(int id);
        Task<Cliente> Agregar(Cliente cliente);
        //<returns> true si se actualizó el cliente, de lo contrario retorna false</returns>
        Task<bool> Actualizar(Cliente cliente, int id);
        //return true si el cliente se eliminó, de lo contrario retorna false
        Task<bool> Eliminar(int id, Cliente cliente);
    }
}
