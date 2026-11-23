-- Pegar este SQL dentro de migrationBuilder.Sql(@"...") de una migración vacía
-- (dotnet ef migrations add ActualizarSpBuscarCliente), DESPUÉS de la migración ModeloRobusto.
-- Motivo: FromSqlRaw exige TODAS las columnas del modelo y Cliente ahora tiene FechaRegistro.
CREATE OR ALTER PROCEDURE dbo.sp_BuscarCliente @Nombre NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Correo, Telefono, Empresa, FechaRegistro
    FROM dbo.Clientes
    WHERE @Nombre IS NULL OR Nombre LIKE '%' + @Nombre + '%'
    ORDER BY Nombre;
END
