namespace ChichoGB.Core
{
    public static class BitUtils
    {
        public static byte ClearBit(byte val, int bitToClear)
        {
            val &= (byte)~(bitToClear);
            return val;
        }

        public static byte SetBit(byte val, int bitToSet)
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

        public static byte GetBit(byte value, int bitPosition)
        {
            return (byte)((value >> bitPosition) & 1);
        }

        public static bool isBitSet(byte value, int bitPosition)
        {
            return GetBit(value, bitPosition) == 1;
        }
    }
}
