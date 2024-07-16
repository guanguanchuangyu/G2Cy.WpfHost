// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace G2Cy.PluginHosting
{
    /// <summary>
    /// Starts hosting logic
    /// </summary>
    /// <remarks>We need this class, because otherwise PluginLoader registration with remoting
    /// does not work. See 
    /// http://stackoverflow.com/questions/18445813/remotingservices-marshal-does-not-work-when-invoked-from-another-appdomain
    /// </remarks>
    public class PluginLoaderBootstrapper : MarshalByRefObject
    {
        private readonly PluginLoader  pluginLoader;
        public PluginLoaderBootstrapper(PluginLoader loader)
        {
            pluginLoader = loader;
        }

        public void Run(string name)
        {
            pluginLoader.Run(name);
        }
    }
}
