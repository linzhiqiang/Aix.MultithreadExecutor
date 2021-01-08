using Aix.MultithreadExecutor;
using Aix.MultithreadExecutor.TaskExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aix.MultithreadExecutorSample
{
    public class MyMultithreadTaskExecutor : MultithreadTaskExecutor
    {
        public MyMultithreadTaskExecutor(Action<MultithreadExecutorOptions> setupOptions) : base(setupOptions)
        {
        }
    }
}
