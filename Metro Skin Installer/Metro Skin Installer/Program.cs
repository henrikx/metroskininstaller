using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

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
#if !DEBUG
            Assembly start = Assembly.Load((byte[])Properties.Resources.Ionic_Zip);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
#if !DEBUG
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyName = new AssemblyName(args.Name).Name.Replace('.', '_');
            byte[] assemblyBytes = (byte[])Properties.Resources.ResourceManager.GetObject(assemblyName, Properties.Resources.Culture);
            return Assembly.Load(assemblyBytes);
        }
#endif
    }
}
