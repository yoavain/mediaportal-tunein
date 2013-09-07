using System;
using System.Security.Cryptography;
using System.Text;

namespace RadioTimePlugin
{
    public class PasswordUtility
    {
        private static readonly byte[] Entropy = Encoding.Unicode.GetBytes("Your radiotime password is incorrect");

        public static string EncryptData(string decryptedString, DataProtectionScope scope)
        {
            if (string.IsNullOrEmpty(decryptedString))
            {
                throw new ArgumentException("decryptedString");
            }

            try
            {
                // To byte array
                var decryptedBytes = Encoding.ASCII.GetBytes(decryptedString);

                // Encrypt the data
                var encrptedBytes = ProtectedData.Protect(decryptedBytes, Entropy, scope);

                // Return as base64 string
                return Convert.ToBase64String(encrptedBytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DecryptData(string encrptedPassword, DataProtectionScope scope)
        {
            if (string.IsNullOrEmpty(encrptedPassword))
            {
                throw new ArgumentException("encrptedPassword");
            }

            try
            {
                // From base64
                var encryptedBytes = Convert.FromBase64String(encrptedPassword);

                // Decrypt the data
                var decryptedData = ProtectedData.Unprotect(encryptedBytes, Entropy, scope);

                // Convert to string
                return Encoding.Default.GetString(decryptedData);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}