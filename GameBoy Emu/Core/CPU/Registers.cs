namespace GameBoy_Emu.core.cpu
{
    internal class Registers
    {
        public const byte ZERO_FLAG = 0x80;
        public const byte SUBTRACT_FLAG = 0x40;
        public const byte HALF_CARRY_FLAG = 0x20;
        public const byte FULL_CARRY_FLAG = 0x10;

        public byte A { get; set; }
        public byte F { get; set; }
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }

        public Registers()
        {
            SetAF(0x01B0);
            SetBC(0x0013);
            SetDE(0x00D8);
            SetHL(0x014D);
        }
        public void SetAF(ushort value)
        {

            A = BitUtils.GetHighOrderByte(value);
            F = BitUtils.GetLowOrderByte(value);
        }

        public ushort GetAF()
        {
            return BitUtils.GetU16(A, F);
        }

        public void SetBC(ushort value)
        {
            B = BitUtils.GetHighOrderByte(value);
            C = BitUtils.GetLowOrderByte(value);
        }

        public ushort GetBC()
        {
            return BitUtils.GetU16(B, C);
        }

        public void AddToBC(ushort val)
        {
            SetBC((ushort)(GetBC() + val));
        }

        public void SubToBC(ushort val)
        {
            SetBC((ushort)(GetBC() - val));
        }


        public void SetDE(ushort value)
        {
            D = BitUtils.GetHighOrderByte(value);
            E = BitUtils.GetLowOrderByte(value);
        }

        public ushort GetDE()
        {
            return BitUtils.GetU16(D, E);
        }

        public void AddToDE(ushort val)
        {
            SetDE((ushort)(GetDE() + val));
        }
        public void SubToDE(ushort val)
        {
            SetDE((ushort)(GetDE() - val));
        }
        public void SetHL(ushort value)
        {
            H = BitUtils.GetHighOrderByte(value);
            L = BitUtils.GetLowOrderByte(value);
        }
        public ushort GetHL()
        {
            return BitUtils.GetU16(H, L);
        }

        public void AddToHL(ushort val)
        {
            SetHL((ushort)(GetHL() + val));
        }
        public void SubToHL(ushort val)
        {
            SetHL((ushort)(GetHL() - val));
        }

        public void SetZFLag(bool zSet)
        {
            F = zSet ? BitUtils.SetBitsWithMask(F, ZERO_FLAG) : BitUtils.ClearBitsWithMask(F, ZERO_FLAG);
        }

        public byte GetZFlag()
        {
            return (byte)((F & ZERO_FLAG) >> 7);
        }
        public void SetNFLag(bool nSet)
        {
            F = nSet ? BitUtils.SetBitsWithMask(F, SUBTRACT_FLAG) : BitUtils.ClearBitsWithMask(F, SUBTRACT_FLAG);
        }

        public byte GetNFlag()
        {
            return (byte)((F & SUBTRACT_FLAG) >> 6);
        }

        public void SetHCYFLag(bool hcySet)
        {
            F = hcySet ? BitUtils.SetBitsWithMask(F, HALF_CARRY_FLAG) : BitUtils.ClearBitsWithMask(F, HALF_CARRY_FLAG);
        }
        public byte GetHCYFlag()
        {
            return (byte)((F & HALF_CARRY_FLAG) >> 5);
        }
        public void SetCYFLag(bool cySet)
        {
            F = cySet ? BitUtils.SetBitsWithMask(F, FULL_CARRY_FLAG) : BitUtils.ClearBitsWithMask(F, FULL_CARRY_FLAG);
        }
        public byte GetCYFlag()
        {
            return (byte)((F & FULL_CARRY_FLAG) >> 4);
        }


    }
}