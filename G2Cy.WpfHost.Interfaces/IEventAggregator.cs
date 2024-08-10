using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace G2Cy.WpfHost.Interfaces
{
    /// <summary>
    /// 事件聚合器
    /// </summary>
    public interface IEventAggregator: IDisposable
    {
        void Build();

        void Activate();
        void Subscribe<T>(string subject, Action<T> eventHandler);
        void Subscribe<T>(string subject, Func<T, Task> asyncEventHandler);

        void Unsubscribe<T>(string subject, Action<T> eventHandler);
        void Unsubscribe<T>(string subject, Func<T, Task> asyncEventHandler);

        bool Publish<T>(string subject, T message, out string errorMessage);
    }
}
