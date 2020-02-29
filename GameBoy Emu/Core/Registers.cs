using System;

namespace GameBoy_Emu.Core
{
    internal class Registers
    {
        public const byte ZERO_FLAG = 0x80;
        public const byte NEGATIVE_FLAG = 0x40;
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
        public void SetFlags(byte zeroFlag, byte negFlag, byte halfFlag, byte fullFlag)
        {
            //TODO Fix this shit
            F = (byte)(ZERO_FLAG & zeroFlag);
            F = (byte)(NEGATIVE_FLAG & negFlag);
            F = (byte)(HALF_CARRY_FLAG & halfFlag);
            F = (byte)(FULL_CARRY_FLAG & fullFlag);
        }

        public void SetZFLag(byte val)
        {
            F = (byte)(val | ZERO_FLAG);
        }

        public int GetZFlag()
        {
            return F & ZERO_FLAG;
        }
        public void SetNFLag(byte val)
        {
            F = (byte)(val | NEGATIVE_FLAG);
        }

        public int GetNFlag()
        {
            return F & NEGATIVE_FLAG;
        }

        public void SetHCYFLag(byte val)
        {
            F = (byte)(val | HALF_CARRY_FLAG);
        }
        public int GetHCYFlag()
        {
            return F & HALF_CARRY_FLAG;
        }
        public void SetCYFLag(byte val)
        {
            F = (byte)(val | FULL_CARRY_FLAG);
        }
        public int GetCYFlag()
        {
            return F & FULL_CARRY_FLAG;
        }

    }
}