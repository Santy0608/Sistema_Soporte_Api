using Abstracciones.Clientes;

namespace Flujo.Clientes;

/// <summary>
/// Mapeo manual entidad -> DTO. Se hace a mano a propósito: es corto, explícito, se puede depurar
/// y no depende de librerías (AutoMapper v15 en adelante tiene licencia comercial).
/// </summary>
internal static class ClienteMapeador
{
    public static ClienteDto ADto(Cliente c) =>
        new(c.Id, c.Nombre, c.Correo, c.Telefono, c.Empresa, c.FechaRegistro);
}
