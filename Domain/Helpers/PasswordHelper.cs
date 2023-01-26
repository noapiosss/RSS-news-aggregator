using System.Security.Cryptography;
using System.Text;
using Domain.Helpers.Interfaces;

namespace Domain.Helpers
{
    public class PasswordHelper : IPasswordHelper
    {
        public string GetSHA256(string password)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder sb = new();
            for (int i = 0; i < bytes.Length; ++i)
            {
                _ = sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}