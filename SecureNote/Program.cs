using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Deployment.Application;


namespace SecureNote
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                string[] activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;

                if (activationData != null && activationData.Length > 0)
                {
                    string[] args = activationData[0].Split(new char[] { ',' });
                    if (args.Length > 0)
                    {
                        // Parameters
                        Application.Run(new frmSecureNote(args[0]));
                    }
                }
                else
                    // No Parameters
                    Application.Run(new frmSecureNote(string.Empty));
            }
            else
                // Open in dev environment
                Application.Run(new frmSecureNote(string.Empty));
        }
    }
}
