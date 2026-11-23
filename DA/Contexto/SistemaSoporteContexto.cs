using Abstracciones.Clientes;
using Abstracciones.Comentarios;
using Abstracciones.Etiquetas;
using Abstracciones.Seguridad;
using Abstracciones.Tickets;
using Microsoft.EntityFrameworkCore;

namespace DA.Contexto;

public class SistemaSoporteContexto : DbContext
{
    public SistemaSoporteContexto(DbContextOptions<SistemaSoporteContexto> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Etiqueta> Etiquetas => Set<Etiqueta>();
    public DbSet<TicketEtiqueta> TicketEtiquetas => Set<TicketEtiqueta>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SistemaSoporteContexto).Assembly);
    }
}
