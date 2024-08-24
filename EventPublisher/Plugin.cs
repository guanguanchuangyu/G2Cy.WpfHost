using EventPublisher.ViewModels;
using G2Cy.WpfHost.Interfaces;
using G2Cy.EventAggregator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventPublisher
{
    public class Plugin : PluginBase
    {
        public override object CreateControl()
        {
            try
            {
                MainControl mainControl = (MainControl)GetService(typeof(MainControl));
                return mainControl;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IServiceProvider _provider;
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IEventAggregator, EventSocketAggregator>(provider => {
                ILogger<EventSocketAggregator> logger = provider.GetService<ILogger<EventSocketAggregator>>();
                return new EventSocketAggregator(logger,new ClientOptions { ServerAddress = "127.0.0.1", ServerPort = 8080 }); 
            });
            services.AddHostedService<EventClientHostService>();
            services.AddSingleton<MainControl>();
            services.AddSingleton<MainControlViewModel>();
            _provider = services.BuildServiceProvider();
        }

        public override object GetService(Type serviceType)
        {
            return _provider.GetService(serviceType);
        }

        public override void InitPlugin()
        {
            //
            var host = (IHostedService)GetService(typeof(IHostedService));
            host.StartAsync(CancellationToken.None).Wait();
        }
    }
}
