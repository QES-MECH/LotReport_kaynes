using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LotReport.Utilities
{
    public class PasswordEncrypter
    {
        public string Encrypt(string password)
        {
            byte[] data = Encoding.UTF8.GetBytes(password);
            byte[] entropy = new byte[10];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            byte[] protectedData = ProtectedData.Protect(data, entropy, DataProtectionScope.CurrentUser);
            return $"{Convert.ToBase64String(protectedData)}|{Convert.ToBase64String(entropy)}";
        }

        public string Decrypt(string encryptedPassword)
        {
            string[] protectedParts = encryptedPassword.Split('|');
            byte[] protectedData = Convert.FromBase64String(protectedParts[0]);
            byte[] entropy = Convert.FromBase64String(protectedParts[1]);

            byte[] unprotectedData = ProtectedData.Unprotect(protectedData, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(unprotectedData);
        }
    }
}
