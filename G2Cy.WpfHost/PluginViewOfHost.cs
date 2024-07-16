// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Diagnostics;
using G2Cy.PluginHosting;
using G2Cy.WpfHost.Interfaces;
using Prism.Ioc;

namespace G2Cy.WpfHost
{
    internal class PluginViewOfHost : MarshalByRefObject, IWpfHost
    {
        private readonly IContainerProvider _container;

        public PluginViewOfHost(IContainerExtension container)
        {
            _container = container;
            var hostChannelName = "WpfHost." + HostProcessId;
            IpcServices.RegisterChannel(hostChannelName);
            // 注入Host对象
            IpcServices.RegisterHostObject(this);
        }

        public void ReportFatalError(string userMessage, string fullExceptionText)
        {
            LastError = new PluginException(userMessage, fullExceptionText);
            if (FatalError != null) FatalError(LastError);
        }

        public int HostProcessId { get { return Process.GetCurrentProcess().Id; } }

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        public event Action<Exception> FatalError;

        public Exception LastError { get; private set; }

        public override object InitializeLifetimeService()
        {
            return null; // live forever
        }
    }
}
