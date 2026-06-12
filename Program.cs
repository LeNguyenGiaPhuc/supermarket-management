using System;
using System.Windows.Forms;

namespace Supermarket
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 1) Hiện LoginForm modal
            var login = new LoginForm();
            login.ShowDialog();

            // 2) Nếu login trả về OK thì lấy Role và chạy frmHome
            if (login.DialogResult == DialogResult.OK)
            {
                Application.Run(new frmHome(login.UserRole));
            }
        }
    }
}

