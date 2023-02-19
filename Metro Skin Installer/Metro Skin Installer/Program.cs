using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Text;

namespace Metro_Skin_Installer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            catch (Exception ex) //handle anything not handled to avoid "freezing"
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Unhandled exception occured!");
                Application.Exit();
            }
        }
    }
}
