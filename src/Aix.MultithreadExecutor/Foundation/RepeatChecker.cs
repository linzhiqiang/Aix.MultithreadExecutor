using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.MultithreadExecutor.Foundation
{
    /// <summary>
    /// 防重检测
    /// </summary>
    public class RepeatChecker
    {
        private volatile bool _state = false;

        public bool Check()
        {
            if (_state) return false;
            lock (this)
            {
                if (_state) return false;
                _state = true;
                return _state;
            }
        }
    }
}
