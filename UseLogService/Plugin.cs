// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Windows;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UseLogService.ViewModels;

namespace UseLogService
{
    class Plugin : PluginBase, IUnsavedData
    {
        private readonly ILogger _log;
        private string _parameter;
        private MainUserControl _control;
        private IServiceProvider _serviceProvider;
        public Plugin()
        {

        }

        // 服务注册
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<MainUserControl>();
            services.AddSingleton<MainUserContentViewModel>();
            _serviceProvider = services.BuildServiceProvider();
        }
        // 服务提供
        public override object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
        public override FrameworkElement CreateControl()
        {
            //_control = new MainUserControl { Log = _log };
            _control = (MainUserControl)GetService(typeof(MainUserControl));
            _control.Log = (ILogger<MainUserControl>)GetService(typeof(ILogger<MainUserControl>));
            var startupInfo = (PluginStartupInfo)GetService(typeof(PluginStartupInfo));
            if (startupInfo != null) _parameter = startupInfo.Parameters;
            _control.Message.Text = _parameter;
            return _control;
        }

        public string[] GetNamesOfUnsavedItems()
        {
            if (_control == null) return null;
            return _control.GetNamesOfUnsavedItems();
        }
    }
}
