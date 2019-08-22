//
// Author: B4rtik (@b4rtik)
// Project: RedPeanut (https://github.com/b4rtik/RedPeanut)
// License: BSD 3-Clause
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace RedPeanutAgent.Core
{
    class Crypto
    {
        public static class RC4
        {
            public static string DecryptMessage(string sessionkey, string input)
            {
                return RC4.Decrypt(sessionkey, input);
            }

            public static string EncryptMessage(string sessionkey, string input)
            {
                return RC4.Encrypt(sessionkey, input);
            }

            public static string Encrypt(string key, string data)
            {
                Encoding unicode = Encoding.Unicode;

                return Convert.ToBase64String(Encrypt(unicode.GetBytes(key), unicode.GetBytes(data)));
            }

            public static string Decrypt(string key, string data)
            {
                Encoding unicode = Encoding.Unicode;

                return unicode.GetString(Encrypt(unicode.GetBytes(key), Convert.FromBase64String(data)));
            }

            public static byte[] Encrypt(byte[] key, byte[] data)
            {
                return EncryptOutput(key, data).ToArray();
            }

            public static byte[] Decrypt(byte[] key, byte[] data)
            {
                return EncryptOutput(key, data).ToArray();
            }

            private static byte[] EncryptInitalize(byte[] key)
            {
                byte[] s = Enumerable.Range(0, 256)
                  .Select(i => (byte)i)
                  .ToArray();

                for (int i = 0, j = 0; i < 256; i++)
                {
                    j = (j + key[i % key.Length] + s[i]) & 255;

                    Swap(s, i, j);
                }

                return s;
            }

            private static IEnumerable<byte> EncryptOutput(byte[] key, IEnumerable<byte> data)
            {
                byte[] s = EncryptInitalize(key);

                int i = 0;
                int j = 0;

                return data.Select((b) =>
                {
                    i = (i + 1) & 255;
                    j = (j + s[i]) & 255;

                    Swap(s, i, j);

                    return (byte)(b ^ s[(s[i] + s[j]) & 255]);
                });
            }

            private static void Swap(byte[] s, int i, int j)
            {
                byte c = s[i];

                s[i] = s[j];
                s[j] = c;
            }
        }

        public class Aes
        {
            public static string DecryptAesMessage(byte[] input, byte[] aeskey, byte[] aesiv)
            {
                return Decrypt(input, aeskey, aesiv);
            }

            public static byte[] EncryptAesMessage(string input, byte[] aeskey, byte[] aesiv)
            {
                return Encrypt(input, aeskey, aesiv);
            }

            static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
            {
                byte[] encrypted;
                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                                sw.Write(plainText);
                            encrypted = ms.ToArray();
                        }
                    }
                }
                return encrypted;
            }

            static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
            {
                string plaintext = null;
                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                    using (MemoryStream ms = new MemoryStream(cipherText))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs))
                                plaintext = reader.ReadToEnd();
                        }
                    }
                }
                return plaintext;
            }
        }
    }
}
