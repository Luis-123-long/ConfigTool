using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LabVIEWConfigSecurity;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;


namespace ConfigTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CryptoHelper _crypto = new CryptoHelper();

        // 这是你的数据源
        public MachineConfig CurrentConfig { get; set; }

        private const string ConfigFileName = "MachineConfig.dat";

        public MainWindow()
        {
            InitializeComponent();
            // 初始化数据
            CurrentConfig = new MachineConfig();

            // 【关键】绑定数据上下文，让界面认识数据
            this.DataContext = CurrentConfig;

            // 记录程序启动
            LogHelper.Info("=== 软件启动 ===");
        }
        private void BtnGetTime_Click(object sender, RoutedEventArgs e)
        {
            DateTime now = DateTime.Now;

            // 更新右工位
            CurrentConfig.RightStation.Year = now.Year;
            CurrentConfig.RightStation.Month = now.Month;
            CurrentConfig.RightStation.Day = now.Day;
            CurrentConfig.RightStation.Hour = now.Hour;
            CurrentConfig.RightStation.Minute = now.Minute;
            CurrentConfig.RightStation.Second = now.Second;

            // 更新左工位 (如果你想两个一起更新)
            CurrentConfig.LeftStation.Year = now.Year;
            CurrentConfig.LeftStation.Month = now.Month;
            CurrentConfig.LeftStation.Day = now.Day;
            CurrentConfig.LeftStation.Hour = now.Hour;
            CurrentConfig.LeftStation.Minute = now.Minute;
            CurrentConfig.LeftStation.Second = now.Second;
            // 【日志】记录操作
            LogHelper.Info($"用户点击了[同步时间]，已将界面参数更新为: {now:yyyy-MM-dd HH:mm:ss}");

            MessageBox.Show("时间已同步！", "提示");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // A. 把对象转成 JSON 字符串 (包含左右工位所有数据)
                string json = JsonConvert.SerializeObject(CurrentConfig, Formatting.Indented);

                // B. 加密
                string encryptedData = _crypto.Encrypt(json);

                // 【新增】保存前记录日志
                LogHelper.Info("正在保存主配置 (MachineConfig)...");

                // C. 弹出“保存文件”对话框，让用户自定义保存路径
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "点检加密配置文件 (*.dat)|*.dat";
                saveDialog.FileName = ConfigFileName;
                // 可选：设置默认保存路径
                // saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, encryptedData);
                    // 【新增】记录成功保存的路径
                    LogHelper.Info($"点检配置保存成功！路径: {saveDialog.FileName}");
                    MessageBox.Show($"保存成功！\n文件已生成: {saveDialog.FileName}", "成功");
                }
                // 用户取消则不做任何操作
            }
            catch (Exception ex)
            {
                // 记录错误详情
                LogHelper.Error("点检配置保存失败",ex);
                MessageBox.Show($"保存失败: {ex.Message}", "错误");
            }
        }

        // 读取主配置文件 (点检参数)
        private void BtnRead_Click(object sender, RoutedEventArgs e) 
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 1. 设置过滤器，只看 dat 文件
            openFileDialog.Filter = "加密主配置文件 (*.dat)|*.dat";
            openFileDialog.Title = "请选择点检参数配置文件";

            // 2. 弹窗选择
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // [日志] 记录用户选了哪个文件
                    LogHelper.Info($"用户尝试读取点检配置: {openFileDialog.FileName}");

                    // 3. 读取密文
                    string encryptedContent = File.ReadAllText(openFileDialog.FileName);

                    // 4. 解密 -> 得到 JSON 字符串
                    string jsonContent = _crypto.Decrypt(encryptedContent);

                    // 5. 反序列化 (JSON -> 对象)
                    // 注意：这里要引用 Newtonsoft.Json
                    var loadedConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<MachineConfig>(jsonContent);

                    if (loadedConfig != null)
                    {
                        // 6. 【关键一步】更新界面数据
                        // 直接把 CurrentConfig 替换成读出来的对象
                        CurrentConfig = loadedConfig;

                        // 强制刷新界面绑定！
                        // 因为直接换了对象，需要告诉界面 DataContext 变了
                        this.DataContext = CurrentConfig;

                         // [日志]
                        LogHelper.Info("点检配置读取成功，界面已刷新。");
                        MessageBox.Show("配置读取成功！界面已更新。");
                    }
                }
                catch (Exception ex)
                {
                    // [日志 
                    LogHelper.Error("点检配置读取失败", ex);
                    MessageBox.Show($"读取失败！\n\n可能原因：\n1. 选错文件了(选成了补偿参数表?)\n2. 密码不对\n\n错误信息: {ex.Message}", "错误");
                }
            }
        }

        // 导入csv
        private void BtnLoadCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV 文件 (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 【新增】记录用户导入了哪个文件
                    LogHelper.Info($"用户尝试导入 CSV: {openFileDialog.FileName}");
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);
                    CurrentConfig.CompensationList.Clear();

                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // 假设 CSV 是：机种名,值1,值2 (hsg1,0,0)
                        string[] parts = line.Split(',');

                        if (parts.Length >= 3)
                        {
                            var item = new CompensationItem();

                            // 第一列：机种名称
                            item.ModelName = parts[0].Trim();

                            // 后面的数值
                            double.TryParse(parts[1], out double v1);
                            item.Value1 = v1;

                            double.TryParse(parts[2], out double v2);
                            item.Value2 = v2;

                            CurrentConfig.CompensationList.Add(item);
                        }
                    }
                    // 【新增】记录导入结果
                    LogHelper.Info($"CSV 导入成功，共加载 {CurrentConfig.CompensationList.Count} 条数据");
                    MessageBox.Show($"导入成功！\n共加载了 {CurrentConfig.CompensationList.Count} 个机种的数据。");
                }
                catch (Exception ex)
                {
                    // 【新增】记录解析失败
                    LogHelper.Error("CSV 解析失败", ex);
                    MessageBox.Show("CSV 解析失败: " + ex.Message);
                }
            }
        }

        // 单独保存补偿表
        private void BtnSaveComp_Click(object sender, RoutedEventArgs e)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var item in CurrentConfig.CompensationList)
            {
                // 保存格式：机种名,值1,值2
                sb.AppendLine($"{item.ModelName},{item.Value1},{item.Value2}");
            }

            // 先转 UTF8 字节，再转 Base64，最后加密
            // 这样 LabVIEW 解密出来就是标准的 UTF8 字符串，不会乱码
            string csvContent = sb.ToString();
            //byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(rawContent);
            //string base64String = Convert.ToBase64String(utf8Bytes);

            //string encryptedData = _crypto.Encrypt(base64String);
            string encryptedData = _crypto.Encrypt(csvContent);
            // 【新增】记录关键动作
            LogHelper.Info($"准备保存补偿表，当前包含 {CurrentConfig.CompensationList.Count} 个机种数据");
            // 3. 弹出“保存文件”对话框
            SaveFileDialog saveDialog = new SaveFileDialog();

            // 设置过滤器：只允许保存为 .dat
            saveDialog.Filter = "加密补偿表 (*.dat)|*.dat";

            // 设置默认文件名 (给用户一个建议的名字，但他可以改)
            // 比如加上日期，方便管理：Compensation_20251230.dat
            saveDialog.FileName = $"Compensation.dat";

            // 设置默认保存路径 (可选，比如默认桌面，不写就是上次打开的地方)
            // saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // 4. 判断用户是否点击了“确定”
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    // saveDialog.FileName 包含了用户选择的完整路径 (例如 F:\Data\MyComp.dat)
                    File.WriteAllText(saveDialog.FileName, encryptedData);
                    // 【新增】闭环记录
                    LogHelper.Info($"补偿表加密保存成功！路径: {saveDialog.FileName}");
                    MessageBox.Show($"保存成功！\n文件路径：{saveDialog.FileName}", "成功");
                }
                catch (Exception ex)
                {
                    LogHelper.Error("补偿表保存失败", ex);
                    MessageBox.Show($"保存失败，请检查路径权限或文件是否被占用。\n{ex.Message}", "错误");
                }
            }
            // 如果用户点了“取消”，什么都不做，流程自然结束

            // ================== 修改结束 ==================

        }
        // 读取加密的 .dat 文件并回显到表格
        private void BtnReadComp_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "加密补偿表 (*.dat)|*.dat";
            openFileDialog.Title = "请选择要查看的加密文件";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 1. 读取文件内容 (此时是乱码)
                    string encryptedContent = File.ReadAllText(openFileDialog.FileName);

                    // 2. 解密 (得到 Base64 字符串)
                    //string base64String = _crypto.Decrypt(encryptedContent);

                    // 3. Base64 -> UTF8 字节 -> 原始 CSV 字符串
                    //byte[] utf8Bytes = Convert.FromBase64String(base64String);
                    //string csvContent = System.Text.Encoding.UTF8.GetString(utf8Bytes);
                    string csvContent = _crypto.Decrypt(encryptedContent);

                    // 【日志】记录解密成功（如果密码错了，这行之前就会报错跳到 catch）
                    LogHelper.Info("文件解密成功，正在解析数据...");
                    // 4. 解析 CSV 字符串 (这部分逻辑和导入 CSV 是一样的)
                    // 先清空当前表格
                    CurrentConfig.CompensationList.Clear();

                    // 按行切割 (兼容各种换行符)
                    string[] lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // 按逗号切割：机种名,值1,值2
                        string[] parts = line.Split(',');

                        if (parts.Length >= 3)
                        {
                            var item = new CompensationItem();
                            item.ModelName = parts[0].Trim();

                            double.TryParse(parts[1], out double v1);
                            item.Value1 = v1;

                            double.TryParse(parts[2], out double v2);
                            item.Value2 = v2;

                            CurrentConfig.CompensationList.Add(item);
                        }
                    }
                    // 【日志】记录最终结果
                    LogHelper.Info($"补偿表加载完毕，成功恢复 {CurrentConfig.CompensationList.Count} 条机种数据");
                    MessageBox.Show("解密并加载成功！\n您可以直接在表格中查看或修改数据。");
                }
                catch (Exception ex)
                {
                    // 【日志】记录详细错误堆栈（这是排查现场问题最有用的信息）
                    LogHelper.Error("读取/解密补偿表失败", ex);
                    MessageBox.Show($"读取失败！\n\n可能原因：\n1. 密钥不匹配\n2. 文件被损坏\n3. 这不是一个有效的补偿表文件\n\n错误信息: {ex.Message}", "错误");
                }
            }
        }
        // Tab 切换事件：控制底部按钮组的显示/隐藏
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. 【关键】过滤掉 Tab 内部子控件（如下拉框）的冒泡事件
            if (!(e.Source is TabControl)) return;

            // 2. 确保控件已加载（防止启动瞬间空引用）
            if (PanelStation == null || PanelLogic == null) return;

            // 3. 获取当前选中的 Tab
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                // 获取 Tab 的标题文字 (Header)
                // 比如: "右工位点检", "逻辑判定参数配置" 等
                string header = selectedTab.Header?.ToString();

                // 4. 判断逻辑
                // 如果标题包含 "逻辑" 或者 "补偿"，就显示第二组按钮
                if (!string.IsNullOrEmpty(header) && (header.Contains("逻辑") || header.Contains("补偿")))
                {
                    // === 切换到 B 组 (逻辑/补偿按钮) ===
                    PanelStation.Visibility = Visibility.Collapsed; // 隐藏原本的时间/保存按钮
                    PanelLogic.Visibility = Visibility.Visible;     // 显示那三个逻辑按钮
                }
                else
                {
                    // === 切换回 A 组 (点检/默认) ===
                    PanelStation.Visibility = Visibility.Visible;   // 显示时间/保存按钮
                    PanelLogic.Visibility = Visibility.Collapsed;   // 隐藏逻辑按钮
                }
            }
        }

    }
}
