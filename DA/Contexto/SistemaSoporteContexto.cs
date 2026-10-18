using Abtracciones.Modelos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DA.Contexto
{
    //sesion de trabajo contra la base de datos que vamos a crear SistemaSoporte
    //Solo la capa DA conoce este tipo, el resto del sistema habla con interfaces.
    public class SistemaSoporteContexto : DbContext
    {
        //Las opciones (proveedor, cadena de conexión) llegan por inyección de dependencias desde Program.cs

        public SistemaSoporteContexto(DbContextOptions<SistemaSoporteContexto> options) : base(options)
        {

        }
                       
        //Cadena dbSet se convierte en una tabla. El nombbre de la propieda define el nombre de la tabla
        public DbSet<Cliente> Clientes => Set<Cliente> ();




    }
}
