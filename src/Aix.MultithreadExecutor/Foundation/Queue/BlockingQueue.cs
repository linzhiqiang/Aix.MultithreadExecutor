using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace  Aix.MultithreadExecutor.Foundation
{
    public class BlockingQueue<T> : IBlockingQueue<T>
    {
        private BlockingCollection<T> BlockQueue;

        public BlockingQueue()
        {
            BlockQueue = new BlockingCollection<T>(new ConcurrentQueue<T>());
        }
        public int Count
        {
            get
            {
                return BlockQueue.Count;
            }
        }

        public bool Enqueue(T item)
        {
            if (!BlockQueue.IsAddingCompleted)
            {
                BlockQueue.Add(item);
                return true;
            }
            return false;
            //  throw new Exception("阻塞队列已停止服务");
        }
        public T Dequeue(CancellationToken cancellationToken = default(CancellationToken))
        {
            return BlockQueue.Take(cancellationToken);
        }

        public IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            return BlockQueue.GetConsumingEnumerable(cancellationToken);
        }

        /// <summary>
        /// 停止加入队列
        /// </summary>
        public void CompleteAdding()
        {
            BlockQueue.CompleteAdding();
        }

        public bool TryDequeue(out T item)
        {
            return BlockQueue.TryTake(out item);
        }
    }
}
