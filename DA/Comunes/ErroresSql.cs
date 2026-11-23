using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DA.Comunes;

/// <summary>
/// Reconoce errores concretos de SQL Server para traducirlos a excepciones de negocio.
/// Es la ÚNICA clase que conoce los números de error de SQL Server.
/// </summary>
internal static class ErroresSql
{
    /// <summary>2601 y 2627: violación de índice único o de llave primaria.</summary>
    public static bool EsDuplicado(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };

    /// <summary>547: violación de llave foránea.</summary>
    public static bool EsLlaveForanea(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 547 };
}
