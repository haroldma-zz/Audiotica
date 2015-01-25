using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Audiotica.Android.Utilities
{
    /// <summary>
    ///     The <see cref="Crypto" /> provides an easy way encrypt and decrypt
    ///     data using a simple password.
    /// </summary>
    /// <remarks>
    ///     Code based on the book "C# 3.0 in a nutshell by Joseph Albahari" (pages 630-632)
    ///     and from this StackOverflow post by somebody called Brett
    ///     http://stackoverflow.com/questions/202011/encrypt-decrypt-string-in-net/2791259#2791259
    /// </remarks>
    internal static class Crypto
    {
        // Define the secret salt value for encrypting data
        private static readonly byte[] Salt = Encoding.ASCII.GetBytes("ILoveAudiotica6");

        /// <summary>
        ///     Takes the given text string and encrypts it using the given password.
        /// </summary>
        /// <param name="textToEncrypt">Text to encrypt.</param>
        /// <param name="encryptionPassword">Encryption password.</param>
        internal static string Encrypt(string textToEncrypt, string encryptionPassword)
        {
            var algorithm = GetAlgorithm(encryptionPassword);

            //Anything to process?
            if (string.IsNullOrEmpty(textToEncrypt)) return "";

            byte[] encryptedBytes;
            using (var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV))
            {
                var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
                encryptedBytes = InMemoryCrypt(bytesToEncrypt, encryptor);
            }
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        ///     Takes the given encrypted text string and decrypts it using the given password
        /// </summary>
        /// <param name="encryptedText">Encrypted text.</param>
        /// <param name="encryptionPassword">Encryption password.</param>
        internal static string Decrypt(string encryptedText, string encryptionPassword)
        {
            var algorithm = GetAlgorithm(encryptionPassword);

            //Anything to process?
            if (string.IsNullOrEmpty(encryptedText)) return "";

            byte[] descryptedBytes;
            using (var decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV))
            {
                var encryptedBytes = Convert.FromBase64String(encryptedText);
                descryptedBytes = InMemoryCrypt(encryptedBytes, decryptor);
            }
            return Encoding.UTF8.GetString(descryptedBytes);
        }

        /// <summary>
        ///     Performs an in-memory encrypt/decrypt transformation on a byte array.
        /// </summary>
        /// <returns>The memory crypt.</returns>
        /// <param name="data">Data.</param>
        /// <param name="transform">Transform.</param>
        private static byte[] InMemoryCrypt(byte[] data, ICryptoTransform transform)
        {
            var memory = new MemoryStream();
            using (Stream stream = new CryptoStream(memory, transform, CryptoStreamMode.Write))
            {
                stream.Write(data, 0, data.Length);
            }
            return memory.ToArray();
        }

        /// <summary>
        ///     Defines a RijndaelManaged algorithm and sets its key and Initialization Vector (IV)
        ///     values based on the encryptionPassword received.
        /// </summary>
        /// <returns>The algorithm.</returns>
        /// <param name="encryptionPassword">Encryption password.</param>
        private static RijndaelManaged GetAlgorithm(string encryptionPassword)
        {
            // Create an encryption key from the encryptionPassword and salt.
            var key = new Rfc2898DeriveBytes(encryptionPassword, Salt);

            // Declare that we are going to use the Rijndael algorithm with the key that we've just got.
            var algorithm = new RijndaelManaged();
            var bytesForKey = algorithm.KeySize/8;
            var bytesForIv = algorithm.BlockSize/8;
            algorithm.Key = key.GetBytes(bytesForKey);
            algorithm.IV = key.GetBytes(bytesForIv);
            return algorithm;
        }
    }
}