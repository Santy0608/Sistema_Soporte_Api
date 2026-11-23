using Abstracciones.Clientes;
using Abstracciones.Excepciones;
using Flujo.Clientes;
using Moq;

namespace Pruebas;

/// <summary>
/// Pruebas unitarias de la capa Flujo. El DA es un SIMULACRO (Moq): no hay base de datos ni HTTP.
/// Patrón de cada prueba: Arrange (preparar) / Act (ejecutar) / Assert (verificar).
/// </summary>
public class ClienteFlujoTests
{
    private readonly Mock<IClienteDA> _da = new();
    private readonly ClienteFlujo _flujo;

    public ClienteFlujoTests()
    {
        _flujo = new ClienteFlujo(_da.Object, TimeProvider.System);
    }

    private static ClienteCrearDto DtoValido() => new()
    {
        Nombre = "  Ana Pérez ",
        Correo = "  ANA@Ejemplo.COM ",
        Telefono = "88888888",
        Empresa = "   "
    };

    [Fact]
    public async Task Crear_NormalizaCorreoYLimpiaCampos()
    {
        _da.Setup(d => d.ObtenerPorCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync((Cliente?)null);
        _da.Setup(d => d.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync((Cliente c, CancellationToken _) => c);

        var resultado = await _flujo.CrearAsync(DtoValido(), CancellationToken.None);

        Assert.Equal("ana@ejemplo.com", resultado.Correo);
        Assert.Equal("Ana Pérez", resultado.Nombre);
        Assert.Null(resultado.Empresa); // una empresa en blanco se guarda como null
    }

    [Fact]
    public async Task Crear_ConCorreoDuplicado_LanzaConflicto()
    {
        _da.Setup(d => d.ObtenerPorCorreoAsync("ana@ejemplo.com", It.IsAny<CancellationToken>()))
           .ReturnsAsync(new Cliente { Id = 5, Correo = "ana@ejemplo.com" });

        await Assert.ThrowsAsync<ConflictoException>(() => _flujo.CrearAsync(DtoValido(), CancellationToken.None));

        // Verifica además que NO se intentó guardar nada.
        _da.Verify(d => d.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Obtener_SiNoExiste_LanzaNoEncontrado()
    {
        _da.Setup(d => d.ObtenerPorIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<NoEncontradoException>(() => _flujo.ObtenerAsync(99, CancellationToken.None));
    }

    [Fact]
    public async Task Actualizar_ConCorreoDeOtroCliente_LanzaConflicto()
    {
        _da.Setup(d => d.ObtenerPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = 1 });
        _da.Setup(d => d.ObtenerPorCorreoAsync("ana@ejemplo.com", It.IsAny<CancellationToken>()))
           .ReturnsAsync(new Cliente { Id = 2, Correo = "ana@ejemplo.com" }); // pertenece a OTRO

        await Assert.ThrowsAsync<ConflictoException>(() =>
            _flujo.ActualizarAsync(1, new ClienteActualizarDto
            {
                Nombre = "Ana", Correo = "ana@ejemplo.com", Telefono = "88888888"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task Eliminar_ConTickets_LanzaConflicto()
    {
        _da.Setup(d => d.ObtenerPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = 1 });
        _da.Setup(d => d.TieneTicketsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictoException>(() => _flujo.EliminarAsync(1, CancellationToken.None));
        _da.Verify(d => d.EliminarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
