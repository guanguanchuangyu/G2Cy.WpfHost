using G2Cy.WpfHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSubscribe
{
    public class Plugin : PluginBase
    {
        public override object CreateControl()
        {
            return new MainControl();
        }
    }
}
