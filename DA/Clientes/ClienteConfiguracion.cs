using Abstracciones.Clientes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DA.Clientes;

public class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Clientes");
        b.HasKey(c => c.Id);

        b.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
        b.Property(c => c.Correo).IsRequired().HasMaxLength(150);
        b.Property(c => c.Telefono).IsRequired().HasMaxLength(20);
        b.Property(c => c.Empresa).HasMaxLength(100);
        b.Property(c => c.FechaRegistro).HasDefaultValueSql("SYSUTCDATETIME()");

        // Índice único: garantiza que no existan dos clientes con el mismo correo AUNQUE dos
        // peticiones lleguen al mismo tiempo. La validación en Flujo da el mensaje amable; el índice es la red de seguridad.
        b.HasIndex(c => c.Correo).IsUnique();
    }
}
