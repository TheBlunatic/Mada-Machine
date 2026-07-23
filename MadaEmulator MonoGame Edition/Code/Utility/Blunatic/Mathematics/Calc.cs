using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Mathematics
{
    public static class Calc
    {
        public static int WrapMod(int a, int b) => (b + a % b) % b;
        public static double WrapMod(double a, double b) => (b + a % b) % b;
        public static float WrapMod(float a, float b) => (b + a % b) % b;
        public static long WrapMod(long a, long b) => (b + a % b) % b;
    }
}
