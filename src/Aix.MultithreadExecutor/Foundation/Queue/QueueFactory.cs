using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.MultithreadExecutor.Foundation
{
    internal class QueueFactory
    {
        public static QueueFactory Instance = new QueueFactory();

        private QueueFactory() { }

        public IBlockingQueue<T> CreateBlockingQueue<T>()
        {
            return new BlockingQueue<T>();
        }
    }
}
