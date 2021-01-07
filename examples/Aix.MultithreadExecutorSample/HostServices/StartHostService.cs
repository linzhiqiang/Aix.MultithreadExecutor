using Aix.MultithreadExecutor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.MultithreadExecutorSample.HostServices
{
    public class StartHostService : IHostedService
    {
        private readonly ILogger<StartHostService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostEnvironment _hostEnvironment;

        private readonly ITaskExecutor _taskExecutor;

        public StartHostService(ILogger<StartHostService> logger, IServiceProvider serviceProvider, IHostEnvironment hostEnvironment
           , ITaskExecutor taskExecutor)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hostEnvironment = hostEnvironment;

            #region 任务执行器相关

            _taskExecutor = taskExecutor;

            #endregion

        }
        public Task StartAsync(CancellationToken cancellationToken)
        {


            Test();
            return Task.CompletedTask;
        }

        private Task Test()
        {
            for (int i = 0; i < 20000; i++)
            {
                //Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}生产即时任务{i}");
                _taskExecutor.Execute(async (state) =>
                {
                    var index = (int)state;
                    _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}即时任务{index}");
                    //await Task.Delay(100);
                    await Task.CompletedTask;
                }, i);
                //Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}生产延迟任务{i}");
                //_taskExecutor.Schedule(async (state) =>
                //{
                //    var index = (int)state;
                //    _logger.LogInformation($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}延迟任务{index}");
                //    //await Task.Delay(100);
                //    await Task.CompletedTask;
                //}, i, TimeSpan.FromSeconds(7));
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


    }
}
