// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using G2Cy.PluginHosting;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Ioc;

namespace G2Cy.WpfHost
{
    internal class Plugin : IServiceProvider, IDisposable
    {
        private readonly IContainerProvider _childContainer;
        private readonly IContainerRegistry _containerRegistry;
        private readonly ILogger _log;
        private PluginProcessProxy _remoteProcess;
        private PluginStartupInfo _startupInfp;
        private bool _isDisposing;
        private bool _fatalErrorOccurred;

        public PluginCatalogEntry CatalogEntry { get; private set; }
        public FrameworkElement View { get; private set; }
        public string Title { get; private set; }

        public event EventHandler<PluginErrorEventArgs> Error;

        private IWpfHost _host;

        public Plugin(IContainerExtension container, ILogger<Plugin> log)
        {
            _log = log;
            _childContainer = container;
            _containerRegistry = container;
            _host = container.Resolve<IWpfHost>();
        }

        // can be executed on any thread
        public void Load(PluginCatalogEntry catalogEntry)
        {
            if (CatalogEntry != null) throw new InvalidOperationException("Plugin can be loaded only once");

            CatalogEntry = catalogEntry;
            Title = catalogEntry.Name;

            Initialize();

            _log.LogDebug(string.Format("Loading plugin {0} from {1}, {2}", CatalogEntry.Name, CatalogEntry.AssemblyPath, CatalogEntry.MainClass));

            var host = _childContainer.Resolve<PluginViewOfHost>();
            host.FatalError += OnFatalError;

            _remoteProcess = _childContainer.Resolve<PluginProcessProxy>();

            _log.LogDebug("Starting plugin process");
            _remoteProcess.Start();
            new ProcessMonitor(OnProcessExited).Start(_remoteProcess.Process);

            _log.LogDebug("Calling LoadPlugin()");
            _remoteProcess.LoadPlugin();
        }

        // must execute on UI thread
        public void CreateView()
        {
            _log.LogDebug("Creating plugin view");
            //TODO:获取进程视图
            //View = FrameworkElementAdapters.ContractToViewAdapter(_remoteProcess.RemotePlugin.Contract);
            if (_remoteProcess.RemotePlugin == null)
            {
                return;
            }
            IntPtr intPtr = IntPtr.Parse(_remoteProcess.RemotePlugin.Contract.ToString());
            View = new ViewHost(intPtr);
            _log.LogDebug("Plugin view created");
        }

        public object GetService(Type serviceType)
        {
            if (_remoteProcess == null) return null;
            return _remoteProcess.RemotePlugin?.GetService(serviceType);
        }

        public void Dispose()
        {
            _isDisposing = true;

            try
            {
                var disposableView = View as IDisposable;
                if (disposableView != null) disposableView.Dispose();
                // 关闭进程
                _remoteProcess?.Process?.Kill();
            }
            catch (Exception ex)
            {
                ReportError("Error when disposing view", ex);
            }
        }

        private void Initialize()
        {
            //_containerRegistry.RegisterSingleton<IWpfHost, PluginViewOfHost>();
            //_containerRegistry.Register<PluginProcessProxy>();

            _startupInfp = new PluginStartupInfo
            {
                FullAssemblyPath = GetFullPath(CatalogEntry.AssemblyPath),
                AssemblyName = Path.GetFileNameWithoutExtension(CatalogEntry.AssemblyPath),
                Bits = CatalogEntry.Bits,
                MainClass = CatalogEntry.MainClass,
                Name = CatalogEntry.Name,
                Parameters = CatalogEntry.Parameters
            };
            var errorservice = _childContainer.Resolve<ErrorHandlingService>();
            _containerRegistry.Register<PluginProcessProxy>(provider => { return new PluginProcessProxy(_startupInfp, _host, errorservice); });
        }

        private string GetFullPath(string assemblyPath)
        {
            var fileinfo = new FileInfo(assemblyPath);
            if (fileinfo.Exists)
            {
                return fileinfo.FullName;
            }
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyPath);
            if (File.Exists(path))
            {
                return path;
            }
            throw new FileNotFoundException($"file not find {path}");
        }

        private void OnProcessExited()
        {
            if (!_isDisposing && !_fatalErrorOccurred)
            {
                ReportError("Plugin process terminated unexpectedly", null);
            }
        }

        private void OnFatalError(Exception ex)
        {
            _fatalErrorOccurred = true;
            ReportError(null, ex);
        }

        private void ReportError(string message, Exception ex)
        {
            if (Error != null)
            {
                Error(this, new PluginErrorEventArgs(this, message, ex));
            }
        }
    }

    class ViewHost : HwndHost
    {
        private readonly IntPtr _handle;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

        public ViewHost(IntPtr handle) => _handle = handle;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            SetParent(new HandleRef(null, _handle), hwndParent);
            return new HandleRef(this, _handle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }
    }
}
