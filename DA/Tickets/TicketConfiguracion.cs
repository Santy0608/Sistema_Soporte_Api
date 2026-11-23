using Abstracciones.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DA.Tickets;

public class TicketConfiguracion : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> b)
    {
        b.ToTable("Tickets");
        b.HasKey(t => t.Id);

        b.Property(t => t.Codigo).IsRequired().HasMaxLength(20);
        b.HasIndex(t => t.Codigo).IsUnique();

        b.Property(t => t.Titulo).IsRequired().HasMaxLength(120);
        b.Property(t => t.Descripcion).IsRequired().HasMaxLength(2000);

        b.Property(t => t.Prioridad).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Relación 1-N Cliente -> Ticket. Restrict: no se puede borrar un cliente que tenga tickets.
        b.HasOne(t => t.Cliente)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices pensados para los filtros que realmente usa la pantalla.
        b.HasIndex(t => new { t.Estado, t.Prioridad });
        b.HasIndex(t => t.FechaCreacion);
        b.HasIndex(t => t.ClienteId);
    }
}
