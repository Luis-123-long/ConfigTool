using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using Newtonsoft.Json;

namespace LabVIEWConfigSecurity
{

    public class TablePackage
    {
        public string TableName { get; set; } // 表的名字
        public string TableJson { get; set; } // 表的内容（也是个JSON字符串）
    }
    public class AccessHelper
    {
        private string _connString;

        /// <summary>
        /// 构造函数：初始化数据库连接
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        /// <param name="password">密码（如果没有密码，传空字符串 "" 即可）</param>
        public AccessHelper(string dbPath, string password)
        {
            // 1. 判断 Access 版本 (.mdb 还是 .accdb)
            // 这一步决定了我们需要调用系统里的哪个驱动引擎
            string provider = dbPath.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase)
                ? "Microsoft.Jet.OLEDB.4.0"
                : "Microsoft.ACE.OLEDB.12.0";

            // 2. 拼接基础连接字符串
            _connString = $"Provider={provider};Data Source={dbPath};Persist Security Info=False;";

            // 3. ★★★ 核心判断逻辑 ★★★
            // 只有当传入了密码，才把密码拼接到连接字符串里
            // 这样既能开加密的，也能开不加密的
            if (!string.IsNullOrEmpty(password))
            {
                _connString += $"Jet OLEDB:Database Password={password};";
            }
        }

        /// <summary>
        /// 【万能搬运工】
        /// 读取整张表的所有数据，直接打包成 JSON 返回。
        /// 不管表里有什么列（权限、备注、最大值...），统统自动读取。
        /// </summary>
        /// <param name="sql">查询语句，例如 "SELECT * FROM SystemParams"</param>
        /// <summary>
        /// 【UTF-8 专用版】
        /// 直接返回 UTF-8 字节数组，防止 LabVIEW 自动转成 GBK 导致解析报错
        /// </summary>
        public byte[] GetRawTableJsonBytes(string sql)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connString))
                {
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // 1. 先拿到 JSON 字符串
                            string jsonString = JsonConvert.SerializeObject(dt, Formatting.None);

                            // 2. ★★★ 关键步骤 ★★★
                            // 强制转换为 UTF-8 字节数组
                            // 这样传给 LabVIEW 的就是纯正的 UTF-8 数据，LabVIEW 不会乱改
                            return System.Text.Encoding.UTF8.GetBytes(jsonString);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 出错时也返回 UTF-8 字节
                var errObj = new { Error = "DB_Error", Msg = ex.Message };
                string errJson = JsonConvert.SerializeObject(new[] { errObj });
                return System.Text.Encoding.UTF8.GetBytes(errJson);
            }
        }
        /// <summary>
        /// 【上帝视角模式】自动扫描数据库里所有的表，并把它们全部读出来
        /// </summary>
        public string GetAllTables()
        {
            var resultList = new List<TablePackage>();

            try
            {
                using (OleDbConnection conn = new OleDbConnection(_connString))
                {
                    conn.Open();

                    // 1. 获取数据库架构信息 (获取所有表名)
                    // 过滤掉 Access 的系统表 (系统表通常以 MSys 开头)
                    DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                    foreach (DataRow schemaRow in schemaTable.Rows)
                    {
                        string tableName = schemaRow["TABLE_NAME"].ToString();

                        // 2. 针对每一张表，执行查询
                        string sql = $"SELECT * FROM [{tableName}]"; // 加中括号防止表名有空格或关键字

                        using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                        {
                            using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                // 3. 打包：把这张表的数据转成 JSON 字符串，存入包裹
                                // 注意：这里是把 JSON 当作一个字符串存起来，LabVIEW 拿到后再拆包
                                resultList.Add(new TablePackage
                                {
                                    TableName = tableName,
                                    TableJson = JsonConvert.SerializeObject(dt, Formatting.None)
                                });
                            }
                        }
                    }
                }

                // 返回大包裹的 JSON
                return JsonConvert.SerializeObject(resultList);
            }
            catch (Exception ex)
            {
                // 报错返回
                var errList = new List<object> { new { TableName = "ERROR", TableJson = ex.Message } };
                return JsonConvert.SerializeObject(errList);
            }
        }
    }
        

}