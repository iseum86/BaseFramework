using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Base.Data
{
    public partial class AesEncryptor
    {
        private const int BlockSize = 128;
        private const int PasswordRandomizeSaltSize = 16;

        public enum KeySizes
        {
            Key128 = 128,
            Key192 = 192,
            Key256 = 256,
        }

        public static void Decrypt(Stream inputStream, string password, Stream outputStream, bool isCompress = false,
            KeySizes keySize = KeySizes.Key128, CipherMode mode = CipherMode.CBC,
            PaddingMode padding = PaddingMode.PKCS7)
        {
            var blockSizeBytes = BlockSize / 8;
            var keySizeBytes = (int)keySize / 8;

            using (var aes = Aes.Create())
            {
                aes.BlockSize = BlockSize;
                aes.KeySize = (int)keySize;
                aes.Mode = mode;
                aes.Padding = padding;

                // 1. Salt 읽기 및 키 생성
                var saltSize = inputStream.ReadByte();
                if (saltSize == -1) return;

                var salt = new byte[saltSize];
                inputStream.Read(salt, 0, salt.Length);

                // 지정된 salt를 사용하여 유사 난수 키 생성
                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt))
                {
                    aes.Key = deriveBytes.GetBytes(keySizeBytes);
                }

                // 2. IV 읽기
                var iv = new byte[blockSizeBytes];
                inputStream.Read(iv, 0, iv.Length);
                aes.IV = iv;

                // 3. 복호화 및 데이터 복구
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                {
                    if (isCompress)
                    {
                        using (var deflateStream = new DeflateStream(cryptoStream, CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(outputStream);
                        }
                    }
                    else
                    {
                        cryptoStream.CopyTo(outputStream);
                    }
                }
            }
        }
    }
}