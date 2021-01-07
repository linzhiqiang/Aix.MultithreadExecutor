using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Aix.MultithreadExecutor.Foundation
{
    internal interface IBlockingQueue<T>
    {
        int Count { get; }

        bool Enqueue(T item);

        /// <summary>
        /// 停止加入队列
        /// </summary>
        void CompleteAdding();

        /// <summary>
        /// 阻塞
        /// </summary>
        /// <returns></returns>
        T Dequeue(CancellationToken cancellationToken = default(CancellationToken));

        IEnumerable<T> GetConsumingEnumerable(CancellationToken cancellationToken);

        bool TryDequeue(out T item);



    }
}
