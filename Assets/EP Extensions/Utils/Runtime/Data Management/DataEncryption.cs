using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace EP.Utils.DataManagement
{
    public static class DataEncryption
    {
        // Number of iterations for PBKDF2
        const int Iterations = 1000;

        // Salt size
        const int SaltSize = 16;

        // Key size for the derived key
        const int KeySize = 32;

        /// <summary>
        /// Encrypts the provided data using the user's key.
        /// </summary>
        /// <param name="data">The data to be encrypted.</param>
        /// <param name="userKey">The user's key for encryption.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        public static byte[] EncryptData(string data, string userKey)
        {
            // Generate a random salt
            byte[] salt = GenerateRandomSalt(SaltSize);

            // Derive a secure key from the user's input key and salt using PBKDF2
            byte[] key = DeriveKeyFromUserKey(userKey, salt, KeySize);

            // Perform the encryption using the derived key
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = key;

            // Generate a random IV for each encryption
            aesAlg.IV = new byte[aesAlg.BlockSize / 8];
            using(RNGCryptoServiceProvider rng = new())
            {
                rng.GetBytes(aesAlg.IV);
            }

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using(CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter swEncrypt = new(csEncrypt);
                swEncrypt.Write(data);
            }

            // Combine the salt, IV, and encrypted data into a single byte array
            return salt.Concat(aesAlg.IV).Concat(msEncrypt.ToArray()).ToArray();
        }

        /// <summary>
        /// Decrypts the provided encrypted data using the user's key.
        /// </summary>
        /// <param name="encryptedData">The encrypted data to be decrypted.</param>
        /// <param name="userKey">The user's key for decryption.</param>
        /// <returns>The decrypted string.</returns>
        public static string DecryptData(byte[] encryptedData, string userKey)
        {
            // Extract the salt from the encrypted data
            byte[] salt = encryptedData.Take(SaltSize).ToArray();

            // Perform the decryption using the derived key and extract IV
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = DeriveKeyFromUserKey(userKey, salt, KeySize);
            aesAlg.IV = encryptedData.Skip(SaltSize).Take(aesAlg.BlockSize / 8).ToArray();

            // Create a buffer for the actual encrypted data
            byte[] encryptedBytes = encryptedData.Skip(SaltSize + aesAlg.IV.Length).ToArray();

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new MemoryStream(encryptedBytes);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }

        /// <summary>
        /// Generates a random salt of the specified size in bytes.
        /// </summary>
        /// <param name="size">The size of the salt in bytes.</param>
        /// <returns>A byte array containing the randomly generated salt.</returns>
        static byte[] GenerateRandomSalt(int size)
        {
            byte[] salt = new byte[size];
            using(RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Derives a secure key from the user's input key and salt using PBKDF2.
        /// </summary>
        /// <param name="userKey">The user's input key.</param>
        /// <param name="salt">The salt used for key derivation.</param>
        /// <param name="keySize">The size of the derived key in bytes.</param>
        /// <returns>The derived key.</returns>
        static byte[] DeriveKeyFromUserKey(string userKey, byte[] salt, int keySize)
        {
            using Rfc2898DeriveBytes pbkdf2 = new(userKey, salt, Iterations);
            return pbkdf2.GetBytes(keySize);
        }
    }
}
