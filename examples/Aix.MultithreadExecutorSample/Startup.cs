using Aix.MultithreadExecutorSample.HostServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aix.MultithreadExecutor;

namespace Aix.MultithreadExecutorSample
{
    public class Startup
    {
        internal static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

            #region 任务执行器相关

            services.AddMultithreadExecutor(8);

            #endregion

            //入口服务
            services.AddHostedService<StartHostService>();
        }
    }
}
