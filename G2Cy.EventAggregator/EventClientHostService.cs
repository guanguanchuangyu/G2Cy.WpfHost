using CodeWF.EventBus.Socket;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace G2Cy.EventAggregator
{
    /// <summary>
    ///  background service that hosts the event client
    /// </summary>
    public class EventClientHostService : BackgroundService
    {
        
        private IEventAggregator aggregator;
        public EventClientHostService(IEventAggregator eventAggregator)
        {
            aggregator = eventAggregator;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            aggregator.Build();
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            aggregator.Activate();
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            aggregator.Dispose();
            await Task.CompletedTask;
        }
    }
}
