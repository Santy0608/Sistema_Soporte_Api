using Abstracciones.Clientes;
using Abstracciones.Etiquetas;
using Abstracciones.Excepciones;
using Abstracciones.Tickets;
using Flujo.Tickets;
using Moq;

namespace Pruebas;

public class TicketFlujoTests
{
    private readonly Mock<ITicketDA> _da = new();
    private readonly Mock<IClienteDA> _clienteDA = new();
    private readonly Mock<IEtiquetaDA> _etiquetaDA = new();
    private readonly TicketFlujo _flujo;

    public TicketFlujoTests()
    {
        _flujo = new TicketFlujo(_da.Object, _clienteDA.Object, _etiquetaDA.Object, TimeProvider.System);
    }

    // Deja un ticket en el "DA" para que Flujo lo encuentre, tanto para leer como para actualizar.
    private Ticket TicketEn(EstadoTicket estado, PrioridadTicket prioridad = PrioridadTicket.Media, int? agenteId = null)
    {
        var ticket = new Ticket
        {
            Id = 1,
            Codigo = "TK-2026-0001",
            Titulo = "Prueba",
            Descripcion = "Descripción de prueba",
            Estado = estado,
            Prioridad = prioridad,
            AgenteId = agenteId,
            ClienteId = 1
        };
        _da.Setup(d => d.ObtenerParaActualizarAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _da.Setup(d => d.ObtenerPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        return ticket;
    }

    [Theory]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.EnProceso)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.EnProceso)]
    public async Task CambiarEstado_TransicionValida_Funciona(EstadoTicket origen, EstadoTicket destino)
    {
        TicketEn(origen);

        var resultado = await _flujo.CambiarEstadoAsync(1, new TicketCambiarEstadoDto { Estado = destino }, CancellationToken.None);

        Assert.Equal(destino, resultado.Estado);
        _da.Verify(d => d.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Cerrado, EstadoTicket.EnProceso)] // Cerrado es estado final
    public async Task CambiarEstado_TransicionInvalida_LanzaConflicto(EstadoTicket origen, EstadoTicket destino)
    {
        TicketEn(origen);

        await Assert.ThrowsAsync<ConflictoException>(() =>
            _flujo.CambiarEstadoAsync(1, new TicketCambiarEstadoDto { Estado = destino }, CancellationToken.None));

        _da.Verify(d => d.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CambiarEstado_CriticoSinAgente_NoPuedeResolverse()
    {
        TicketEn(EstadoTicket.EnProceso, PrioridadTicket.Critica, agenteId: null);

        await Assert.ThrowsAsync<ConflictoException>(() =>
            _flujo.CambiarEstadoAsync(1, new TicketCambiarEstadoDto { Estado = EstadoTicket.Resuelto }, CancellationToken.None));
    }

    [Fact]
    public async Task CambiarEstado_CriticoConAgenteEnElMismoRequest_SiPuedeResolverse()
    {
        TicketEn(EstadoTicket.EnProceso, PrioridadTicket.Critica, agenteId: null);

        var resultado = await _flujo.CambiarEstadoAsync(
            1, new TicketCambiarEstadoDto { Estado = EstadoTicket.Resuelto, AgenteId = 7 }, CancellationToken.None);

        Assert.Equal(7, resultado.AgenteId);
    }

    [Fact]
    public async Task CambiarEstado_ResueltoAEnProceso_IncrementaReaperturas()
    {
        var ticket = TicketEn(EstadoTicket.Resuelto);

        await _flujo.CambiarEstadoAsync(1, new TicketCambiarEstadoDto { Estado = EstadoTicket.EnProceso }, CancellationToken.None);

        Assert.Equal(1, ticket.Reaperturas);
    }

    [Fact]
    public async Task Reabrir_SiNoEstaResuelto_LanzaConflicto()
    {
        TicketEn(EstadoTicket.Abierto);

        await Assert.ThrowsAsync<ConflictoException>(() => _flujo.ReabrirAsync(1, CancellationToken.None));
    }

    [Fact]
    public async Task CambiarEstado_SiNoExisteElTicket_LanzaNoEncontrado()
    {
        _da.Setup(d => d.ObtenerParaActualizarAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Ticket?)null);

        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            _flujo.CambiarEstadoAsync(99, new TicketCambiarEstadoDto { Estado = EstadoTicket.EnProceso }, CancellationToken.None));
    }

    [Fact]
    public async Task Crear_ConClienteInexistente_LanzaValidacionNegocio()
    {
        _clienteDA.Setup(c => c.ObtenerPorIdAsync(50, It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<ValidacionNegocioException>(() =>
            _flujo.CrearAsync(new TicketCrearDto { Titulo = "Titulo", Descripcion = "Descripcion larga", ClienteId = 50 }, CancellationToken.None));
    }

    [Fact]
    public async Task Crear_GeneraCodigoConFormatoYEstadoAbierto()
    {
        _clienteDA.Setup(c => c.ObtenerPorIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Cliente { Id = 1 });
        _da.Setup(d => d.ContarPorAnioAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(6); // ya hay 6 este año

        Ticket? guardado = null;
        _da.Setup(d => d.AgregarAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()))
           .Callback<Ticket, CancellationToken>((t, _) => { t.Id = 10; guardado = t; })
           .ReturnsAsync((Ticket t, CancellationToken _) => t);
        _da.Setup(d => d.ObtenerPorIdAsync(10, It.IsAny<CancellationToken>()))
           .ReturnsAsync(() => guardado);

        var resultado = await _flujo.CrearAsync(
            new TicketCrearDto { Titulo = "Titulo", Descripcion = "Descripcion larga", ClienteId = 1 }, CancellationToken.None);

        Assert.True(FormatoCodigoTicket.EsValido(resultado.Codigo));
        Assert.EndsWith("-0007", resultado.Codigo); // secuencia = 6 + 1
        Assert.Equal(EstadoTicket.Abierto, resultado.Estado);
    }

    [Theory]
    [InlineData("TK-2026-0007", true)]
    [InlineData("TK-26-7", false)]
    [InlineData("tk-2026-0007", false)]
    [InlineData("TK-2026-00071", false)]
    public void FormatoCodigo_ValidaElPatron(string codigo, bool esperado)
    {
        Assert.Equal(esperado, FormatoCodigoTicket.EsValido(codigo));
    }
}
