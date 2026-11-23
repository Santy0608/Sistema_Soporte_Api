using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using static Web.Models.Etiquetas;

namespace Web.Servicios
{
    public class EtiquetasApi : IEtiquetasApi
    {
        private readonly HttpClient _http;

        public EtiquetasApi(HttpClient http) => _http = http;

        public async Task<List<EtiquetaVm>> ListarAsync(CancellationToken ct = default)
        {
            var dtos = await _http.GetFromJsonAsync<List<EtiquetaDto>>("api/etiquetas", ct)
                       ?? new List<EtiquetaDto>();

            return dtos.Select(MapearAVm).ToList();
        }

        public async Task<EtiquetaVm?> ObtenerAsync(int id, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"api/etiquetas/{id}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();

            var dto = await resp.Content.ReadFromJsonAsync<EtiquetaDto>(ct);
            return dto is null ? null : MapearAVm(dto);
        }

        public async Task<(bool Ok, EtiquetaVm? Creada, string? Error)> CrearAsync(
            EtiquetaCrearVm vm, CancellationToken ct = default)
        {
            var payload = MapearACrearDto(vm);
            var resp = await _http.PostAsJsonAsync("api/etiquetas", payload, ct);

            if (resp.IsSuccessStatusCode)
            {
                var creadaDto = await resp.Content.ReadFromJsonAsync<EtiquetaDto>(ct);
                return (true, creadaDto is null ? null : MapearAVm(creadaDto), null);
            }

            if (resp.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
            {
                var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>(ct);
                var mensaje = problem?.Detail ?? problem?.Title ?? "No se pudo completar la operación.";
                return (false, null, mensaje);
            }

            resp.EnsureSuccessStatusCode();
            return (false, null, "Error inesperado al comunicarse con el API.");
        }

        // Recibe EtiquetaDto y devuelve EtiquetaVm
        private static EtiquetaVm MapearAVm(EtiquetaDto dto) => new()
        {
            Id = dto.IdEtiqueta, // Asegúrate de que EtiquetaDto tenga propiedad pública Id (o dto.IdEtiqueta si así se llama)
            Nombre = dto.Nombre,
            Color = dto.Color
        };

        // Recibe EtiquetaCrearVm y devuelve EtiquetaCrearDto
        private static EtiquetaCrearDto MapearACrearDto(EtiquetaCrearVm vm) => new()
        {
            Nombre = vm.Nombre,
            Color = vm.Color
        };
    }
}