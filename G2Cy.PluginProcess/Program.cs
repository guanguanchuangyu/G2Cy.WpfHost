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
            // TODO: 进程执行时，需要在构建服务提供器之前，注册插件程序集中的主类类型

            services.AddSingleton<PluginLoader>(provider => {
                ILogger<PluginLoader> logger = provider.GetService<ILogger<PluginLoader>>();
                AssemblyResolver assemblyResolver = provider.GetService<AssemblyResolver>();
                return new PluginLoader(logger, Dispatcher.CurrentDispatcher,assemblyResolver);
            });

            services.AddSingleton<PluginLoaderBootstrapper>();
            IHost host = builder.Build();
            IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
            PluginProcessOptions processOptions = new PluginProcessOptions();
            configuration.GetSection("PluginLoader").Bind(processOptions);
            bool breakIntoDebugger = processOptions.BreakIntoDebugger;
            if (breakIntoDebugger) System.Diagnostics.Debugger.Break();

            bool pauseOnError = processOptions.PauseOnError;

            if (args.Length != 3)
            {
                Console.Error.WriteLine("Usage: PluginProcess name assemblyPath");
                if (pauseOnError) Console.ReadLine();
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
                //var configFile = GetConfigFile(assemblyPath);
                //var appBase = Path.GetDirectoryName(assemblyPath);
                //var appDomain = CreateAppDomain(appBase, configFile);
                //var bootstrapper = CreateInstanceFrom<PluginLoaderBootstrapper>(appDomain);

                var bootstrapper = services.BuildServiceProvider().GetService<PluginLoaderBootstrapper>();
                bootstrapper.Run(name, hostDir);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                if (pauseOnError)
                {
                    Console.Error.WriteLine("Pausing on error, press any key to exit...");
                    Console.ReadLine();
                }
            }

            host.Run();
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
