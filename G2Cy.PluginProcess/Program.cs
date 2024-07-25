// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Configuration;
using System.IO;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Threading;
using G2Cy.PluginHosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using G2Cy.Log4Net;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Reflection;

namespace G2Cy.PluginProcess
{
    class Program
    {
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            // 引入Log4Net
            builder.Logging.AddLog4Net();

            IServiceCollection services = builder.Services;

            services.AddSingleton<AssemblyResolver>();
            services.AddSingleton<PluginLoader>(provider => {
                ILogger<PluginLoader> logger = provider.GetService<ILogger<PluginLoader>>();
                AssemblyResolver assemblyResolver = provider.GetService<AssemblyResolver>();
                return new PluginLoader(logger, Dispatcher.CurrentDispatcher,assemblyResolver);
            });

            services.AddSingleton<PluginLoaderBootstrapper>();

            if (args.Length != 4)
            {
                Console.Error.WriteLine("Usage: PluginProcess name assemblyPath");
                return;
            }

            try
            {
                var name = args[0];
                int bits = IntPtr.Size * 8;
                Console.WriteLine("Starting PluginProcess {0}, {1} bit", name, bits);

                var assemblyPath = args[1];
                Console.WriteLine("Plugin assembly: {0}", assemblyPath);
                var hostDir = args[2];// 获取主进程的工作路径
                Console.WriteLine("Host WorkDir: {0}", hostDir);
                CheckFileExists(assemblyPath);

                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                var typeName = args[3]; // 主类类型
                // TODO: 进程执行时，需要在构建服务提供器之前，注册插件程序集中的主类类型
                //var assembly = Assembly.LoadFile(assemblyPath);
                // 指定程序集中的主类类型
                var type = assembly.GetType(typeName);
                if (type == null) throw new InvalidOperationException("Could not find type " + typeName + " in assembly " + Path.GetFileName(assemblyPath));
                services.AddSingleton(type);
                //var configFile = GetConfigFile(assemblyPath);
                //var appBase = Path.GetDirectoryName(assemblyPath);
                //var appDomain = CreateAppDomain(appBase, configFile);
                //var bootstrapper = CreateInstanceFrom<PluginLoaderBootstrapper>(appDomain);
                IHost host = builder.Build();
                var bootstrapper = host.Services.GetService<PluginLoaderBootstrapper>();
                bootstrapper.Run(name, hostDir);
                IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
                PluginProcessOptions processOptions = new PluginProcessOptions();
                configuration.GetSection("PluginLoader").Bind(processOptions);
                bool breakIntoDebugger = processOptions.BreakIntoDebugger;
                if (breakIntoDebugger) System.Diagnostics.Debugger.Break();
                bool pauseOnError = processOptions.PauseOnError;
                host.Run();
                if (pauseOnError) Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            Console.WriteLine($"PluginProcess:{args.LoadedAssembly.FullName}");
        }

        private static T CreateInstanceFrom<T>(AppDomain appDomain)
        {
            return (T)appDomain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.Location, typeof(T).FullName);
        }

        private static void CheckFileExists(string path)
        {
            var fileinfo = new FileInfo(path);
            if (!fileinfo.Exists)
            {
                throw new InvalidOperationException("File '" + fileinfo.FullName + "' does not exist");
            }
        }

        private static string GetConfigFile(string assemblyPath)
        {
            //var name = assemblyPath + ".config";
            var name = assemblyPath + ".json";
            return File.Exists(name) ? name : null;
        }

        private static AppDomain CreateAppDomain(string appBase, string config)
        {
            return AppDomain.CurrentDomain;
        }
    }
}
