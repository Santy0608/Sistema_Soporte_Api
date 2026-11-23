using Abstracciones.Etiquetas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DA.Etiquetas;

public class EtiquetaConfiguracion : IEntityTypeConfiguration<Etiqueta>
{
    public void Configure(EntityTypeBuilder<Etiqueta> b)
    {
        b.ToTable("Etiquetas");
        b.HasKey(e => e.Id);
        b.Property(e => e.Nombre).IsRequired().HasMaxLength(40);
        b.Property(e => e.Color).IsRequired().HasMaxLength(7);
        b.HasIndex(e => e.Nombre).IsUnique();
    }
}

/// <summary>
/// Relación N-M con entidad intermedia y llave compuesta.
/// </summary>
public class TicketEtiquetaConfiguracion : IEntityTypeConfiguration<TicketEtiqueta>
{
    public void Configure(EntityTypeBuilder<TicketEtiqueta> b)
    {
        b.ToTable("TicketEtiquetas");
        b.HasKey(te => new { te.TicketId, te.EtiquetaId });
        b.Property(te => te.FechaAsignacion).IsRequired();

        // Si se borra el ticket, se borran sus asignaciones. Si se intenta borrar una etiqueta en uso, se impide.
        b.HasOne(te => te.Ticket)
            .WithMany(t => t.TicketEtiquetas)
            .HasForeignKey(te => te.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(te => te.Etiqueta)
            .WithMany(e => e.TicketEtiquetas)
            .HasForeignKey(te => te.EtiquetaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
