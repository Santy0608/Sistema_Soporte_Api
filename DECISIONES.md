# Registro de Decisiones Técnicas y de Diseño (DECISIONES.md)

Este documento detalla la justificación técnica de la arquitectura, patrones de diseño, códigos de estado HTTP y estrategias de manejo de datos implementadas en el proyecto.

---

## 1. Identificación
* **Variante (V)**: V1
* **Nombre Completo**: Santiago Barquero Torres

---

## 2. ClienteId Inexistente al Crear un Ticket: ¿400, 404 o 422?

**Decisión Elegida**: **`422 Unprocessable Entity`** (o `400 Bad Request` si se sigue una interpretación estricta de validación de contrato).

### Justificación:
* **Por qué no `404 Not Found`**: El código 404 está reservado primariamente para cuando el URI/recurso solicitado en la ruta no existe (`POST /api/tickets` sí existe). Retornar 404 por un identificador dentro del payload confunde la localización del endpoint con la existencia de datos secundarios.
* **Por qué `422 Unprocessable Entity` frente a `400 Bad Request`**: 
  - `400 Bad Request` indica que la sintaxis o estructura del JSON entregado es incorrecta o ilegible.
  - `422 Unprocessable Entity` es la respuesta semánticamente precisa para solicitudes sintácticamente válidas cuyas reglas semánticas/de entidad fallan (el JSON es un documento bien formado, pero viola la integridad referencial al no existir el `ClienteId` en la base de datos).

---

## 3. Comportamiento de Restricciones de Ruta (`/api/tickets/abc`) vs Body Inválido

### ¿Por qué `/api/tickets/abc` responde `404 Not Found`?
Las restricciones de ruta (`IRouteConstraint` / `IParameterPolicy`) actúan en la etapa de **Routing / Endpoint Matching** del middleware pipeline de ASP.NET Core:
* Cuando la petición llega, el enrutador compara la plantilla de la URL con la restricción configurada (por ejemplo, exigir que sea un entero o que cumpla el patrón `TK-AAAA-NNNN`).
* Si `abc` no satisface la restricción, el enrutador descarta el endpoint por completo como un candidato válido. Al no existir ningún otro endpoint que coincida con esa plantilla de URL, ASP.NET Core determina que el recurso solicitado no fue encontrado, retornando **`404 Not Found`**.

### ¿Por qué un Body inválido responde `400 Bad Request`?
* Un body inválido supera con éxito la fase de enrutamiento y alcanza la etapa de **Model Binding / Validation** dentro del controlador.
* Al fallar la deserialización del JSON o las reglas del modelo (`[Required]`, tipos de datos incompatibles), el marco de ASP.NET Core genera una respuesta de error de validación bajo RFC 9457 con código **`400 Bad Request`**.

### ¿Cuándo usar cada mecanismo?
* **Restricciones de Ruta**: Se utilizan para diferenciar sobrecargas de endpoints en la URL y evitar que peticiones con formatos evidentemente equivocados o maliciosos lleguen a instanciar controladores o capas de negocio.
* **Validación de Body**: Se utiliza para validar reglas complejas de modelo, rangos, tipos de datos anidados e integridad del payload entregado.

---

## 4. Comunicación de Errores de Negocio desde la Capa de Flujo

### Comparativa de Alternativas:

1. **Excepciones Personalizadas (Ej. `DomainRuleException`)**:
   - *Ventajas*: Mantiene la firma de los métodos limpia (`Ticket Crear(Ticket t)`). Es expresivo e interrumpe de inmediato el flujo sin necesidad de validar manualmente en cada nivel.
   - *Desventajas*: El control de flujo mediante excepciones es costoso a nivel de rendimiento (.NET recaba el stack trace) y requiere un middleware global (`ExceptionHandlerMiddleware`) para capturarlas y traducirlas a `ProblemDetails`.

2. **Objeto de Resultado Patrón `Result<T>` / Monad**:
   - *Ventajas*: Es explícito, seguro en compilación y no penaliza el rendimiento por recolección de stack trace. Facilita las pruebas unitarias.
   - *Desventajas*: Enfoca el código hacia el estilo funcional, obligando a cada capa a desempaquetar el resultado (`result.IsSuccess`) y propagar manualmente los mensajes de error.

