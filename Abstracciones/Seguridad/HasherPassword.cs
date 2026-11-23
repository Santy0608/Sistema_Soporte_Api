using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Abstracciones.Seguridad
{
    public static class HasherPassword
    {
        private const int Iteraciones = 100_000;

        public static string Hashear(string password)
        {
            var sal = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
            return $"{Iteraciones}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verificar(string password, string almacenado)
        {
            var partes = almacenado.Split('.');
            if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones)) return false;
            var sal = Convert.FromBase64String(partes[1]);
            var esperado = Convert.FromBase64String(partes[2]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(password, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);   // comparación en tiempo constante
        }
    }
}
