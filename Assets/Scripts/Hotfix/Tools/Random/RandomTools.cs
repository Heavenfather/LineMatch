using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Log;

namespace Hotfix.Tools.Random
{
    public class RandomTools
    {
        /// <summary>
        /// 获取唯一GUID
        /// </summary>
        public static string GetGUID(int len)
        {
            len = (len > 32) ? 32 : len;
            System.Guid guid = new System.Guid();
            guid = Guid.NewGuid();
            var id = guid.ToString("N").Substring(0, len);
            return id;
        }

        /// <summary>
        /// 获取随机数，长度不能超过18位数(long最大值19位数)
        /// </summary>
        public static long RandomNumber(int len)
        {
            len = (len > 18) ? 18 : len;
            string number = "";
            for (var i = 0; i < 2; i++)
            {
                int seed = GetRandomSeed();
                System.Random random = new System.Random(seed);
                int n = random.Next();
                number += n + "";
            }

            return Convert.ToInt64(number.Substring(0, len));
        }

        /// <summary>
        /// 根据权重获取随机数据(基于前缀和算法，复杂度为O(1))
        /// </summary>
        /// <param name="weights">权重列表</param>
        /// <param name="values">待抽取的数据列表</param>
        /// <param name="count">要抽取个数</param>
        /// <param name="withoutReplacement">不放回抽样</param>
        /// <returns></returns>
        public static List<T> WeightedRandomValues<T>(IEnumerable<int> weights, IEnumerable<T> values, int count, bool withoutReplacement)
        {
            if (values.Count() <= 0 || weights.Count() <= 0 || (weights.Count() != values.Count()) || count <= 0 || (withoutReplacement && values.Count() <= count))
            {
                Logger.Error("格式错误");
                return new();
            }

            System.Random random = new(GetRandomSeed());
            List<Tuple<double, int, T>> prefixSum = new(); //前缀和数据 (sum, weight, value)

            double totalWeights = 0;

            using var valueEnumerator = values.GetEnumerator();
            using var weightEnumerator = weights.GetEnumerator();
            {
                while (valueEnumerator.MoveNext() && weightEnumerator.MoveNext())
                {
                    var value = valueEnumerator.Current;
                    var weight = weightEnumerator.Current;

                    prefixSum.Add(new(weight + totalWeights, weight, value));
                    totalWeights += weight;
                }
            }

            List<T> l = new();
            for (var i = 0; i < count; i++)
            {
                double randomWeight = random.NextDouble() * totalWeights;
                // 二分查找随机权重所在的位置
                int index = prefixSum.BinarySearch(Tuple.Create(randomWeight, 0, default(T)),
                    Comparer<Tuple<double, int, T>>.Create((a, b) => a.Item1.CompareTo(b.Item1)));
                
                if (index < 0)
                    index = ~index;

                l.Add(prefixSum[index].Item3);

                if (withoutReplacement)
                {
                    var weight = prefixSum[index].Item2;
                    totalWeights -= weight;
                    for (int j = index + 1; j < prefixSum.Count; j++)
                    {
                        prefixSum[j] = new(prefixSum[j].Item1 - weight, prefixSum[j].Item2, prefixSum[j].Item3);
                    }
                    prefixSum.RemoveAt(index);
                }
            }

            return l;
        }

        public static int WeightedRandomIndex(List<int> weights)
        {
            if (weights is null || weights.Count() <= 0)
                return -1;
            
            int totalWeight = 0;
            List<int> prefixSum = new();
            foreach (var weight in weights)
            {
                totalWeight += weight;
                prefixSum.Add(totalWeight);
            }
            
            int index = prefixSum.BinarySearch(RandomRange(0, totalWeight));
            if (index < 0)
                index = ~index;
            
            return index;
        }

        /// <summary>
        /// 随机区间一个数字(含左右区间)
        /// </summary>
        public static int RandomRange(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// 随机区间一个数字(含左右区间)
        /// </summary>
        public static float RandomRange(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// 随机区间一个数字(含左右区间)
        /// </summary>
        public static int RandomRange(double min, double max)
        {
            return UnityEngine.Mathf.FloorToInt(UnityEngine.Random.Range((float)min, (float)max));
        }

        //随机种子
        public static int GetRandomSeed()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            int seed = BitConverter.ToInt32(buffer, 0);
            return seed;
        }
    }
}