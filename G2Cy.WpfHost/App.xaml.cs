// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using CodeWF.EventBus.Socket;
using Example;
using G2Cy.EventAggregator;
using G2Cy.WpfHost.Interfaces;
using G2Cy.WpfHost.ViewModels;
using G2Cy.WpfHost.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;

namespace G2Cy.WpfHost
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public App()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        private void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            //if (args.LoadedAssembly.FullName.Contains("resources"))
            //{
            //    Debugger.Break();
            //}
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var culture = Thread.CurrentThread.CurrentUICulture;
            Debug.WriteLine($"当前UI线程语言：{culture.Name}");
        }
        protected override Window CreateShell()
        {
            // 启动总线服务
            var eventServer = Container.Resolve<EventServerHostService>();
            eventServer.StartAsync(CancellationToken.None);
            var eventClient = Container.Resolve<EventClientHostService>();
            eventClient.StartAsync(CancellationToken.None);
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
            {
                Debug.WriteLine(assembly.FullName);
            });
            // 注册服务
            containerRegistry.RegisterSingleton<MainWindow>();
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            containerRegistry.RegisterSingleton<PluginController>();
            containerRegistry.RegisterSingleton<PluginViewOfHost>();
            var wpfhost = Container.Resolve<PluginViewOfHost>();
            containerRegistry.RegisterSingleton<IWpfHost>(() => { return wpfhost; });
            containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));
            containerRegistry.Register(typeof(ILoggerFactory), typeof(LoggerFactory));
            containerRegistry.RegisterSingleton<ErrorHandlingService>();
            // 注册服务
            var host = new EventServerHostService(new ServerOptions { Address = "127.0.0.1", Port = 8080 });
            var aggregator = new EventSocketAggregator(new ClientOptions { ServerAddress = "127.0.0.1", ServerPort = 8080 });
            // 注册事件队列
            containerRegistry.RegisterInstance(host);
            containerRegistry.RegisterInstance<IEventAggregator>(aggregator);
            var client = new EventClientHostService(aggregator);
            containerRegistry.RegisterInstance(client);
        }

        //protected override IModuleCatalog CreateModuleCatalog()
        //{
        //    // 实例化模块集合
        //    string moduledir = @"./Modules";
        //    var modules = new List<IModuleCatalogItem>();
        //    if (!Directory.Exists(moduledir))
        //    {
        //        Directory.CreateDirectory(moduledir);
        //    }
        //    // 获取当前目录下的模块
        //    foreach (var dir in Directory.GetDirectories(moduledir))
        //    {
        //        // 获取模块对应实现类
        //        var dirCategorylog = new MetaModuleCatalog
        //        {
        //            ModulePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dir)
        //        };
        //        // 初始化模块
        //        dirCategorylog.Initialize();
        //        // 添加到模型集合
        //        modules.AddRange(dirCategorylog.Items);
        //    }
        //    // 创建模块类型实例
        //    var catalog = new ModuleCatalog();
        //    foreach (var module in modules)
        //    {
        //        catalog.Items.Add(module);
        //    }

        //    return catalog;
        //}
    }
}
