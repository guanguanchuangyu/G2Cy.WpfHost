// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.Logging;

namespace G2Cy.PluginHosting
{
    public class AssemblyResolver
    {
        private string _thisAssemblyName;
        private string _interfacesAssemblyName;
        private string _hostDir;
        private ILogger<AssemblyResolver> _logger;
        public AssemblyResolver(ILogger<AssemblyResolver> logger)
        {
            _logger = logger;
        }

        public void Setup(string hostdir)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                _thisAssemblyName = GetType().Assembly.GetName().Name;
                _interfacesAssemblyName = typeof(IWpfHost).Assembly.GetName().Name;
                //TODO:需要优化逻辑细节
                _hostDir = hostdir;
                _logger.LogInformation($"主进程执行路径为 {_hostDir}");
                //_logger.LogInformation($"_thisAssemblyName:{_thisAssemblyName}|_interfacesAssemblyName:{_interfacesAssemblyName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setup AssemblyResolver Error");
                throw ex;
            }
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            //Console.WriteLine($"AssemblyResolve/assemblyName:{assemblyName}");
            var assemblydllpath = args.RequestingAssembly?.Location;
            //if (!string.IsNullOrEmpty(assemblydllpath))
            //{
            //    Console.WriteLine($"AssemblyResolve/assemblydllpath:{assemblydllpath}");
            //}
            if (assemblyName.Name == _thisAssemblyName) return GetType().Assembly;
            if (assemblyName.Name == _interfacesAssemblyName) return typeof(IWpfHost).Assembly;
            if (!string.IsNullOrEmpty(_hostDir))
            {
                // 加载目标依赖的程序集
                var assemblyPath = Path.Combine(_hostDir, assemblyName.Name + ".dll");
                _logger.LogInformation($"AssemblyResolve:{assemblyPath}");
                if (File.Exists(assemblyPath))
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                }
            }
            return null;
        }

    }
}
