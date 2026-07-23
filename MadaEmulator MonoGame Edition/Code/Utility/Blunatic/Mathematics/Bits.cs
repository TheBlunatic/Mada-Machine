using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Mathematics
{
    public static class Bits
    {
        public static int PopCount(byte value) => BitOperations.PopCount(value);
        public static int PopCount(uint value) => BitOperations.PopCount(value);
        public static int PopCount(ulong value) => BitOperations.PopCount(value);
        public static int PopCount(ushort value) => BitOperations.PopCount(value);
        public static int PopCount(int value) => BitOperations.PopCount((uint)value);
        public static int PopCount(long value) => BitOperations.PopCount((ulong)value);
        public static int PopCount(short value) => BitOperations.PopCount((ushort)value);
    }
}
