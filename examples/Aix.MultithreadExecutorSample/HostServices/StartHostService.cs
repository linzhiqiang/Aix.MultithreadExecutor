﻿using Aix.MultithreadExecutor;
using Aix.MultithreadExecutor.Foundation;
using Aix.MultithreadExecutorSample.Utils;
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

        private readonly MyMultithreadTaskExecutor _taskExecutor;

        public StartHostService(ILogger<StartHostService> logger, IServiceProvider serviceProvider, IHostEnvironment hostEnvironment
           , MyMultithreadTaskExecutor taskExecutor)
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

        #region test


        IBlockingQueue<NodeInfo> Queue = new BlockingQueue<NodeInfo>();

        CancellationTokenSource CTS = new CancellationTokenSource();
        private Task Test()
        {
            List<NodeInfo> nodeList = new List<NodeInfo>();
            for (int i = 0; i < 250; i++)
            {
                nodeList.Add(new NodeInfo
                {
                    Id = i,
                    StartTime = 2,
                    ExposureTime = RandomUtils.Next(5, 10),
                    ReadTime = 3,
                    Opstate = Opstate.Start
                });
            }

            foreach (var item in nodeList.OrderBy(x => x.ExposureTime))
            {
                Queue.Enqueue(item);
            }
            List<ResultInfo> result = new List<ResultInfo>();

            DateTime endTime = DateTime.Now.AddSeconds(5);

            var token = CTS.Token;

            Task.Run(async () =>
            {

                await Task.Delay(3000);

                while (true)
                    if (DateTime.Now > endTime)
                    {
                        CTS.Cancel();
                    }
            });

            try
            {
                while (true)
                {
                    // if (DateTime.Now > endTime) break;
                    var item = Queue.Dequeue(token);
                    if (item.Opstate == Opstate.Start)
                    {
                        var delay = item.StartTime + item.ExposureTime;
                        result.Add(new ResultInfo { Id = item.Id, Opstate = Opstate.Start, Delay = delay });
                        
                        _taskExecutor.Schedule(obj =>
                        {
                            var nodeInfo = obj as NodeInfo;
                            nodeInfo.Opstate = Opstate.Read;
                            Queue.Enqueue(nodeInfo); //可以读取了

                            return Task.CompletedTask;
                        }, item, TimeSpan.FromMilliseconds(item.StartTime + item.ExposureTime));
                    }
                    else if (item.Opstate == Opstate.Read)
                    {
                        //读取 
                        if (DateTime.Now <= endTime)
                        {
                            result.Add(new ResultInfo { Id = item.Id, Opstate = Opstate.Read, Delay = item.ReadTime });
                            
                            _taskExecutor.Schedule(obj =>
                            {
                                var nodeInfo = obj as NodeInfo;
                                nodeInfo.Opstate = Opstate.Start;
                                Queue.Enqueue(nodeInfo); //读取完成，开始下次启动

                                return Task.CompletedTask;
                            }, item, TimeSpan.FromMilliseconds(item.ReadTime));
                        }
                    }


                }
            }
            catch (OperationCanceledException ex)
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


            //ResultInfo last = null;
            //foreach (var item in result)
            //{
            //    if (last == null)
            //    {
            //        item.Delay = 0;
            //    }
            //    else
            //    {
            //        item.Delay = (item.Time - last.Time).TotalMilliseconds;
            //    }
            //    last = item;
            //}

            foreach (var item in result)
            {

            }


            return Task.CompletedTask;
        }

        #endregion

        private Task Test1()
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

    public class NodeInfo
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 启动时间
        /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        /// 曝光时间 millisecond  可以并行执行
        /// </summary>
        public int ExposureTime { get; set; }

        /// <summary>
        /// 读取时间
        /// </summary>
        public int ReadTime { get; set; }

        public Opstate Opstate { get; set; }
    }

    public class ResultInfo
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }

        public Opstate Opstate { get; set; }

        // public DateTime Time { get; set; }

        /// <summary>
        /// 等待时间
        /// </summary>
        public double Delay { get; set; }


    }

    public enum Opstate
    {
        Start = 0,

        Read = 1
    }
}
