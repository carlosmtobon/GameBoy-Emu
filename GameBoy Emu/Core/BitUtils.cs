using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.Core
{
    public static class BitUtils
    {
        public static byte ClearBit(byte val, byte bitToClear)
        {
            val &= (byte)~(bitToClear);
            return val;
        }

        public static byte SetBit(byte val, byte bitToSet)
        {
            val |= (byte)(bitToSet);
            return val;
        }

        public static byte GetHighOrderByte(ushort value)
        {
           return (byte)(value >> 8);
        }

        public static byte GetLowOrderByte(ushort value)
        {
            return (byte)(value & 0x00FF);
        }

        public static ushort GetU16(byte highByte, byte lowByte)
        {
            return (ushort)(highByte << 8 | lowByte);
        }
    }
}
