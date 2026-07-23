using System;
using System.Collections.Generic;
using System.Linq;
using Blunatic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Parsing
{
    public static class Hex
    {
        private static char[] _valToChar = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        private static Dictionary<char, byte> _charToVal = new Dictionary<char, byte>
        {
            { '0', _initChar('0') },
            { '1', _initChar('1') },
            { '2', _initChar('2') },
            { '3', _initChar('3') },
            { '4', _initChar('4') },
            { '5', _initChar('5') },
            { '6', _initChar('6') },
            { '7', _initChar('7') },
            { '8', _initChar('8') },
            { '9', _initChar('9') },
            { 'A', _initChar('A') },
            { 'B', _initChar('B') },
            { 'C', _initChar('C') },
            { 'D', _initChar('D') },
            { 'E', _initChar('E') },
            { 'F', _initChar('F') },
        };

        private static byte _initChar(char c)
        {
            return (byte)Array.IndexOf(_valToChar, c);
        }

        public static string GetString(params byte[] bytes)
        {
            string returnValue = string.Empty;

            foreach (byte b in bytes)
            {
                returnValue += $"{_valToChar[b / 16]}{_valToChar[b % 16]}";
            }

            return returnValue;
        }
        public static string GetString(ushort value)
        {
            return GetString((byte)(value >> 8), (byte)value);
        }
        public static byte[] GetBytes(string input)
        {
            input = input.ToUpper();
            byte[] returnItems = new byte[input.Length / 2];
            for (int i = 0; i < returnItems.Length; i++)
            {
                returnItems[i] = (byte)(_charToVal[input[i * 2]] * 16 + _charToVal[input[i * 2 + 1]]);
            }
            return returnItems;
        }

        public static Microsoft.Xna.Framework.Color GetColor(string bareHex)
        {
            byte[] bytes = GetBytes(bareHex);
            if (bytes.Length == 3) return new Microsoft.Xna.Framework.Color(bytes[0], bytes[1], bytes[2]);
            if (bytes.Length == 4) return new Microsoft.Xna.Framework.Color(bytes[0], bytes[1], bytes[2], bytes[3]);
            throw new BlunaticException($"Input hex was not a valid length ({bareHex} is {bareHex.Length} characters long, which is {bytes.Length} bytes)");
        }
        public static Microsoft.Xna.Framework.Color GetColor(Microsoft.Xna.Framework.Color color, byte alpha)
        {
            return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, alpha);
        }
        public static string GetString(Microsoft.Xna.Framework.Color color, bool includeAlpha = false)
        {
            return includeAlpha ? GetString(color.R, color.G, color.B, color.A) : GetString(color.R, color.G, color.B);
        }
    }
}
