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
using System.Windows.Shapes;
using System.IO;

namespace ConfigTool
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        // 默认密码文件名为 admin_pass.txt
        private const string PasswordFile = "admin_pass.txt";

        public LoginWindow()
        {
            InitializeComponent();
            txtPassword.Focus(); // 假设 XAML 里有一个叫 txtPassword 的 PasswordBox
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string truePwd = "admin"; // 默认硬编码密码

            // 尝试读取同目录下的密码文件
            if (File.Exists(PasswordFile))
            {
                string filePwd = File.ReadAllText(PasswordFile).Trim();
                if (!string.IsNullOrEmpty(filePwd)) truePwd = filePwd;
            }

            if (txtPassword.Password == truePwd) this.DialogResult = true;
            else MessageBox.Show("密码错误");
        }
    }
}
