# SistemaSoporte
 
API de soporte en 4 capas (.NET 10): `Abstracciones`, `DA`, `Flujo`, `API`, mas `Pruebas`.
Incluye un MVC (`Web/`, otra solucion) que consume el API por HTTP: lista, detalle y creacion de tickets,
cambio de estado, exportar CSV, inicio de sesion y menu por rol.
 
## Correr el API (la primera vez)
 
```bash
# 1. Las migraciones ya estan en DA/Migrations. Solo si cambia el modelo, genere una nueva:
#    dotnet ef migrations add NombreDelCambio --project DA --startup-project API --output-dir Migrations
 
# 2. Levantar el API. Con Base:MigrarAlIniciar = true (valor por defecto) crea la base y aplica las migraciones sola.
dotnet run --project API          # http://localhost:5190/scalar/v1  |  /salud  |  /tickets.html
 
# 3. Datos de demostracion y usuarios (muestra el servidor destino y pide confirmacion). Con el API detenido.
#    Antes configure los secretos de la seccion siguiente.
dotnet run --project API -- --seed
 
# 4. Pruebas
dotnet test
```
 
Cambiar de base de datos es cambiar la cadena de conexion (nombre de la base u otro servidor). Con LocalDB:
clonar, `dotnet run --project API` y la base se crea con el nombre que diga `ConnectionStrings:SistemaSoporte`.
Para apagar la migracion automatica: `"Base": { "MigrarAlIniciar": false }` o la variable `Base__MigrarAlIniciar=false`.
 
## Seguridad: login, JWT y roles
 
Dos secretos locales (nunca van en el repositorio). Se guardan con User Secrets, fuera del proyecto:
 
```bash
dotnet user-secrets set "Jwt:Clave" "una-frase-larga-de-al-menos-32-caracteres" --project API
dotnet user-secrets set "Semilla:ClaveInicial" "Soporte2026!" --project API
dotnet run --project API -- --seed --yes      # crea los usuarios (y los datos de demostracion si la base esta vacia)
```
 
| Usuario | Rol |
|---|---|
| `admin@soporte.local` | Admin (puede borrar clientes y exportar CSV) |
| `agente@soporte.local` | Agente |
 
La clave de ambos es la que se puso en `Semilla:ClaveInicial`.
 
- `POST /api/auth/login` devuelve el token. Pruebelo en `API/API.http`, seccion SEGURIDAD.
- `Seguridad:Exigir` en `API/appsettings.json`: en `false` el API queda abierto, salvo los endpoints con `[Authorize]`;
  en `true` todo endpoint pide token (excepto login, `/salud` y la documentacion).
- Sin `Jwt:Clave` el login no puede emitir tokens, y con `Exigir = true` el API no arranca.
## Problemas frecuentes
 
**El programa se detiene al probar una clave incorrecta (con F5).** No es un error del sistema. Cuando el login recibe una
clave incorrecta, el codigo lanza `CredencialesInvalidasException` a proposito: es una excepcion de NEGOCIO, y el manejador
global del API la convierte en una respuesta 401. Pero Visual Studio, con el depurador (F5), ve la excepcion primero y pausa
la ejecucion. Si la deja pausada, la peticion de `API.http` termina con `The request was cancelled due to the configured
timeout of 20 second(s) elapsing`. Opciones, de la mas simple a la mas permanente:
 
1. Presione **F5 (Continuar)** en el depurador: la respuesta 401 sale normal.
2. Corra **sin depurador**: **Ctrl+F5** en Visual Studio, o `dotnet run --project API` en una terminal. Aqui la excepcion se
   imprime en la consola como informacion y el API sigue funcionando sin detenerse.
3. Para que el depurador no se detenga en ella: en la ventana de la excepcion, desmarque
   **"Break when this exception type is user-unhandled"**, o abra Depurar, Ventanas, **Configuracion de excepciones**
   (Exception Settings) y desmarque `CredencialesInvalidasException`.
**HTTP0021 en API.http.** El token se lee con `{{loginAdmin.response.body.$.token}}`; el `$.` es obligatorio.
 
**El MVC dice que no puede iniciar sesion.** Revise que el API este corriendo, que existan los dos secretos
(`dotnet user-secrets list --project API`) y que haya corrido la semilla (`--seed --yes`).
 
**Errores de "ya contiene una definicion" al compilar.** Casi siempre es una carpeta duplicada tipo `Clientes - Copy`.
Al pegar carpetas en Windows elija "Reemplazar los archivos en el destino", no "Conservar ambos".
 
## Correr el MVC (otra terminal o segunda ventana de Visual Studio)
 
```bash
dotnet run --project Web/Web.csproj    # http://localhost:5200  (el API debe estar corriendo)
```
 
El MVC pide iniciar sesion con alguno de los usuarios de la semilla. El token del API queda dentro de la cookie
de sesion y se envia en cada llamada (`TokenApiHandler`).
 
La URL del API sale de `Api:UrlBase` en `Web/appsettings.json` (o la variable `Api__UrlBase`).
`Web` no tiene ninguna referencia a los proyectos del API: solo habla HTTP.
 
## Azure (resumen)
 
| Que | Donde |
|---|---|
| Cadena de conexion | Web App > Variables de entorno > Cadenas de conexion, nombre `SistemaSoporte`, tipo SQLAzure |
| Migrar al primer arranque | App setting `Base__MigrarAlIniciar = true` |
| Ver Scalar en Azure | App setting `Documentacion__Habilitar = true` |
| URL del API para el MVC | App setting `Api__UrlBase = https://<tu-api>.azurewebsites.net/` |
| Verificar el despliegue | Abrir `/salud` (200 = app y base vivas) |
 
Ejemplo de pipeline de despliegue: `docs/ejemplo-despliegue-azure.yml` (el real lo genera el Centro de implementacion).
 
## Modo Practica 1 (tickets en memoria)
 
En `API/appsettings.json` cambie `"Almacenamiento": { "Tickets": "Memoria" }`.
`TicketDAMemoria` reemplaza a `TicketDA` (Entity Framework) sin tocar Flujo ni API.
 
## Regla de dependencias
 
```
API  ->  Flujo  ->  Abstracciones  <-  DA
 \________________________________/
   (API referencia DA solo en Program.cs)
 
Web  ->  (solo HTTP, ninguna referencia)
```
 
