using System;
using System.IO;
using Newtonsoft.Json;

namespace LabVIEWConfigSecurity
{
    public class AutoUpdater
    {
        /// <summary>
        /// 自动更新时间
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="stationName">工位 (Left/Right)</param>
        /// <param name="isPassed">【新增参数】 true=合格(写当前时间), false=失败(写一年前)</param>
        /// <returns>是否成功</returns>
        public static bool UpdateInspectionTime(string filePath, string stationName, bool isPassed)
        {
            try
            {
                if (!File.Exists(filePath)) return false;

                // 1. 解密 & 反序列化
                string encryptedContent = File.ReadAllText(filePath);
                CryptoHelper cryptoHelper = new CryptoHelper(); // 创建 CryptoHelper 实例
                string jsonContent = cryptoHelper.Decrypt(encryptedContent); // 使用实例调用 Decrypt 方法
                var config = JsonConvert.DeserializeObject<MachineConfig>(jsonContent);
                if (config == null) return false;

                // 2. 【核心逻辑修改】确定要写入的时间
                DateTime timeToWrite;
                if (isPassed)
                {
                    timeToWrite = DateTime.Now; // 合格：写现在
                }
                else
                {
                    // 失败：写一年前 (让它立刻过期)
                    timeToWrite = DateTime.Now.AddYears(-1);
                }

                // 3. 更新对应的工位
                if (stationName.Equals("Left", StringComparison.OrdinalIgnoreCase))
                {
                    SetTime(config.LeftStation, timeToWrite);
                }
                else if (stationName.Equals("Right", StringComparison.OrdinalIgnoreCase))
                {
                    SetTime(config.RightStation, timeToWrite);
                }
                else
                {
                    return false;
                }

                // 4. 加密 & 回写
                string newJson = JsonConvert.SerializeObject(config);
                string newEncrypted = cryptoHelper.Encrypt(newJson); // 使用实例调用 Encrypt 方法
                File.WriteAllText(filePath, newEncrypted);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // 辅助方法不需要变
        private static void SetTime(StationConfig s, DateTime dt)
        {
            s.Year = dt.Year;
            s.Month = dt.Month;
            s.Day = dt.Day;
            s.Hour = dt.Hour;
            s.Minute = dt.Minute;
            s.Second = dt.Second;
        }
    }
}