// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using dotnetCampus.Ipc;
using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.Pipes;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Xml.Linq;

namespace G2Cy.PluginHosting
{
    public class IpcServices
    {
        public static bool Registered;

        public static object Mutex = new object();

        private static IpcProvider ipcProvider;

        //private static IPluginLoader pluginLoader;
        /// <summary>
        /// 注册命名管道
        /// </summary>
        /// <param name="portName">通道名称</param>
        public static void RegisterChannel(string portName)
        {
            lock (Mutex)
            {
                if (Registered) return;

                //var serverProvider = new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full };
                //var clientProvider = new BinaryClientFormatterSinkProvider();
                //var properties = new Hashtable();
                //properties["portName"] = portName;
                //var channel = new IpcChannel(properties, clientProvider, serverProvider);
                //ChannelServices.RegisterChannel(channel, false);
                // 创建`Ipc`提供器实例，并指定管道名称
                ipcProvider = new IpcProvider(portName);
                // 启动服务监听
                ipcProvider.StartServer();
                Debug.WriteLine($"注册管道：{portName}");
                Registered = true;
            }
        }
        /// <summary>
        /// 注册目标对象
        /// </summary>
        public static void RegisterLoaderObject(IPluginLoader pluginLoader)
        {
            //TODO: 发送数据到管道
            //RemotingServices.Marshal(this, "PluginLoader", typeof(IPluginLoader));
            //// 连接指定端点通道
            //string serverpipeName = "PluginLoader";
            //// 连接成功返回端点代理
            //var peer = ipcProvider.GetAndConnectToPeerAsync(serverpipeName).Result;
            //// 调用远程契约接口代理对象
            //pluginLoader = ipcProvider.CreateIpcProxy<IPluginLoader>(peer);
            Debug.WriteLine($"注册类型对象：{pluginLoader.GetType().FullName}");
            // 创建用于对接来自其他端通过 IPC 访问指定类型的对接对象。
            ipcProvider.CreateIpcJoint(pluginLoader);
        }

        public static void RegisterHostObject(IWpfHost host)
        {
            //TODO: 发送数据到管道
            //RemotingServices.Marshal(this, "PluginLoader", typeof(IPluginLoader));
            //// 连接指定端点通道
            //string serverpipeName = "PluginLoader";
            //// 连接成功返回端点代理
            //var peer = ipcProvider.GetAndConnectToPeerAsync(serverpipeName).Result;
            //// 调用远程契约接口代理对象
            //pluginLoader = ipcProvider.CreateIpcProxy<IPluginLoader>(peer);
            Debug.WriteLine($"注册对象：{host.GetType().Name}");
            // 创建用于对接来自其他端通过 IPC 访问指定类型的对接对象。
            ipcProvider.CreateIpcJoint(host);
        }
        public static T GetIpcObject<T>(string name) where T : class
        {
            // 连接成功返回端点代理
            var peer = ipcProvider.GetAndConnectToPeerAsync(name).Result;
            // 调用远程契约接口代理对象
            var ipcContract = ipcProvider.CreateIpcProxy<T>(peer);
            Debug.WriteLine($"{name} 获取对象：{typeof(T).Name}");
            return ipcContract;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public static void Dispose()
        {
            if (ipcProvider != null) {
                Debug.WriteLine($"释放通道资源:{ipcProvider.IpcContext.PipeName}");
                ipcProvider.Dispose();
            }
        }
    }
}
