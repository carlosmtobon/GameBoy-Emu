using System;

namespace GameBoy_Emu.Core
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


        public void SetAF(ushort value)
        {
            A = (byte)(value >> 8);
            F = (byte)(value & 0x00FF);
        }

        public ushort GetAF()
        {
            return (ushort)(A << 8 | F);
        }
       
        public void SetBC(ushort value)
        {
            B = (byte)(value >> 8);
            C = (byte)(value & 0x00FF);
        }

        public ushort GetBC()
        {
            return (ushort)(B << 8 | C);
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
            D = (byte)(value >> 8);
            E = (byte)(value & 0x00FF);
        }

        public ushort GetDE()
        {
            return (ushort)(D << 8 | E);
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
            H = (byte)(value >> 8);
            L = (byte)(value & 0x00FF);
        }
        public ushort GetHL()
        {
            return (ushort)(H << 8 | L);
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
            F = zSet ? SetBit(F, ZERO_FLAG) : ClearBit(F, ZERO_FLAG);
        }

        public byte GetZFlag()
        {
            return (byte)((F & ZERO_FLAG) >> 7);
        }
        public void SetNFLag(bool nSet)
        {
            F = nSet ? SetBit(F, SUBTRACT_FLAG) : ClearBit(F, SUBTRACT_FLAG);
        }

        public byte GetNFlag()
        {
            return (byte)((F & SUBTRACT_FLAG) >> 6);
        }

        public void SetHCYFLag(bool hcySet)
        {
             F = hcySet ? SetBit(F, HALF_CARRY_FLAG) : ClearBit(F, HALF_CARRY_FLAG);
        }
        public byte GetHCYFlag()
        {
            return (byte)((F & HALF_CARRY_FLAG) >> 5);
        }
        public void SetCYFLag(bool cySet)
        {
            F = cySet ? SetBit(F, FULL_CARRY_FLAG) : ClearBit(F, FULL_CARRY_FLAG);
        }
        public byte GetCYFlag()
        {
            return (byte)((F & FULL_CARRY_FLAG) >> 4);
        }

        public byte ClearBit(byte val, byte bitToClear)
        {
            val &= (byte)~(bitToClear);
            return val;
        }

        public byte SetBit(byte val, byte bitToSet)
        {
            val |= (byte)(bitToSet);
            return val;
        }
    }
}