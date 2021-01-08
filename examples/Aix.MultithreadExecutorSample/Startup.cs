using Aix.MultithreadExecutorSample.HostServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aix.MultithreadExecutor;
using Microsoft.Extensions.Logging;

namespace Aix.MultithreadExecutorSample
{
    public class Startup
    {
        internal static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

            #region 任务执行器相关
            //自己实现抽象类MultithreadTaskExecutor，如下方式注册      这样设计的目的，不同场景使用时进行隔离
            services.AddSingleton(serviceProvider=> {
                var logger = serviceProvider.GetService<ILogger<MyMultithreadTaskExecutor>>();
                var taskExecutor=  new MyMultithreadTaskExecutor(options=> {
                    options.ThreadCount = Environment.ProcessorCount * 2;
                });
                taskExecutor.OnException += ex =>
                  {
                      logger.LogError(ex, "本地多线程任务执行器执行出错");
                      return Task.CompletedTask;
                  };
                taskExecutor.Start();
                logger.LogInformation($"本地多线程任务执行器开始 ThreadCount={taskExecutor.ThreadCount}......");
                return taskExecutor;
            });

            #endregion

            //入口服务
            services.AddHostedService<StartHostService>();
        }
    }
}
