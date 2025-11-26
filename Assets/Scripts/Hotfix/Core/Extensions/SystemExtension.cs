using System;
using System.Collections.Generic;

namespace HotfixCore.Extensions
{
    public static class SystemExtension
    {
        private static readonly Random _globalRandom = new Random();

        /// <summary>
        /// 随机打乱List顺序
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        public static void Shuffle<T>(this List<T> list)
        {
            if (list == null || list.Count <= 1)
                return;

            var random = _globalRandom;
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static T[] Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = _globalRandom.Next(n + 1);
                (array[k], array[n]) = (array[n], array[k]);
            }

            return array;
        }
    }
}