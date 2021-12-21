using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Aix.MultithreadExecutorSample.Utils
{
    public static class RandomUtils
    {
        /// <summary>
        /// 随机数 [minValue,maxValue)
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int minValue, int maxValue)
        {
            Random r = new Random(GetRandomSeed());
            return r.Next(minValue, maxValue);
        }

        /// <summary>
        /// 随机数 [0,1)
        /// </summary>
        /// <returns></returns>
        public static double NextDouble()
        {
            Random r = new Random(GetRandomSeed());
            return r.NextDouble();
        }



        /// <summary>
        /// 获取相对安全的随机数种子
        /// 速度较慢，不适合用于短时间获取大量种子的需求
        /// </summary>
        /// <returns></returns>
        private static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
