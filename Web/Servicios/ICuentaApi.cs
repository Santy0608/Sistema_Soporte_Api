using Web.Models;

namespace Web.Servicios;

/// <summary>Lo que el API devuelve al iniciar sesión (coincide con SesionDto del API).</summary>
public record SesionApi(string Token, DateTime ExpiraEnUtc, string Nombre, string Correo, string Rol);

/// <summary>Cliente tipado SOLO para el login. No lleva el handler del token porque aún no hay token.</summary>
public interface ICuentaApi
{
    Task<SesionApi> IniciarSesionAsync(LoginVm datos, CancellationToken ct = default);
}
