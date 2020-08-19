using System;
using System.Linq;

namespace NatsTask.Common.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            var arr = new byte[hex.Length >> 1];

            for (var i = 0; i < hex.Length >> 1; ++i)
                arr[i] = (byte) ((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));

            return arr;
        }

        public static byte[] StringToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private static int GetHexVal(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : val < 97 ? 55 : 87);
        }
    }
}