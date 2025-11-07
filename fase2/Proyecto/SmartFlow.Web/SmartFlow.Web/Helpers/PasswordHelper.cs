using System.Security.Cryptography;
using System.Text;

namespace SmartFlow.Web.Helpers
{
    public static class PasswordHelper
    {
        // Genera un hash SHA256 a partir de una contraseña en texto plano
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // Compara una contraseña en texto plano con su hash guardado
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
