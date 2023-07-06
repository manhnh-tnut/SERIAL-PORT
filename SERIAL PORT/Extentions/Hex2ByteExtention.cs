using System;
using System.Linq;

namespace Extentions
{
    public class Hex2ByteExtention
    {
        public static byte[] StringToByteArray(string hex)
        {
            try
            {
                return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
            }
            catch
            {
                return new byte[0];
            }
        }
    }
}
