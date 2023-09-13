using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using UserService.Data;
using UserService.Models;

namespace UserService.Security
{
    public class Token
    {
        public int UserId { get; set; }
        public DateTime Expires { get; set; }
    }

    public static class TokenHelper
    {
        public static string? GetToken(string userName, string password, IUserRepository userRepository)
        {
            // do a db lookup to confirm the userName and password
            User? user = userRepository.Users.FirstOrDefault(u => u.Email == userName && u.Password == password);

            if (user != null)
            {
                // create the token
                Token token = new Token
                {
                    UserId = (int)user.Id,
                    Expires = DateTime.UtcNow.AddMinutes(1),
                };
                string jsonString = JsonSerializer.Serialize(token);
                string encryptedJsonString = Crypto.EncryptStringAES(jsonString);
                return encryptedJsonString;
            }
            else
            {
                return null;
            }    
        }

        public static Token? DecodeToken(string token)
        {
            string decryptedJsonString = Crypto.DecryptStringAES(token);
            Token? tokenObject = JsonSerializer.Deserialize<Token>(decryptedJsonString);
            if (tokenObject == null || tokenObject.Expires < DateTime.UtcNow)
            {
                return null;
            }
            return tokenObject;
        }
    }

    public class Crypto
    {
        private static readonly byte[] Salt =
            Encoding.ASCII.GetBytes("B78A07A7-14D8-4890-BC99-9145A14713C1");
        private const string Password = "sharedSecretPassword";
        /// <summary>
        /// Encrypt the given string using AES.
        /// The string can be decrypted using DecryptStringAES().
        ///</summary>
        /// <param name="plainText">The text to encrypt.</param>
        public static string EncryptStringAES(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException("plainText");
            }

            string outStr;                   // Encrypted string to return
            RijndaelManaged aesAlg = null;   // Used to encrypt the data
            try
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(Password, Salt);
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }
            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using
        /// EncryptStringAES(), using an identical password.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        public static string DecryptStringAES(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentNullException("cipherText");
            }

            RijndaelManaged aesAlg = null;
            string plaintext;
            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(Password, Salt);

                // Create the streams used for decryption.
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }
            return plaintext;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }
            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }
            return buffer;
        }
    }
}
