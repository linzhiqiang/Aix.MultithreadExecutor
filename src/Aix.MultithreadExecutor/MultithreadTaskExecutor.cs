﻿using Aix.MultithreadExecutor.TaskExecutor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.MultithreadExecutor
{
    /// <summary>
    /// 多线程任务执行器
    /// </summary>
    public abstract class MultithreadTaskExecutor : ITaskExecutor
    {
        static readonly int DefaultTaskExecutorThreadCount = Environment.ProcessorCount * 2;//默认线程数
        static Func<ITaskExecutor> DefaultExecutorFactory = () => new SingleThreadTaskExecutor();
        readonly ITaskExecutor[] EventLoops;
        int requestId;

        private MultithreadExecutorOptions options;
        public int ThreadCount => options.ThreadCount; 

        public MultithreadTaskExecutor(Action<MultithreadExecutorOptions> setupOptions)
        {
            options = new MultithreadExecutorOptions();
            if (setupOptions != null) setupOptions(options);
            if (options.ThreadCount <= 0) throw new ArgumentException("线程数必须大于0", "ThreadCount");
            var threadCount = options.ThreadCount;
            this.EventLoops = new ITaskExecutor[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var eventLoop = DefaultExecutorFactory();
                this.EventLoops[i] = eventLoop;
                eventLoop.OnException += EventLoop_OnException;
            }

        }

        public ITaskExecutor GetNext()
        {
            int id = Interlocked.Increment(ref this.requestId);
            return GetNext(id);
        }

        public ITaskExecutor GetNext(int index)
        {
            return this.EventLoops[Math.Abs(index % this.EventLoops.Length)];
        }

        public ITaskExecutor GetSingleThreadTaskExecutor(int routeId)
        {
            return GetNext(routeId);
        }
        public ITaskExecutor GetSingleThreadTaskExecutor(string routeId)
        {
            if (!string.IsNullOrEmpty(routeId))
                return GetNext(routeId.GetHashCode());

            return GetNext();
        }

        private async Task EventLoop_OnException(Exception ex)
        {
            if (OnException != null) await OnException(ex);
        }

        #region ITaskExecutor

        public event Func<Exception, Task> OnException;

        public void Execute(Func<object, Task> action, object state)
        {
            this.GetNext().Execute(action, state);
        }

        public void Execute(IRunnable task)
        {
            this.GetNext().Execute(task);
        }

        public void Schedule(IRunnable action, TimeSpan delay)
        {
            this.GetNext().Schedule(action, delay);
        }

        public void Schedule(Func<object, Task> action, object state, TimeSpan delay)
        {
            this.GetNext().Schedule(action, state, delay);
        }

        public int GetTaskCount()
        {
            int sum = 0;
            foreach (var item in this.EventLoops)
            {
                sum += item.GetTaskCount();
            }

            return sum;
        }
        public void Start()
        {
            foreach (var item in this.EventLoops)
            {
                item.Start();
            }
        }

        public void Stop()
        {
            foreach (var item in this.EventLoops)
            {
                item.Stop();
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
        #endregion

    }
}
