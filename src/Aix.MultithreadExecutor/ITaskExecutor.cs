﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.MultithreadExecutor
{
    /// <summary>
    /// 任务执行器
    /// </summary>
    public interface ITaskExecutor : IDisposable
    {
        /// <summary>
        /// 执行任务 (任务队列等待执行)
        /// </summary>
        /// <param name="action"></param>
        void Execute(Func<object,Task> action, object state);

        void Execute(IRunnable task);

        /// <summary>
        /// 执行延迟任务
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        void Schedule(IRunnable action, TimeSpan delay);

        /// <summary>
        /// 执行延迟任务
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        void Schedule(Func<object,Task> action, object state,TimeSpan delay);

        void Start();

        void Stop();

        /// <summary>
        /// 获取单线程执行器 根据routeId获取固定的执行器 （保证相同的routeId使用同一个线程执行器）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        ITaskExecutor GetSingleThreadTaskExecutor(int routeId);

        /// <summary>
        /// 获取单线程执行器 根据routeKey获取固定的执行器 （保证相同的routeKey使用同一个线程执行器）
        /// </summary>
        /// <param name="routeKey"></param>
        /// <returns></returns>
        ITaskExecutor GetSingleThreadTaskExecutor(string routeKey);

        /// <summary>
        /// 获取待执行任务数量(即时任务，不包括延迟任务)
        /// </summary>
        /// <returns></returns>
        int GetTaskCount();

        /// <summary>
        /// 异常事件
        /// </summary>
        event Func<Exception, Task> OnException;
    }
}
