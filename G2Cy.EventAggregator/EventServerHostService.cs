using CodeWF.EventBus.Socket;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace G2Cy.EventAggregator
{
    public class EventServerHostService : BackgroundService
    {
        private readonly ServerOptions serverOptions;
        private IEventServer server;
        public EventServerHostService(ServerOptions options)
        {
            serverOptions = options;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            server = new EventServer();
            Debug.WriteLine($"事件监听服务:创建server实例");
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            server?.Start(serverOptions.Address, serverOptions.Port);
            //
            Debug.WriteLine($"事件监听服务:{serverOptions.Address}:{serverOptions.Port}");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            server.Stop();
            await Task.CompletedTask;
        }
    }
}
