// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using G2Cy.PluginHosting;
using G2Cy.WpfHost.Interfaces;

namespace G2Cy.WpfHost
{
    internal class PluginProcessProxy : IDisposable
    {
        private readonly IWpfHost _host;
        private readonly PluginStartupInfo _startupInfo;
        private readonly ErrorHandlingService _errorHandlingService;
        private EventWaitHandle _readyEvent;
        private Process _process;
        private string _name;
        private IPluginLoader _pluginLoader;

        public Process Process { get { return _process; } }
        public IRemotePlugin RemotePlugin { get; private set; }

        public PluginProcessProxy(PluginStartupInfo startupInfo, IWpfHost host, ErrorHandlingService errorHandlingService)
        {
            _startupInfo = startupInfo;
            _host = host;
            _errorHandlingService = errorHandlingService;
        }

        public void Start()
        {
            if (Process != null) throw new InvalidOperationException("Plugin process already started, cannot load more than one plugin per process");
            StartPluginProcess(_startupInfo.FullAssemblyPath);
        }

        public void LoadPlugin()
        {
            if (Process == null) throw new InvalidOperationException("Plugin process not started");
            if (Process.HasExited) throw new InvalidOperationException("Plugin process has terminated unexpectedly");

            _pluginLoader = GetPluginLoader();
            RemotePlugin = _pluginLoader.LoadPlugin(_startupInfo);
        }

        public void Dispose()
        {
            if (RemotePlugin != null)
            {
                try
                {
                    RemotePlugin.Dispose();
                }
                catch (Exception ex)
                {
                    _errorHandlingService.LogError("Error disposing remote plugin for " + _startupInfo.Name, ex);
                }
            }

            if (_pluginLoader != null)
            {
                try
                {
                    _pluginLoader.Dispose();
                }
                catch (Exception ex)
                {
                    _errorHandlingService.LogError("Error disposing plugin loader for " + _startupInfo.Name, ex);
                }
            }

            // this can take some time if we have many plugins; should be made asynchronous
            if (Process != null)
            {
                Process.WaitForExit(5000);
                if (!Process.HasExited)
                {
                    _errorHandlingService.LogError("Remote process for " + _startupInfo.Name + " did not exit within timeout period and will be terminated", null);
                    Process.Kill();
                }
            }
        }

        private void StartPluginProcess(string assemblyPath)
        {
            _name = "PluginProcess." + Guid.NewGuid();
            var eventName = _name + ".Ready";
            // 设置跨进程等待处理程序
            _readyEvent = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);

            var directory = Path.GetDirectoryName(GetType().Assembly.Location);
            var bitstr = _startupInfo.Bits == 64 ? "x64" : "x86";
            var exeFile = _startupInfo.Bits == 64 ? "G2Cy.PluginProcess64.exe" : "G2Cy.PluginProcess.exe";
            var processName = Path.Combine(directory,bitstr, exeFile);

            if (!File.Exists(processName)) throw new InvalidOperationException("Could not find file '" + processName + "'");

            const string quote = "\"";
            const string doubleQuote = "\"\"";

            var quotedAssemblyPath = quote + assemblyPath.Replace(quote, doubleQuote) + quote;
            //TODO:加载配置控制进程是否控制台显示
            //var createNoWindow = !bool.Parse(ConfigurationManager.AppSettings["PluginProcess.ShowConsole"]);
            var createNoWindow = true;

            var info = new ProcessStartInfo
            {
                Arguments = _name + " " + quotedAssemblyPath +" " + AppDomain.CurrentDomain.BaseDirectory + " " + _startupInfo.MainClass,// 命令行运行时传入主进程工作路径
                CreateNoWindow = createNoWindow,
                UseShellExecute = false,
                FileName = processName
            };

            Trace.WriteLine(info.Arguments);

            _process = Process.Start(info);
        }

        private IPluginLoader GetPluginLoader()
        {
            if (Process.HasExited)
            {
                throw new InvalidOperationException("Plugin process has terminated unexpectedly");
            }
            //TODO:加载配置控制进程准备超时时间
            //var timeoutMs = int.Parse(ConfigurationManager.AppSettings["PluginProcess.ReadyTimeoutMs"]);
            var timeoutMs = 10000;

            if (!_readyEvent.WaitOne(timeoutMs))
            {
                // 退出ProcessLoader进程
                if (_process != null && !_process.HasExited)
                {
                    Process p = Process.GetProcessById(_process.Id);
                    p.Kill();
                }
                throw new InvalidOperationException("Plugin process did not respond within timeout period");
            }
            _startupInfo.HostChannelName = $"WpfHost.{_host.HostProcessId}";
            //var url = "ipc://" + _name + "/PluginLoader";
            //var pluginLoader = (IPluginLoader)Activator.GetObject(typeof(IPluginLoader), url);
            var pluginLoader = IpcServices.GetIpcObject<IPluginLoader>(_name);
            return pluginLoader;
        }
    }
}
