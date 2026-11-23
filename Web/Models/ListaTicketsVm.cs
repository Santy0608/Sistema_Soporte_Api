namespace Web.Models;

/// <summary>Lo que necesita la vista de la lista: las filas, el filtro actual y los datos de paginación.</summary>
public class ListaTicketsVm
{
    public List<TicketVm> Items { get; set; } = new();

    // Filtro actual (se vuelve a pintar en el formulario y se conserva al paginar)
    public string? Estado { get; set; }
    public string? Q { get; set; }

    // Paginación: el API la manda en los HEADERS (X-Total-Count), no en el cuerpo.
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 10;
    public int Total { get; set; }

    public int TotalPaginas => Tamano <= 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamano);
    public bool HayAnterior => Pagina > 1;
    public bool HaySiguiente => Pagina < TotalPaginas;
}
