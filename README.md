# Sistema Soporte API - ASP.NET Core Web API

API RESTful desarrollada en **ASP.NET Core** bajo principios de **Clean Architecture**, diseñada para la gestión integral de tickets de soporte técnico. Implementa estándares modernos de la industria como manejo de errores estandarizado (RFC 9457 / ProblemDetails), restricciones de ruta personalizadas, paginación y caché con ETag.

---

## 🏗️ Arquitectura del Proyecto

El proyecto está estructurado en tres capas principales para separar responsabilidades:

1. **API (Capa de Presentación)**: 
   - Controladores (`TicketsController`) y enrutamiento.
   - Configuración de middlewares, inyección de dependencias y archivos estáticos (Frontend).
2. **Flujo (Capa de Lógica de Negocio)**:
   - Reglas de negocio, validaciones y orquestación (`TicketFlujo`).
3. **DA (Capa de Acceso a Datos)**:
   - Persistencia y manejo de datos (`TicketDA`), implementando almacenamiento en memoria con soporte estático para persistencia entre peticiones.

---

##  Características Principales

* **Manejo de Errores RFC 9457**: Respuestas de errores normalizadas mediante `ProblemDetails` para excepciones de validación y negocio.
* **Restricción de Ruta Personalizada**: Validador de formato para códigos de ticket bajo el patrón `TK-AAAA-NNNN` utilizando `IParameterPolicy`.
* **Paginación Avanzada**: Endpoints GET optimizados con parámetros de filtrado por estado, prioridad, cliente, fechas y ordenamiento dinámico.
* **Caché HTTP (ETag)**: Optimización de consultas recurrentes para evitar sobrecarga en el servidor.
* **Frontend Integrado**: Interfaz web estática (`listadoTickets.html`) configurada como página de inicio por defecto para consumir la API mediante `fetch`.

---

##  Tecnologías y Requisitos

* **.NET 10.0 SDK** 
* **C# 12**
* **Visual Studio 2026 

---

## 📋 Endpoints Principales

### 1. Listar Tickets (Paginado y Filtrado)
* **Método:** `GET`
* **Ruta:** `/api/tickets`
* **Parámetros de consulta (Query):**
  * `estado`: Filtra por estado (`Abierto`, `EnProceso`, `Resuelto`, `Cerrado`).
  * `prioridad`: Filtra por prioridad (`Baja`, `Media`, `Alta`, `Critica`).
  * `clienteId`: Identificador del cliente (entero).
  * `desde` / `hasta`: Rango de fechas de creación (`YYYY-MM-DD`).
  * `q`: Búsqueda por texto en título o descripción.
  * `orden`: Criterio de ordenamiento (ej. `-fecha` para descendente).
  * `pagina`: Número de página (por defecto `1`).
  * `tamano`: Cantidad de registros por página (por defecto `10`).

---

### 2. Obtener Ticket por ID
* **Método:** `GET`
* **Ruta:** `/api/tickets/{id}`
* **Descripción:** Devuelve un ticket específico. Soporta encabezado `ETag` (`If-None-Match`) devolviendo un estado `304 Not Modified` si los datos no han cambiado.

---

### 3. Obtener Ticket por Código
* **Método:** `GET`
* **Ruta:** `/api/tickets/codigo/{codigo}`
* **Descripción:** Busca por el código único del ticket. Aplica la restricción de ruta personalizada `ticketcode` (`TK-AAAA-NNNN`). Si el formato no coincide, la ruta devuelve `404 Not Found`.

---

### 4. Crear un Ticket
* **Método:** `POST`
* **Ruta:** `/api/tickets`
* **Cuerpo de la petición (JSON):**
  ```json
  {
    "codigo": "TK-2026-0001",
    "titulo": "Error en el inicio de sesión",
    "descripcion": "Los usuarios reportan fallas con el botón de acceso.",
    "estado": "Abierto",
    "prioridad": "Alta",
    "clienteId": 101,
    "fechaCreacion": "2026-10-18T14:30:00"
  }
