using System;
using System.IO;
using System.Security.Cryptography;

namespace FalconNetSdk.Scripts.Bigdata.Encrypts.Aes
{
    public static class AesService
    {
        public static byte[] GenerateAesKey()
        {
            using var aesAlg = System.Security.Cryptography.Aes.Create();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.GenerateKey();
            return aesAlg.Key;
        }
        
        public static string GenerateAesKeyStr()
        {
            return Convert.ToBase64String(GenerateAesKey());
        }

        public static AesEncrypted Encrypt(byte[] key, byte[] content)
        {
            using var aesAlg = System.Security.Cryptography.Aes.Create();
            
            aesAlg.Key = key;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.GenerateIV();

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(content, 0, content.Length);
            cryptoStream.FlushFinalBlock();
            var encrypted = memoryStream.ToArray();
            return new AesEncrypted( encrypted, aesAlg.IV);
        }

        public static byte[] Decrypt(byte[] key, AesEncrypted encrypted)
        {
            using var aesAlg = System.Security.Cryptography.Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = encrypted.Iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            // Create a decryptor to perform the stream transform.
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            // Create the streams used for decryption.
            using var memoryStream = new MemoryStream(encrypted.Data);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            return ReadFully(cryptoStream);
        }

        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}