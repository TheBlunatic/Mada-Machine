using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Mathematics
{
    public static class Iterate
    {
        public static IEnumerable<int> InBounds(int lower, int upper)
        {
            int i = lower;
            while (i < upper)
            {
                yield return i++;
            }
        }

        public static void Each<T>(IEnumerable<T> enumerable, Action<T> action )
        {
            foreach (T item in enumerable) action(item);
        }
        public static void Each<T>(IEnumerable<T> enumerable, Func<T, bool> condition, Action<T> action)
        {
            foreach (T item in enumerable) if (condition(item)) action(item);
        }
        public static void Each<T>(IEnumerable<T> enumerable, Func<T, bool> condition, Action<T> action, bool breakWhenTrue)
        {
            if (!breakWhenTrue)
            {
                Each(enumerable, condition, action);
                return;
            }
            foreach (T item in enumerable)
            {
                if (condition(item))
                {
                    action(item);
                    return;
                }
            }
        }
    }
}
