using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Base.Data
{
    public partial class AesEncryptor
    {
        public static void Encrypt(Stream inputStream, string password, Stream outputStream, bool isCompress = false,
            KeySizes keySize = KeySizes.Key128, CipherMode mode = CipherMode.CBC,
            PaddingMode padding = PaddingMode.PKCS7)
        {
            var keySizeBytes = (int)keySize / 8;

            using (var aes = Aes.Create())
            {
                aes.BlockSize = BlockSize;
                aes.KeySize = (int)keySize;
                aes.Mode = mode;
                aes.Padding = padding;

                var deriveBytes = new Rfc2898DeriveBytes(password, PasswordRandomizeSaltSize);
                var salt = deriveBytes.Salt;

                aes.Key = deriveBytes.GetBytes(keySizeBytes);
                aes.GenerateIV();

                using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                {
                    outputStream.WriteByte((byte)salt.Length);
                    outputStream.Write(salt, 0, salt.Length);
                    outputStream.Write(aes.IV, 0, aes.IV.Length);

                    if (isCompress)
                    {
                        using (var deflateStream = new DeflateStream(cryptoStream, CompressionMode.Compress))
                        {
                            inputStream.CopyTo(deflateStream);
                        }
                    }
                    else
                    {
                        inputStream.CopyTo(cryptoStream);
                    }
                }
            }
        }
    }
}