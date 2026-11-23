using Flujo.Tickets;

namespace Pruebas;

public class TicketCsvTests
{
    // TicketCsv es internal: se expone a Pruebas con InternalsVisibleTo (ver Flujo.csproj).
    [Theory]
    [InlineData("Hola", "Hola")]
    [InlineData("Hola, mundo", "\"Hola, mundo\"")]
    [InlineData("Dijo \"hola\"", "\"Dijo \"\"hola\"\"\"")]
    [InlineData("=SUMA(A1:A2)", "'=SUMA(A1:A2)")] // defensa contra CSV injection
    public void Escapar_AplicaLasReglasDeCsv(string entrada, string esperado)
    {
        Assert.Equal(esperado, TicketCsv.Escapar(entrada));
    }
}
