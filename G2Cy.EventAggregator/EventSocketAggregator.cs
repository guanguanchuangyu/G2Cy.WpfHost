using CodeWF.EventBus.Socket;
using G2Cy.WpfHost.Interfaces;
using Microsoft.Extensions.Logging;
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
        private ILogger<EventSocketAggregator> _logger;
        public EventSocketAggregator(ILogger<EventSocketAggregator> logger,ClientOptions options)
        {
            _logger = logger;
            clientOptions = options;
        }

        public void Build()
        {
            _logger.LogInformation("事件客户端实例开始构建");
            eventClient = new EventClient();
            _logger.LogInformation("事件客户端实例构建完成");
        }

        public void Activate()
        {
           _logger.LogInformation("事件客户端开始连接");
           eventClient.Connect(clientOptions.ServerAddress, clientOptions.ServerPort);
            _logger.LogInformation("事件客户端连接完成");
        }

        public void Subscribe<T>(string subject, Action<T> eventHandler)
        {
           eventClient.Subscribe<T>(subject, eventHandler);
           _logger.LogInformation(subject + "订阅成功");
        }

        public void Subscribe<T>(string subject, Func<T, Task> asyncEventHandler)
        {
            eventClient.Subscribe<T>(subject, asyncEventHandler);
            _logger.LogInformation(subject + "订阅成功");
        }

        public void Unsubscribe<T>(string subject, Action<T> eventHandler)
        {
            eventClient.Unsubscribe<T>(subject, eventHandler);
            _logger.LogInformation(subject + "订阅取消");
        }

        public void Unsubscribe<T>(string subject, Func<T, Task> asyncEventHandler)
        {
            eventClient.Unsubscribe<T>(subject, asyncEventHandler);
            _logger.LogInformation(subject + "订阅取消");
        }

        public bool Publish<T>(string subject, T message, out string errorMessage)
        {
            _logger.LogInformation(subject + "发布消息");
            return eventClient.Publish<T>(subject, message, out errorMessage);
        }

        public void Dispose()
        {
            if (eventClient.ConnectStatus == ConnectStatus.Connected)
            {
                eventClient.Disconnect();
                _logger.LogInformation($"事件客户端：{eventClient} 关闭");
            }
        }
    }
}
