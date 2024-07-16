using dotnetCampus.Ipc.CompilerServices.Attributes;
using G2Cy.WpfHost;
using G2Cy.WpfHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace G2Cy.PluginHosting
{
    [IpcPublic(IgnoresIpcException = true, Timeout = 5000)]
    public interface IPluginLoader : IDisposable
    {
        IRemotePlugin LoadPlugin(PluginStartupInfo startupInfo);
    }
}
