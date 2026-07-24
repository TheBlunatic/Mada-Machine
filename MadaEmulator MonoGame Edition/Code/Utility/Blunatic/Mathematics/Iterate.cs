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
    }
}
