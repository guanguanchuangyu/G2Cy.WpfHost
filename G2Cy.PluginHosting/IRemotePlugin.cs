// Copyright (c) 2013 Ivan Krivyakov
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
using dotnetCampus.Ipc.CompilerServices.Attributes;
using System;
//using System.AddIn.Contract;

namespace G2Cy.PluginHosting
{
    [IpcPublic]
    public interface IRemotePlugin : IServiceProvider, IDisposable
    {
        //INativeHandleContract Contract { get; }
        Int32 Contract { get; set; }
    }
}
