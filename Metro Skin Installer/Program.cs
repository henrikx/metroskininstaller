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
        static void Main(string[] args)
        {
            // Load your assembly with the entry point from resources:
            Assembly start = Assembly.Load((byte[])Properties.Resources.Ionic_Zip);
            Type t = start.GetType("Foo.Bar.Program");

            // Install the resolver event
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyName = new AssemblyName(args.Name).Name.Replace('.', '_');

            // Locate and load the contents of the resource 
            byte[] assemblyBytes = (byte[])Properties.Resources.ResourceManager.GetObject(assemblyName, Properties.Resources.Culture);

            // Return the loaded assembly
            return Assembly.Load(assemblyBytes);
        }
    }
}
