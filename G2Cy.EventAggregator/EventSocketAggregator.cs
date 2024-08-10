using CodeWF.EventBus.Socket;
using G2Cy.WpfHost.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace G2Cy.EventAggregator
{
    public class EventSocketAggregator : IEventAggregator
    {
        private readonly ClientOptions clientOptions;
        private IEventClient eventClient;
        public EventSocketAggregator(ClientOptions options)
        {
            clientOptions = options;
        }

        public void Build()
        {
            eventClient = new EventClient();
        }

        public void Activate()
        {
           eventClient.Connect(clientOptions.ServerAddress, clientOptions.ServerPort);
        }

        public void Subscribe<T>(string subject, Action<T> eventHandler)
        {
           eventClient.Subscribe<T>(subject, eventHandler);
        }

        public void Subscribe<T>(string subject, Func<T, Task> asyncEventHandler)
        {
            eventClient.Subscribe<T>(subject, asyncEventHandler);
        }

        public void Unsubscribe<T>(string subject, Action<T> eventHandler)
        {
            eventClient.Unsubscribe<T>(subject, eventHandler);
        }

        public void Unsubscribe<T>(string subject, Func<T, Task> asyncEventHandler)
        {
            eventClient.Unsubscribe<T>(subject, asyncEventHandler);
        }

        public bool Publish<T>(string subject, T message, out string errorMessage)
        {
            return eventClient.Publish<T>(subject, message, out errorMessage);
        }

        public void Dispose()
        {
            if (eventClient.ConnectStatus == ConnectStatus.Connected)
            {
                eventClient.Disconnect();
            }
        }
    }
}
