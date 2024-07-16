using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace G2Cy.Log4Net
{
    /// <summary>
    /// ServiceCollectionExtions扩展
    /// </summary>
    public static class ServiceCollectionExtions
    {
        /// <summary>
        /// 注册log4net日志提供器
        /// </summary>
        /// <param name="builder">日志构造接口</param>
        public static void AddLog4Net(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, Log4NetLoggerProvider>();
        }
    }
}
