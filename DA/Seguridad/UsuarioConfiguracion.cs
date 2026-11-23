using Abstracciones.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DA.Seguridad
{
    public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> b) 
        {
            b.ToTable("Usuarios");
            b.HasKey(u => u.Id);
            b.Property(u => u.Correo).IsRequired().HasMaxLength(200);
            b.HasIndex(u => u.Correo).IsUnique();                             // no se repiten correos
            b.Property(u => u.Nombre).IsRequired().HasMaxLength(120);
            b.Property(u => u.ClaveHash).IsRequired().HasMaxLength(200);
            b.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);
        }
    }
}
