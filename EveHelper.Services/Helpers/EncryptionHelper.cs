using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EveHelper.Services.Helpers
{
    /// <summary>
    /// Helper class for encrypting and decrypting sensitive data
    /// </summary>
    public static class EncryptionHelper
    {
        private const int SaltSize = 32; // 256 bits
        private const int IvSize = 16; // 128 bits for AES
        private const int KeySize = 32; // 256 bits for AES-256

        /// <summary>
        /// Encrypts text using AES-256 with PBKDF2 key derivation
        /// </summary>
        /// <param name="plaintext">Text to encrypt</param>
        /// <param name="password">Password for encryption</param>
        /// <param name="iterations">PBKDF2 iterations</param>
        /// <returns>Base64-encoded encrypted data</returns>
        public static string Encrypt(string plaintext, string password, int iterations = 10000)
        {
            if (string.IsNullOrEmpty(plaintext))
                throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Generate random salt and IV
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            rng.GetBytes(salt);
            rng.GetBytes(iv);

            // Derive key from password
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);

            // Encrypt the data
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(plaintextBytes, 0, plaintextBytes.Length);
                csEncrypt.FlushFinalBlock();
            }

            var ciphertext = msEncrypt.ToArray();

            // Combine salt + IV + ciphertext
            var result = new byte[SaltSize + IvSize + ciphertext.Length];
            Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
            Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
            Buffer.BlockCopy(ciphertext, 0, result, SaltSize + IvSize, ciphertext.Length);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decrypts text using AES-256 with PBKDF2 key derivation
        /// </summary>
        /// <param name="encryptedData">Base64-encoded encrypted data</param>
        /// <param name="password">Password for decryption</param>
        /// <param name="iterations">PBKDF2 iterations</param>
        /// <returns>Decrypted plaintext</returns>
        public static string Decrypt(string encryptedData, string password, int iterations = 10000)
        {
            if (string.IsNullOrEmpty(encryptedData))
                throw new ArgumentException("Encrypted data cannot be null or empty", nameof(encryptedData));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                var data = Convert.FromBase64String(encryptedData);

                if (data.Length < SaltSize + IvSize + 1)
                    throw new ArgumentException("Invalid encrypted data format");

                // Extract salt, IV, and ciphertext
                var salt = new byte[SaltSize];
                var iv = new byte[IvSize];
                var ciphertext = new byte[data.Length - SaltSize - IvSize];

                Buffer.BlockCopy(data, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(data, SaltSize, iv, 0, IvSize);
                Buffer.BlockCopy(data, SaltSize + IvSize, ciphertext, 0, ciphertext.Length);

                // Derive key from password
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var key = pbkdf2.GetBytes(KeySize);

                // Decrypt the data
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(ciphertext);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);

                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new InvalidOperationException("Failed to decrypt data. Invalid password or corrupted data.", ex);
            }
        }

        /// <summary>
        /// Generates a cryptographically secure random password
        /// </summary>
        /// <param name="length">Password length</param>
        /// <returns>Random password</returns>
        public static string GenerateSecurePassword(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using var rng = RandomNumberGenerator.Create();
            var data = new byte[length];
            rng.GetBytes(data);

            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[data[i] % chars.Length]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Securely generates a machine-specific encryption key
        /// </summary>
        /// <returns>Machine-specific key</returns>
        public static string GetMachineKey()
        {
            // Use machine name and user as base for key derivation
            var machineInfo = $"{Environment.MachineName}_{Environment.UserName}_{Environment.OSVersion.VersionString}";
            
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineInfo));
            return Convert.ToBase64String(hashBytes);
        }
    }
} 