using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ServicesA07
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            var serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = "GameServerService",
                DisplayName = "Game Server Service",
                Description = "A TCP server hosting a game as a Windows Service."
            };

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
