using System;
using System.Security.Cryptography;

namespace mtto.Utilidades
{
    /// <summary>
    /// PBKDF2-SHA256 con sal por usuario. El hash se guarda como
    /// "iteraciones.salBase64.hashBase64" en app_usuario.password_hash, así que
    /// el número de iteraciones viaja con el hash: si algún día se sube, los
    /// hashes viejos se siguen verificando con el valor con el que se crearon.
    /// </summary>
    public static class PasswordHasher
    {
        private const int Iteraciones = 100000;
        private const int TamanoSalBytes = 16;
        private const int TamanoHashBytes = 32;

        public static string Hashear(string password)
        {
            var sal = new byte[TamanoSalBytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sal);
            }

            var hash = Derivar(password, sal, Iteraciones);
            return Iteraciones + "." + Convert.ToBase64String(sal) + "." + Convert.ToBase64String(hash);
        }

        public static bool Verificar(string password, string hashGuardado)
        {
            var partes = (hashGuardado ?? "").Split('.');
            if (partes.Length != 3) return false;

            int iteraciones;
            if (!int.TryParse(partes[0], out iteraciones)) return false;

            byte[] sal;
            byte[] hashEsperado;
            try
            {
                sal = Convert.FromBase64String(partes[1]);
                hashEsperado = Convert.FromBase64String(partes[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            return SonIguales(Derivar(password, sal, iteraciones), hashEsperado);
        }

        /// Comparación en tiempo constante: no corta al primer byte distinto,
        /// para no filtrar por tiempo qué tanto acertó una contraseña.
        private static bool SonIguales(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            var diferencia = 0;
            for (var i = 0; i < a.Length; i++)
            {
                diferencia |= a[i] ^ b[i];
            }
            return diferencia == 0;
        }

        private static byte[] Derivar(string password, byte[] sal, int iteraciones)
        {
            using (var kdf = new Rfc2898DeriveBytes(password, sal, iteraciones, HashAlgorithmName.SHA256))
            {
                return kdf.GetBytes(TamanoHashBytes);
            }
        }
    }
}
