using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace LabVIEWConfigSecurity
{
    public class CryptoHelper
    {
        // 预设一个固定的密钥，这样 LabVIEW 端调用时可以偷懒不传密钥
        // 注意：AES-128 需要 16 字节，AES-256 需要 32 字节
        private static readonly string DefaultKey = "MyLabVIEWKey2024"; // 正好16个字符

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">明文 (JSON字符串)</param>
        /// <param name="key">密钥 (可选，留空则使用默认密钥)</param>
        /// <returns>Base64 编码的密文</returns>
        public string Encrypt(string plainText, string key = null)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            if (string.IsNullOrEmpty(key)) key = DefaultKey;

            // 简单处理：确保密钥长度符合要求 (截取前16位)
            var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(16).Substring(0, 16));

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = keyBytes; // 生产环境建议随机IV，这里为了读写方便使用固定IV
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="cipherText">密文 (Base64字符串)</param>
        /// <param name="key">密钥 (可选，留空则使用默认密钥)</param>
        /// <returns>明文</returns>
        public string Decrypt(string cipherText, string key = null)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            if (string.IsNullOrEmpty(key)) key = DefaultKey;

            var keyBytes = Encoding.UTF8.GetBytes(key.PadRight(16).Substring(0, 16));

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = keyBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // 如果解密失败（比如文件被篡改），返回特定标识或空
                return "ERROR_DECRYPT";
            }
        }
    }
}
