
using Aix.MultithreadExecutor.Foundation;
using Aix.MultithreadExecutor.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.MultithreadExecutor.TaskExecutor
{
    /// <summary>
    /// 单线程任务执行器
    /// </summary>
    internal class SingleThreadTaskExecutor : ITaskExecutor
    {
        public static int MaxTaskCount = int.MaxValue;
        IBlockingQueue<IRunnable> _taskQueue = QueueFactory.Instance.CreateBlockingQueue<IRunnable>();
        protected readonly PriorityQueue<IScheduledRunnable> ScheduledTaskQueue = new PriorityQueue<IScheduledRunnable>();
        volatile bool _isStart = false;
        CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        volatile bool _isStartDelay = false;//是否开启延迟任务线程，有插入延迟任务时就 自动开启，默认不开启

        public SingleThreadTaskExecutor()
        {

        }

        private void StartRunTask()
        {
            Task.Factory.StartNew(async () =>
            {
                while (_isStart && !CancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        //var action = _taskQueue.Dequeue(CancellationTokenSource.Token);
                        foreach (var action in _taskQueue.GetConsumingEnumerable(CancellationTokenSource.Token))
                        {
                            await action.Run(action.state);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        await handlerException(ex);
                    }
                }
            }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void StartRunDelayTask()
        {
            if (_isStartDelay) return;
            lock (this)
            {
                if (_isStartDelay) return;
                _isStartDelay = true;
            }
            Task.Factory.StartNew(async () =>
            {
                while (_isStart && !CancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        RunDelayTask();
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        await handlerException(ex);
                    }
                }
            }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void RunDelayTask()
        {
            lock (ScheduledTaskQueue)
            {
                IScheduledRunnable nextScheduledTask = this.ScheduledTaskQueue.Peek();
                if (nextScheduledTask != null)
                {
                    var tempDelay = nextScheduledTask.TimeStamp - DateUtils.GetTimeStamp();
                    if (tempDelay > 0)
                    {
                        Monitor.Wait(ScheduledTaskQueue, (int)tempDelay);
                    }
                    else
                    {
                        this.ScheduledTaskQueue.Dequeue();
                        Execute(nextScheduledTask);
                    }
                }
                else
                {
                    Monitor.Wait(ScheduledTaskQueue);
                }
            }

        }
        private async Task handlerException(Exception ex)
        {
            if (OnException != null)
            {
                try
                {
                    await OnException(ex);
                }
                catch
                {
                }
            }
        }

        #region ITaskExecutor

        public event Func<Exception, Task> OnException;

        public ITaskExecutor GetSingleThreadTaskExecutor(int routeId)
        {
            return this;
        }

        public ITaskExecutor GetSingleThreadTaskExecutor(string routeKey)
        {
            return this;
        }
        public void Execute(Func<object, Task> action, object state)
        {
            Execute(new TaskRunnable(action, state));
        }

        public void Execute(IRunnable task)
        {
            if (this._taskQueue.Count > MaxTaskCount) throw new Exception($"即时任务队列超过{MaxTaskCount}条");
            var isAddSuccess = _taskQueue.Enqueue(task);
            if (!isAddSuccess) //这种情况基本不会发生，就在程序关闭那一刻可能会有，做个兼容
            {
                task.Run(task.state);
            }
        }

        public void Schedule(IRunnable action, TimeSpan delay)
        {
            if (delay <= TimeSpan.Zero)
            {
                Execute(action);
                return;
            }
            Schedule(new ScheduledRunnable(action, DateUtils.GetTimeStamp(DateTime.Now.Add(delay))));
        }

        public void Schedule(Func<object, Task> action, object state, TimeSpan delay)
        {
            if (delay <= TimeSpan.Zero)
            {
                Execute(action, state);
                return;
            }
            Schedule(new TaskRunnable(action, state), delay);
        }

        private void Schedule(IScheduledRunnable task)
        {
            StartRunDelayTask();

            this.Execute((state) =>
            {
                lock (ScheduledTaskQueue)
                {
                    if (this.ScheduledTaskQueue.Count > MaxTaskCount) throw new Exception($"延迟任务队列超过{MaxTaskCount}条");
                    this.ScheduledTaskQueue.Enqueue(task);
                    Monitor.Pulse(ScheduledTaskQueue);
                }
                return Task.CompletedTask;
            }, null);
        }

        public int GetTaskCount()
        {
            return this._taskQueue.Count;
        }

        public void Start()
        {
            if (_isStart) return;
            lock (this)
            {
                if (_isStart) return;
                _isStart = true;
            }

            Task.Run(() =>
            {
                StartRunTask();
                //StartRunDelayTask();
            });
        }

        public void Stop()
        {
            if (this._isStart == false) return;
            lock (this)
            {
                if (this._isStart)
                {
                    //int tryIndex = 0;
                    //while (_taskQueue.Count > 0 && tryIndex < 5)
                    //{
                    //    tryIndex++;
                    //    Thread.Sleep(100);
                    //}

                    this._isStart = false;
                    this._isStartDelay = false;
                    _taskQueue.CompleteAdding();
                    CancellationTokenSource.Cancel();
                }
            }
        }

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }
}
