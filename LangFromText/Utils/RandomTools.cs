using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.Utils
{
    public static class RandomTools
    {
        public static IList<T> Shuffle<T>(this IList<T> list, Random rnd)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static TNum[] GeneratePostupnost<TNum>(int fromInclusive, int toInclusive) where TNum : struct
        {
            TNum[] res = new TNum[toInclusive - fromInclusive + 1];

            for (int i = 0; i <= (toInclusive - fromInclusive); i++)
                res[i] = (TNum)Convert.ChangeType((fromInclusive + i), typeof(TNum));

            return res;
        }
    }
}
