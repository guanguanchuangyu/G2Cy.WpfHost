// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using dotnetCampus.Ipc.Pipes;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace G2Cy.PluginHosting
{
    /// <summary>
    /// Loads plugins for the host
    /// </summary>
    public class PluginLoader : MarshalByRefObject, IPluginLoader
    {
        private Dispatcher _dispatcher;
        private AssemblyResolver _assemblyResolver;
        private ILogger _log;
        private IWpfHost _host;
        private string _name;
        private IServiceCollection _services;
        public PluginLoader(ILogger<PluginLoader> logger,Dispatcher dispatcher, AssemblyResolver assemblyResolver, IServiceCollection serviceDescriptors)
        {
            _log = logger;
            _dispatcher = dispatcher;
            _assemblyResolver = assemblyResolver;
            _services = serviceDescriptors;
        }

        public void Run(string name,string hostdir)
        {
            _name = name;
            //_dispatcher = Dispatcher.CurrentDispatcher;

            try
            {
                _log.LogInformation("PluginHost running at " + IntPtr.Size * 8 + " bit, CLR version " + Environment.Version);
                // 获取主进程所在的目录
                _assemblyResolver.Setup(hostdir);
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                // 注册管道
                IpcServices.RegisterChannel(name);
                _log.LogDebug(string.Format("Listening on ipc://{0}/PluginLoader", name));
                // 注册插件加载对象
                IpcServices.RegisterLoaderObject(this);
                // 准备标记
                SignalReady();
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                ReportFatalError(ex);
            }

            Thread.Sleep(100); // allow any pending remoting operations to finish
            _log.LogDebug("Shutdown complete");
        }

        public IRemotePlugin LoadPlugin(PluginStartupInfo startupInfo)
        {
            string hostname = startupInfo.HostChannelName;
            _services.AddSingleton(startupInfo);
            _host = IpcServices.GetIpcObject<IWpfHost>(hostname);
            _log.LogInformation(string.Format("LoadPlugin('{0}','{1}')", startupInfo.AssemblyName, startupInfo.MainClass));

            new ProcessMonitor(Dispose).Start(_host.HostProcessId);
            //_log.LogDebug($"ThreadId:{_dispatcher.Thread.ManagedThreadId.ToString()}");
            Func<PluginStartupInfo, object> createOnUiThread = LoadPluginOnUiThread;
            // 在UI线程上创建插件
            var result = _dispatcher.Invoke(createOnUiThread, startupInfo);

            _log.LogDebug("Returning plugin object to host");

            if (result is Exception)
            {
                _log.LogError("Error loading plugin:{0}", (Exception)result);
                throw new TargetInvocationException((Exception)result);
            }
            return (IRemotePlugin)result;
        }

        public void Dispose()
        {
            _log.LogInformation("Shutdown requested");

            IpcServices.Dispose();

            if (_dispatcher != null)
            {
                _log.LogDebug("Performing dispatcher shutdown");
                _dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            }
            else
            {
                _log.LogDebug("No dispatcher, exiting the process");
                Environment.Exit(1);
            }
        }

        public override object InitializeLifetimeService()
        {
            return null; // live forever
        }

        private object LoadPluginOnUiThread(PluginStartupInfo startupInfo)
        {
            _log.LogDebug("Creating plugin on UI thread");

            var assembly = startupInfo.AssemblyName;
            var mainClass = startupInfo.MainClass;

            try
            {
                var obj = PluginCreator.CreatePlugin(startupInfo,_host);
                _log.LogDebug("Created local plugin class instance");

                var localPlugin = obj as IPlugin;

                if (localPlugin == null)
                {
                    var message = string.Format("Object of type {0} cannot be loaded as plugin " +
                        "because it does not implement IPlugin interface", mainClass);

                    throw new InvalidOperationException(message);
                }
                var remotePlugin = new RemotePlugin(localPlugin,_services);
                Console.WriteLine("Created plugin control");
                return remotePlugin;
            }
            catch (Exception ex)
            {
                var message = string.Format("Error loading type '{0}' from assembly '{1}'. {2}",
                    mainClass, assembly, ex.Message);

                return new ApplicationException(message, ex);
            }
        }

        private void RegisterObject()
        {
            

        }

        private void SignalReady()
        {
            var eventName = _name + ".Ready";
            var readyEvent = EventWaitHandle.OpenExisting(eventName);
            readyEvent.Set();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception =
                e.ExceptionObject as Exception ??
                new Exception("Unknown error. Exception object is null");

            ReportFatalError(exception);
        }

        private void ReportFatalError(Exception exception)
        {
            _log.LogError("Unhandled exception", exception);

            if (_host != null)
            {
                _log.LogDebug("Reporting fatal error to host");
                _host.ReportFatalError(ExceptionUtil.GetUserMessage(exception), exception.ToString());
            }
            else
            {
                _log.LogWarning("Host is null, cannot report error to host");
            }

            _log.LogInformation("Exiting the process to prevent 'program stopped working' dialog");
            //_log.Dispose(); // flush pending data
            Environment.Exit(2);
        }
    }
}
