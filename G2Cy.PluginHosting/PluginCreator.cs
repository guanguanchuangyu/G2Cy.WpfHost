// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using G2Cy.WpfHost.Interfaces;

namespace G2Cy.PluginHosting
{
    internal static class PluginCreator
    {
        public static object CreatePlugin(string assemblypath, string typeName, IWpfHost host)
        {
            var assembly = Assembly.LoadFile(assemblypath);
            var type = assembly.GetType(typeName);

            if (type == null) throw new InvalidOperationException("Could not find type " + typeName + " in assembly " + assembly.GetName());

            //SetupWpfApplication(assembly);
            var hostConstructor = type.GetConstructor(new[] { typeof(IWpfHost) });
            if (hostConstructor != null)
            {
                return hostConstructor.Invoke(new object[] { host });
            }
            var defaultConstructor = type.GetConstructor(new Type[0]);
            if (defaultConstructor == null)
            {
                var message = string.Format("Cannot create an instance of {0}. Either a public default constructor, or a public constructor taking IWpfHost must be defined", typeName);
                throw new InvalidOperationException(message);
            }
            return defaultConstructor.Invoke(null);
        }

        private static void SetupWpfApplication(Assembly assembly)
        {
            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Application.ResourceAssembly = assembly;
        }
    }
}
