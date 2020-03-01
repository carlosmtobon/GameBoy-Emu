using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.Core
{
    class CPU
    {
        public int CpuCycles { get; set; }
        public byte Opcode { get; set; }
        public ushort PC { get; set; }
        public ushort SP { get; set; }
        internal Registers Registers { get; }
        private RAM _ram;

        public CPU(RAM ram)
        {
            _ram = ram;
            Registers = new Registers();
            PC = 0x0100;
            SP = (ushort)(ram.Memory.Length - 1);
        }

        public void UpdatePCAndCycles(ushort val, int cycles)
        {
            PC += val;
            CpuCycles += cycles;
        }

        public void Push()
        {
            _ram.Memory[SP - 1] = (byte)(PC & 0x00FF);
            _ram.Memory[SP - 2] = (byte)(PC >> 8);
            SP -= 2;
        }

        public ushort Pop()
        {
            SP+=2;
            return (ushort)(_ram.Memory[SP] << 8 | _ram.Memory[SP + 1]); ;
        }

        public void ProcessOpcode()
        {
            Opcode = _ram.Memory[PC];
            switch (Opcode)
            {
                case 0x00:
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x01:
                    Registers.SetBC(_ram.LoadU16Bits(PC + 1));
                    UpdatePCAndCycles(3, 12);
                    break;
                case 0x02:
                    _ram.StoreU8Bits(Registers.GetBC(), Registers.A);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x03:
                    Registers.AddToBC(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x04:
                    Registers.B++;
                    //SET FLAGS
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x05:
                    Registers.B--;
                    //SET FLAGS
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x06:
                    Registers.B = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x07:
                // RLCA
                case 0x08:
                    ushort storeAddr = _ram.LoadU16Bits(PC + 1);
                    _ram.StoreU16Bits(storeAddr, SP);
                    UpdatePCAndCycles(3, 20);
                    break;
                case 0x09:
                    Registers.AddToHL(Registers.GetBC());
                    //SET FLAGS
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x0A:
                    Registers.A = _ram.LoadU8Bits(Registers.GetBC());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x0B:
                    Registers.SubToBC(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x0C:
                    Registers.C++;
                    // SET FLAGS
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x0D:
                    Registers.C--;
                    //SET FLAGS
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x0E:
                    Registers.C = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x0F:
                    //RRCA
                    PC++;
                    break;
                case 0x10:
                    // STOP
                    PC += 2;
                    break;
                case 0x11:
                    Registers.SetDE(_ram.LoadU16Bits(PC + 1));
                    UpdatePCAndCycles(3, 12);
                    break;
                case 0x12:
                    _ram.StoreU8Bits(Registers.GetDE(), Registers.A);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x13:
                    Registers.AddToDE(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x14:
                    Registers.D++;
                    //Set Flags
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x15:
                    Registers.D--;
                    // SET FLAGS
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x16:
                    Registers.D = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x17:
                    var oldBit7 = Registers.A & 0x80; // CY flag
                    // SET FLAGS
                    Registers.A <<= 1;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x18:
                    PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                    UpdatePCAndCycles(2, 12);
                    break;
                case 0x19:
                    Registers.AddToHL(Registers.GetDE());
                    // SET FLAGS
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x1A:
                    Registers.A = _ram.LoadU8Bits(Registers.GetDE());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x1B:
                    Registers.SubToDE(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x1C:
                    Registers.E++;
                    //Set Flags
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x1D:
                    Registers.E--;
                    //Set flags
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x1E:
                    Registers.E = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x1F:
                    var oldBit0 = Registers.A & 1;
                    // set CY flag
                    Registers.A >>= 1;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x20:
                    // JR NZ,i8
                    if (Registers.GetZFlag() == 0)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        UpdatePCAndCycles(0, 12);
                    }
                    else
                    {
                        UpdatePCAndCycles(2, 8);
                    }
                    
                    break;
                case 0x21:
                    Registers.SetHL(_ram.LoadU16Bits(PC + 1));
                    UpdatePCAndCycles(3, 12);
                    break;
                case 0x22:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.A);
                    Registers.AddToHL(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x23:
                    Registers.AddToHL(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x24:
                    Registers.H++;
                    // set flags
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x25:
                    Registers.H--;
                    // set flags
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x26:
                    Registers.H = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x27:
                    //DAA
                    //set flags
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x28:
                    //JR Z,i8 
                    if (Registers.GetZFlag() == 1)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        UpdatePCAndCycles(0, 12);
                    }
                    else
                    {
                        UpdatePCAndCycles(2, 8);
                    }
                    break;
                case 0X29:
                    Registers.AddToHL(Registers.GetHL());
                    //Set flags
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x2A:
                    Registers.A = _ram.LoadU8Bits(Registers.GetHL());
                    Registers.AddToHL(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x2B:
                    Registers.SubToHL(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x2C:
                    Registers.L++;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x2D:
                    Registers.L--;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x2E:
                    Registers.L = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x2F:
                    Registers.A = (byte)~Registers.A;
                    Registers.SetNFLag(true);
                    Registers.SetHCYFLag(true);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x30:
                    if (Registers.GetCYFlag() == 0)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        UpdatePCAndCycles(0, 12);
                    } 
                    else
                    {
                        UpdatePCAndCycles(2, 8);
                    }
                    break;
                case 0x31:
                    SP = _ram.LoadU16Bits(PC + 1);
                    UpdatePCAndCycles(3, 12);
                    break;
                case 0x32:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.A);
                    Registers.SubToHL(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x33:
                    SP++;
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x34:
                    Registers.AddToHL(1);
                    //set flag
                    UpdatePCAndCycles(1, 12);
                    break;
                case 0x35:
                    Registers.SubToHL(1);
                    //set flag
                    UpdatePCAndCycles(1, 12);
                    break;
                case 0x36:
                    _ram.StoreU8Bits(Registers.GetHL(), _ram.LoadU8Bits(PC + 1));
                    UpdatePCAndCycles(2, 12);
                    break;
                case 0x37:
                    Registers.SetCYFLag(true);
                    Registers.SetNFLag(false);
                    Registers.SetHCYFLag(false);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x38:
                    if (Registers.GetCYFlag() == 1)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        UpdatePCAndCycles(0, 12);
                    }
                    else
                    {
                        UpdatePCAndCycles(2, 8);
                    }
                    break;
                case 0x39:
                    Registers.AddToHL(SP);
                    //set flags
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x3A:
                    Registers.A = _ram.LoadU8Bits(Registers.GetHL());
                    Registers.SubToHL(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x3B:
                    SP--;
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x3C:
                    Registers.A++;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x3D:
                    Registers.A--;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x3E:
                    Registers.A = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x3F:
                   // Registers.SetCYFLag((bool)(~Registers.GetCYFlag()));
                    Registers.SetNFLag(false);
                    Registers.SetHCYFLag(false);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x40:
                    Registers.B = Registers.B;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x41:
                    Registers.B = Registers.C;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x42:
                    Registers.B = Registers.D;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x43:
                    Registers.B = Registers.E;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x44:
                    Registers.B = Registers.H;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x45:
                    Registers.B = Registers.L;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x46:
                    Registers.B = _ram.LoadU8Bits(Registers.GetHL());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x47:
                    Registers.B = Registers.A;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x48:
                    Registers.C = Registers.B;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x49:
                    Registers.C = Registers.C;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x4A:
                    Registers.C = Registers.D;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x4B:
                    Registers.C = Registers.E;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x4C:
                    Registers.C = Registers.H;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x4D:
                    Registers.C = Registers.L;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x4E:
                    Registers.C = _ram.LoadU8Bits(Registers.GetHL());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x4F:
                    Registers.C = Registers.A;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x50:
                    Registers.D = Registers.B;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x51:
                    Registers.D = Registers.C;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x52:
                    Registers.D = Registers.D;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x53:
                    Registers.D = Registers.E;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x54:
                    Registers.D = Registers.H;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x55:
                    Registers.D = Registers.L;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x56:
                    Registers.D = _ram.LoadU8Bits(Registers.GetHL());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x57:
                    Registers.D = Registers.A;
                     UpdatePCAndCycles(1, 4);
                    break;
                case 0x58:
                    Registers.E = Registers.B;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x59:
                    Registers.E = Registers.C;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x5A:
                    Registers.E = Registers.D;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x5B:
                    Registers.E = Registers.E;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x5C:
                    Registers.E = Registers.H;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x5D:
                    Registers.E = Registers.L;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x5E:
                    Registers.E = _ram.LoadU8Bits(Registers.GetHL());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x5F:
                    Registers.E = Registers.A;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x60:
                    Registers.H = Registers.B;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x61:
                    Registers.H = Registers.C;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x62:
                    Registers.H = Registers.D;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x63:
                    Registers.H = Registers.E;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x64:
                    Registers.H = Registers.H;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x65:
                    Registers.H = Registers.L;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x66:
                    Registers.H = _ram.LoadU8Bits(Registers.GetHL());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x67:
                    Registers.H = Registers.A;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x68:
                    Registers.L = Registers.B;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x69:
                    Registers.L = Registers.C;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x6A:
                    Registers.L = Registers.D;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x6B:
                    Registers.L = Registers.E;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x6C:
                    Registers.L = Registers.H;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x6D:
                    Registers.L = Registers.L;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x6E:
                    Registers.L = _ram.LoadU8Bits(Registers.GetHL());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x6F:
                    Registers.L = Registers.A;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x70:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.B);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x71:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.C);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x72:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.D);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x73:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.E);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x74:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.H);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x75:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.L);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x76:
                    //halt;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x77:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.A);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x78:
                    Registers.A = Registers.B;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x79:
                    Registers.A = Registers.C;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x7A:
                    Registers.A = Registers.D;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x7B:
                    Registers.A = Registers.E;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x7C:
                    Registers.A = Registers.H;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x7D:
                    Registers.A = Registers.L;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x7E:
                    Registers.A = _ram.LoadU8Bits(Registers.GetHL());
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x7F:
                    Registers.A = Registers.A;
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x80:
                    Registers.A += Registers.B;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x81:
                    Registers.A += Registers.C;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x82:
                    Registers.A += Registers.D;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x83:
                    Registers.A += Registers.E;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x84:
                    Registers.A += Registers.H;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x85:
                    Registers.A += Registers.L;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x86:
                    Registers.A += _ram.LoadU8Bits(Registers.GetHL());
                    //set flag
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x87:
                    Registers.A += Registers.A;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x88:
                    Registers.A += (byte)(Registers.B + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x89:
                    Registers.A += (byte)(Registers.C + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x8A:
                    Registers.A += (byte)(Registers.D + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x8B:
                    Registers.A += (byte)(Registers.E + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x8C:
                    Registers.A += (byte)(Registers.H + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x8D:
                    Registers.A += (byte)(Registers.L + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x8E:
                    Registers.A += (byte)(_ram.LoadU8Bits(Registers.GetHL()) + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x8F:
                    Registers.A += (byte)(Registers.A + Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x90:
                    Registers.A -= Registers.B;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x91:
                    Registers.A -= Registers.C;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x92:
                    Registers.A -= Registers.D;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x93:
                    Registers.A -= Registers.E;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x94:
                    Registers.A -= Registers.H;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x95:
                    Registers.A -= Registers.L;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x96:
                    Registers.A -= _ram.LoadU8Bits(Registers.GetHL());
                    //set flag
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x97:
                    Registers.A -= Registers.A;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x98:
                    Registers.A -= (byte)(Registers.B - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x99:
                    Registers.A -= (byte)(Registers.C - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x9A:
                    Registers.A -= (byte)(Registers.D - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x9B:
                    Registers.A -= (byte)(Registers.E - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x9C:
                    Registers.A -= (byte)(Registers.H - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x9D:
                    Registers.A -= (byte)(Registers.L - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0x9E:
                    Registers.A -= (byte)(_ram.LoadU8Bits(Registers.GetHL()) - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x9F:
                    Registers.A -= (byte)(Registers.A - Registers.GetCYFlag());
                    // set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA0:
                    Registers.A &= Registers.B;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA1:
                    Registers.A &= Registers.C;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA2:
                    Registers.A &= Registers.D;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA3:
                    Registers.A &= Registers.E;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA4:
                    Registers.A &= Registers.H;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA5:
                    Registers.A &= Registers.L;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA6:
                    Registers.A &= _ram.LoadU8Bits(Registers.GetHL());
                    //set flag
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0xA7:
                    Registers.A &= Registers.A;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA8:
                    Registers.A ^= Registers.B;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xA9:
                    Registers.A ^= Registers.C;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xAA:
                    Registers.A ^= Registers.D;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xAB:
                    Registers.A ^= Registers.E;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xAC:
                    Registers.A ^= Registers.H;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xAD:
                    Registers.A ^= Registers.L;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xAE:
                    Registers.A ^= _ram.LoadU8Bits(Registers.GetHL());
                    //set flag
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0xAF:
                    Registers.A ^= Registers.A;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB0:
                    Registers.A |= Registers.B;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB1:
                    Registers.A |= Registers.C;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB2:
                    Registers.A |= Registers.D;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB3:
                    Registers.A |= Registers.E;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB4:
                    Registers.A |= Registers.H;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB5:
                    Registers.A |= Registers.L;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB6:
                    Registers.A |= _ram.LoadU8Bits(Registers.GetHL());
                    //set flag
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0xB7:
                    Registers.A |= Registers.A;
                    //set flag
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB8:
                    Registers.SetZFLag(Registers.A == Registers.B);
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < Registers.B);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xB9:
                    Registers.SetZFLag(Registers.A == Registers.C);
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < Registers.C);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xBA:
                    Registers.SetZFLag(Registers.A == Registers.D);
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < Registers.D);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xBB:
                    Registers.SetZFLag(Registers.A == Registers.E);
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < Registers.E);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xBC:
                    Registers.SetZFLag(Registers.A == Registers.H);
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < Registers.H);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xBD:
                    Registers.SetZFLag(Registers.A == Registers.L);
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < Registers.L);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xBE:
                    Registers.SetZFLag(Registers.A == _ram.LoadU8Bits(Registers.GetHL()));
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < _ram.LoadU8Bits(Registers.GetHL()));
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0xBF:
                    Registers.SetZFLag(Registers.A == Registers.A);
                    Registers.SetNFLag(true);
                    // hcy set
                    Registers.SetCYFLag(Registers.A < Registers.A);
                    UpdatePCAndCycles(1, 4);
                    break;
                case 0xC0:
                    if (Registers.GetZFlag() == 0)
                    {
                        PC = Pop();
                        UpdatePCAndCycles(0, 20);
                    }
                    else
                    {
                        UpdatePCAndCycles(1, 8);
                    }
                    break;
                case 0xC1:
                    Registers.SetBC(Pop());
                    UpdatePCAndCycles(1, 12);
                    break;

            }
        }
    }
}
