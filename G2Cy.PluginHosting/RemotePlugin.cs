// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Windows.Interop;
using System.Windows.Media;

using System.Windows;

//using System.AddIn.Contract;
//using System.AddIn.Pipeline;
using G2Cy.WpfHost.Interfaces;

namespace G2Cy.PluginHosting
{
    internal class RemotePlugin : MarshalByRefObject, IRemotePlugin
    {
        private readonly IPlugin _plugin;

        public RemotePlugin(IPlugin plugin)
        {
            _plugin = plugin;
            var control = plugin.CreateControl();
            var localContract = ViewToHwnd(control);
            //Contract = new NativeHandleContractInsulator(localContract);
            Contract = localContract.ToInt32();
        }

        //public INativeHandleContract Contract { get; private set; }
        public Int32 Contract { get; set; }

        public object GetService(Type serviceType)
        {
            return _plugin.GetService(serviceType);
        }

        public void Dispose()
        {
            _plugin.Dispose();
        }

        public override object InitializeLifetimeService()
        {
            return null; // live forever
        }
        /// <summary>
        /// WPF元素转句柄
        /// </summary>
        /// <param name="element">元素节点</param>
        /// <returns></returns>
        private static IntPtr ViewToHwnd(FrameworkElement element)
        {
            var p = new HwndSourceParameters()
            {
                ParentWindow = new IntPtr(-3), // message only
                WindowStyle = 1073741824
            };
            var hwndSource = new HwndSource(p)
            {
                RootVisual = element,
                SizeToContent = SizeToContent.Manual,
            };
            hwndSource.CompositionTarget.BackgroundColor = Colors.White;
            return hwndSource.Handle;
        }
    }
}
