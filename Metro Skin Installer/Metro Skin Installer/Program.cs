using System;
using System.Text;
using System.Windows.Forms;

namespace Metro_Skin_Installer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            catch (Exception ex) //handle anything not handled to avoid "freezing"
            {
                _ = MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Unhandled exception occured!");
                Application.Exit();
            }
        }
    }
}
