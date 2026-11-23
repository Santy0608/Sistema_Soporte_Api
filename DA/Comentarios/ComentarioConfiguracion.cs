using Abstracciones.Comentarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DA.Comentarios;

public class ComentarioConfiguracion : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> b)
    {
        b.ToTable("Comentarios");
        b.HasKey(c => c.Id);
        b.Property(c => c.Autor).IsRequired().HasMaxLength(100);
        b.Property(c => c.Texto).IsRequired().HasMaxLength(1000);

        // Relación 1-N: un ticket tiene muchos comentarios. Si se borra el ticket, se borran sus comentarios.
        b.HasOne(c => c.Ticket)
            .WithMany(t => t.Comentarios)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(c => new { c.TicketId, c.FechaCreacion });
    }
}
