using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace SecureNote
{
    class Crypto
    {
        public string ByteArrayToString(byte[] ba)
        {
            try
            {
                StringBuilder hex = new StringBuilder(ba.Length * 2);
                foreach (byte b in ba)
                    hex.AppendFormat("{0:x2}", b);
                return hex.ToString();
            }
            catch { return String.Empty; }
        }

        public string RemoveWhitespace(string str)
        {
            try
            {
                return string.Join(String.Empty, str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            }
            catch { return String.Empty; }
        }

        public int RSAStorePublicKey(string filename, string PublicKey)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    sw.WriteLine("-----BEGIN PUBLIC KEY-----");
                    sw.WriteLine(PublicKey);
                    sw.WriteLine("-----END PUBLIC KEY-----");
                }
                return 0;
            }
            catch { return 1; }
        }

        public string RSAReadPublicKey(string filename)
        {
            try
            {
                string pKey;

                pKey = File.ReadAllText(filename);

                pKey = RemoveWhitespace(pKey);
                pKey = pKey.Replace("BEGINPUBLICKEY", String.Empty);
                pKey = pKey.Replace("ENDPUBLICKEY", String.Empty);
                pKey = pKey.Replace("-", String.Empty);

                return pKey;
            }
            catch { return String.Empty; }
        }

        public int RSAStorePrivateKey(string filename, string PrivateKey, string password)
        {
            try
            {
                RijndaelManaged AES = new RijndaelManaged();

                AES.KeySize = 256;
                AES.GenerateIV();

                byte[] Passkey = AESCreateKey(password);

                string pKey = Convert.ToBase64String(IVCipher(AES.IV, AESEncrypt(Encoding.UTF8.GetBytes(PrivateKey), Passkey, AES.IV)),Base64FormattingOptions.InsertLineBreaks);

                if (AES != null) AES.Dispose();

                using (StreamWriter sw = new StreamWriter(filename))
                {
                    sw.WriteLine("-----BEGIN PRIVATE KEY-----");
                    sw.WriteLine(pKey);
                    sw.WriteLine("-----END PRIVATE KEY-----");
                }

                return 0;
            }
            catch { return 1; }
        }

        public string RSAReadPrivateKey(string filename, string password)
        {
            try
            {
                string PKey;
                byte[] IVCipher;
                byte[] IV;
                byte[] Cipher;

                byte[] Passkey = AESCreateKey(password);

                PKey = File.ReadAllText(filename);

                PKey = RemoveWhitespace(PKey);
                PKey = PKey.Replace("BEGINPRIVATEKEY", String.Empty);
                PKey = PKey.Replace("ENDPRIVATEKEY", String.Empty);
                PKey = PKey.Replace("-", String.Empty);

                IVCipher = Convert.FromBase64String(PKey);
                IV = GetIVfromIVCipher(IVCipher);
                Cipher = GetCipherfromIVCipher(IVCipher);

                return Encoding.UTF8.GetString(AESDecrypt(Cipher, Passkey, IV));
            }
            catch { return String.Empty; }
        }

        public byte[] KeyIV(byte[] Key, byte[] IV)
        {
            try
            {
                byte[] outputBytes = new byte[Key.Length + IV.Length];
                Buffer.BlockCopy(Key, 0, outputBytes, 0, Key.Length);
                Buffer.BlockCopy(IV, 0, outputBytes, Key.Length, IV.Length);
                return outputBytes;
            }
            catch { return null; }
        }

        public byte[] GetKeyfromKeyIV(byte[] KeyIV)
        {
            try
            {
                byte[] output = new byte[32];
                int x;

                for (x = 0; x < 32; x++)
                {
                    output[x] = KeyIV[x];
                }

                return output;
            }
            catch { return null; }
        }

        public byte[] GetIVfromKeyIV(byte[] KeyIV)
        {
            try
            {
                byte[] output = new byte[16];
                int x;

                for (x = 0; x < 16; x++)
                {
                    output[x] = KeyIV[x + 32];
                }

                return output;
            }
            catch { return null; }
        }

        public byte[] IVCipher(byte[] IV, byte[] Cipher)
        {
            try
            {
                byte[] outputBytes = new byte[IV.Length + Cipher.Length];
                Buffer.BlockCopy(IV, 0, outputBytes, 0, IV.Length);
                Buffer.BlockCopy(Cipher, 0, outputBytes, IV.Length, Cipher.Length);
                return outputBytes;
            }
            catch { return null; }
        }

        public byte[] GetIVfromIVCipher(byte[] IVCipher)
        {
            try
            {
                byte[] output = new byte[16];
                int x;

                for (x = 0; x < 16; x++)
                {
                    output[x] = IVCipher[x];
                }

                return output;
            }
            catch { return null; }
        }

        public byte[] GetCipherfromIVCipher(byte[] IVCipher)
        {
            try
            {
                byte[] output = new byte[IVCipher.Length - 16];
                int x;

                for (x = 0; x < IVCipher.Length - 16; x++)
                {
                    output[x] = IVCipher[x + 16];
                }

                return output;
            }
            catch { return null; }
        }

        public byte[] KeyIVCipher(byte[] KeyIV, byte[] Cipher)
        {
            try
            {
                byte[] outputBytes = new byte[KeyIV.Length + Cipher.Length];
                Buffer.BlockCopy(KeyIV, 0, outputBytes, 0, KeyIV.Length);
                Buffer.BlockCopy(Cipher, 0, outputBytes, KeyIV.Length, Cipher.Length);
                return outputBytes;
            }
            catch { return null; }
        }

        public byte[] GetKeyIVfromKeyIVCipher(byte[] KeyIVCipher)
        {
            try
            {
                byte[] output = new byte[256];
                int x;

                for (x = 0; x < 256; x++)
                {
                    output[x] = KeyIVCipher[x];
                }

                return output;
            }
            catch { return null; }
        }

        public byte[] GetCipherfromKeyIVCipher(byte[] KeyIVCipher)
        {
            try
            {
                byte[] output = new byte[KeyIVCipher.Length - 256];
                int x;

                for (x = 0; x < KeyIVCipher.Length - 256; x++)
                {
                    output[x] = KeyIVCipher[x + 256];
                }

                return output;
            }
            catch { return null; }
        }

        public Tuple<string, string> RSACreateKeyPair()
        {
            try
            {
                CspParameters cspParams = new CspParameters { ProviderType = 1 };

                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(2048, cspParams);

                string publicKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(false), Base64FormattingOptions.InsertLineBreaks);
                string privateKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(true), Base64FormattingOptions.InsertLineBreaks);

                return new Tuple<string, string>(privateKey, publicKey);
            }
            catch { return null; }
        }

        public byte[] RSAEncrypt(string publicKey, byte[] plainBytes)
        {
            try
            {
                CspParameters cspParams = new CspParameters { ProviderType = 1 };
                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);

                rsaProvider.ImportCspBlob(Convert.FromBase64String(publicKey));

                byte[] encryptedBytes = rsaProvider.Encrypt(plainBytes, false);

                return encryptedBytes;
            }
            catch { return null; }
        }

        public byte[] RSADecrypt(string privateKey, byte[] encryptedBytes)
        {
            try
            {
                CspParameters cspParams = new CspParameters { ProviderType = 1 };
                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);

                rsaProvider.ImportCspBlob(Convert.FromBase64String(privateKey));

                byte[] plainBytes = rsaProvider.Decrypt(encryptedBytes, false);

                return plainBytes;
            }
            catch { return null; }
        }

        public byte[] AESCreateKey(string password)
        {
            try
            {
                byte[] SALT = new byte[] { 0x71, 0xa6, 0xf8, 0xa7, 0x14, 0xdf, 0x34, 0xc1, 0x5c, 0xe7, 0xbd, 0xc9, 0x3a, 0x14, 0xbd, 0xbc };

                const int Iterations = 10000;
                var keyGenerator = new Rfc2898DeriveBytes(password, SALT, Iterations);
                return keyGenerator.GetBytes(32);
            }
            catch { return null; }
        }

        public byte[] AESEncrypt(byte[] data, byte[] Key, byte[] IV)
        {
            try
            {
                if (data == null || data.Length <= 0) return null;
                if (Key == null || Key.Length < 32) return null;
                if (IV == null || IV.Length < 16) return null;

                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;

                    using (ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV))
                    {
                        return PerformCryptography(encryptor, data);
                    }

                }
            }
            catch { return null; }
        }

        public byte[] AESDecrypt(byte[] data, byte[] Key, byte[] IV)
        {
            try
            {
                if (data == null || data.Length <= 0) return null;
                if (Key == null || Key.Length < 32) return null;
                if (IV == null || IV.Length < 16) return null;

                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.Key = Key;
                    rijAlg.IV = IV;

                    using (ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV))
                    {
                        return PerformCryptography(decryptor, data);
                    }

                }
            }
            catch { return null; }
        }

        private byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                        return memoryStream.ToArray();
                    }
                }
            }
            catch { return null; }
        }
    }
}