**Elección**: Se utilizaron **Excepciones Personalizadas de Dominio** junto con el middleware de **`ProblemDetails` (RFC 9457)**.
* **Costo**: Generación de overhead mínimo en el stack trace al lanzar excepciones en flujos alternativos, a cambio de mantener una arquitectura limpia y acoplada a los estándares de ASP.NET Core.

---

## 5. Paginación y Control de Carga

### ¿Por qué limitar el tamaño máximo de página (`tamanoMaximo`)?
Para prevenir ataques de denegación de servicio (DoS) involuntarios o intencionados. Permitir que un cliente pida `tamano=1000000` saturaría la memoria del servidor y el ancho de banda al serializar listados masivos.

### ¿Qué devuelve una página fuera de rango?
Devuelve una respuesta **`200 OK` con un array vacío `[]` y metadatos de paginación legibles** (ej. `totalItems: 3`, `paginaActual: 99`).
* *Razón*: Estructuralmente la petición fue válida; simplemente no existen elementos en esa subsección del conjunto. No constituye un error del cliente (400) ni la inexistencia del recurso principal (404).

### Paginación por Cursor vs Offset
* **Paginación usada (Offset / Limit)**: Selecciona páginas especificando `pagina` y `tamano`. Es simple de implementar pero sufre de degradación de rendimiento en offset altos (`SKIP n`) e inconsistencias si se agregan registros mientras se navega.
* **Paginación por Cursor**: Utiliza un puntero (como el ID o Timestamp del último elemento visto) para pedir "los $N$ siguientes después del cursor". No se usó en esta implementación porque el volumen de datos de prueba es acotado y se requería navegación directa a páginas arbitrarias (ej. ir a la página 5 directamente desde el UI).

---

## 6. Estrategia de Caché mediante ETag

### Problema que resuelve
Optimiza el consumo de ancho de banda y evita el procesamiento innecesario reduciendo la transferencia de payloads idénticos mediante respuestas `304 Not Modified`.

### Tipo de ETag y Cálculo
Se utilizó un **ETag Débil (Weak ETag)** calculado mediante un hash encriptado (MD5/SHA256) sobre la representación serializada del contenido o concatenando el ID con la marca de tiempo de modificación (`FechaActualizacion`).

### Situaciones que NO cubre
No previene la ejecución inicial de la consulta a la base de datos o almacenamiento si el hash se calcula dinámicamente en el servidor en cada petición. Para mitigar esto, se requiere combinarlo con estrategias de caché de datos en memoria (ej. `IMemoryCache`).

---

## 7. Transiciones de Estado Inválidas: 409 Conflict

Se utiliza **`409 Conflict`** (y no 400 ni 422) porque el conflicto de estado ocurre debido a una discrepancia entre el **estado actual del recurso persistido en el servidor** y la acción solicitada. 

* `400` denota un problema en la estructura del payload.
* `422` denota una regla semántica del objeto enviado.
* `409` denota explícitamente que la solicitud no puede procesarse porque viola el estado presente de la entidad (ej. intentar reabrir un ticket que ya se encuentra en estado `Abierto` o cancelar un ticket `Cerrado`).

---

## 8. Fuentes Consultadas

* Microsoft Docs - *Handle errors in ASP.NET Core web APIs (RFC 9457)*: https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors
* RFC 9457 - *Problem Details for HTTP APIs*: https://datatracker.ietf.org/doc/html/rfc9457
* Microsoft Docs - *Custom Route Constraints in ASP.NET Core*: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing

---

## 9. Declaración de Uso de IA

Se utilizó un asistente de Inteligencia Artificial (LLM) como herramienta de apoyo técnico durante el desarrollo del proyecto para:
* Consultar sintaxis de enrutamiento y firmas de interfaces en .NET 10 (`IParameterPolicy`).
* Diagnosticar excepciones en tiempo de ejecución (resolución de registros en el contenedor de inyección de dependencias).
* Estructurar respuestas alineadas con el estándar RFC 9457.
