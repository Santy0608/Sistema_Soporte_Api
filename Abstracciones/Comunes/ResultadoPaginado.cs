namespace Abstracciones.Comunes;

/// <summary>Una página de resultados más el total, para que el cliente pueda dibujar la paginación.</summary>
public class ResultadoPaginado<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Total { get; init; }
    public int Pagina { get; init; }
    public int Tamano { get; init; }
    public int TotalPaginas => Tamano <= 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamano);
}
