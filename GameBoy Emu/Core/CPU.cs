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
        }

        public void ProcessOpcode()
        {
            Opcode = _ram.Memory[PC];
            switch (Opcode)
            {
                case 0x00:
                    CpuCycles += 4;
                    PC++;
                    break;
                case 0x01:
                    Registers.SetBC(_ram.LoadU16Bits(PC + 1));
                    PC += 3;
                    CpuCycles += 12;
                    break;
                case 0x02:
                    _ram.StoreU8Bits(Registers.GetBC(), Registers.A);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x03:
                    Registers.AddToBC(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x04:
                    Registers.B++;
                    //SET FLAGS
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x05:
                    Registers.B--;
                    //SET FLAGS
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x06:
                    Registers.B = _ram.LoadU8Bits(PC + 1);
                    PC += 2;
                    CpuCycles += 8;
                    break;
                case 0x07:
                // RLCA
                case 0x08:
                    ushort storeAddr = _ram.LoadU16Bits(PC + 1);
                    _ram.StoreU16Bits(storeAddr, SP);
                    PC += 3;
                    CpuCycles += 20;
                    break;
                case 0x09:
                    Registers.AddToHL(Registers.GetBC());
                    //SET FLAGS
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x0A:
                    Registers.A = _ram.LoadU8Bits(Registers.GetBC());
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x0B:
                    Registers.SubToBC(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x0C:
                    Registers.C++;
                    // SET FLAGS
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x0D:
                    Registers.C--;
                    //SET FLAGS
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x0E:
                    Registers.C = _ram.LoadU8Bits(PC + 1);
                    PC += 2;
                    CpuCycles += 8;
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
                    PC += 3;
                    CpuCycles += 12;
                    break;
                case 0x12:
                    _ram.StoreU8Bits(Registers.GetDE(), Registers.A);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x13:
                    Registers.AddToDE(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x14:
                    Registers.D++;
                    //Set Flags
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x15:
                    Registers.D--;
                    // SET FLAGS
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x16:
                    Registers.D = _ram.LoadU8Bits(PC + 1);
                    PC += 2;
                    CpuCycles += 8;
                    break;
                case 0x17:
                    var oldBit7 = Registers.A & 0x80; // CY flag
                    // SET FLAGS
                    Registers.A <<= 1;
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x18:
                    PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                    PC += 2;
                    CpuCycles += 12;
                    break;
                case 0x19:
                    Registers.AddToHL(Registers.GetDE());
                    // SET FLAGS
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x1A:
                    Registers.A = _ram.LoadU8Bits(Registers.GetDE());
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x1B:
                    Registers.SubToDE(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x1C:
                    Registers.E++;
                    //Set Flags
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x1D:
                    Registers.E--;
                    //Set flags
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x1E:
                    Registers.E = _ram.LoadU8Bits(PC + 1);
                    PC += 2;
                    CpuCycles += 8;
                    break;
                case 0x1F:
                    var oldBit0 = Registers.A & 1;
                    // set CY flag
                    Registers.A >>= 1;
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x20:
                    // JR NZ,i8
                    if (Registers.GetZFlag() == 0)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        CpuCycles += 12;
                    }
                    else
                    {
                        PC += 2;
                        CpuCycles += 8;
                    }
                    
                    break;
                case 0x21:
                    Registers.SetHL(_ram.LoadU16Bits(PC + 1));
                    PC += 3;
                    CpuCycles += 12;
                    break;
                case 0x22:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.A);
                    Registers.AddToHL(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x23:
                    Registers.AddToHL(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x24:
                    Registers.H++;
                    // set flags
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x25:
                    Registers.H--;
                    // set flags
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x26:
                    Registers.H = _ram.LoadU8Bits(PC + 1);
                    PC += 2;
                    CpuCycles += 8;
                    break;
                case 0x27:
                    //DAA
                    //set flags
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x28:
                    //JR Z,i8 
                    if (Registers.GetZFlag() == 1)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        CpuCycles += 12;
                    }
                    else
                    {
                        PC += 2;
                        CpuCycles += 8;
                    }
                    break;
                case 0X29:
                    Registers.AddToHL(Registers.GetHL());
                    //Set flags
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x2A:
                    Registers.A = _ram.LoadU8Bits(Registers.GetHL());
                    Registers.AddToHL(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x2B:
                    Registers.SubToHL(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x2C:
                    Registers.L++;
                    //set flag
                    PC += 1;
                    CpuCycles += 4;
                    break;
                case 0x2D:
                    Registers.L--;
                    //set flag
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x2E:
                    Registers.L = _ram.LoadU8Bits(PC + 1);
                    PC += 2;
                    CpuCycles += 8;
                    break;
                case 0x2F:
                    Registers.A ^= 0xFF;
                    //set flag 
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x30:
                    if (Registers.GetCYFlag() == 0)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        CpuCycles += 12;
                    } 
                    else
                    {
                        PC += 2;
                        CpuCycles += 8;
                    }
                    break;
                case 0x31:
                    SP = _ram.LoadU16Bits(PC + 1);
                    PC += 3;
                    CpuCycles += 12;
                    break;
                case 0x32:
                    _ram.StoreU8Bits(Registers.GetHL(), Registers.A);
                    Registers.SubToHL(1);
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x33:
                    SP++;
                    PC++;
                    CpuCycles += 8;
                    break;
                case 0x34:
                    Registers.AddToHL(1);
                    //set flag
                    PC++;
                    CpuCycles += 12;
                    break;
                case 0x35:
                    Registers.SubToHL(1);
                    //set flag
                    PC++;
                    CpuCycles += 12;
                    break;
                case 0x36:
                    _ram.StoreU8Bits(Registers.GetHL(), _ram.LoadU8Bits(PC + 1));
                    PC += 2;
                    CpuCycles += 12;
                    break;
                case 0x37:
                    Registers.SetCYFLag(1);
                    Registers.SetNFLag(0);
                    Registers.SetHCYFLag(0);
                    PC++;
                    CpuCycles += 4;
                    break;
                case 0x38:
                    if (Registers.GetCYFlag() == 1)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        CpuCycles += 12;
                    }
                    else
                    {
                        PC += 2;
                        CpuCycles += 8;
                    }
                    break;
                case 0x39:

                case 0x7F:
                    Registers.A = Registers.A;
                    CpuCycles += 4;
                    break;
                case 0x78:
                    Registers.A = Registers.B;
                    CpuCycles += 4;
                    break;
                case 0x79:
                    Registers.A = Registers.C;
                    CpuCycles += 4;
                    break;
                case 0x7A:
                    Registers.A = Registers.D;
                    CpuCycles += 4;
                    break;
                case 0x7B:
                    Registers.A = Registers.E;
                    CpuCycles += 4;
                    break;
                case 0x7C:
                    Registers.A = Registers.H;
                    CpuCycles += 4;
                    break;
                case 0x7D:
                    Registers.A = Registers.L;
                    CpuCycles += 4;
                    break;
                case 0x7E:
                    Registers.A = _ram.LoadU8Bits(Registers.GetHL());
                    CpuCycles += 8;
                    break;
                case 0x40:
                    Registers.B = Registers.B;
                    CpuCycles += 4;
                    break;
                case 0x41:
                    Registers.B = Registers.C;
                    CpuCycles += 4;
                    break;
                case 0x42:
                    Registers.B = Registers.D;
                    CpuCycles += 4;
                    break;
                case 0x43:
                    Registers.B = Registers.E;
                    CpuCycles += 4;
                    break;
                case 0x44:
                    Registers.B = Registers.H;
                    CpuCycles += 4;
                    break;
                case 0x45:
                    Registers.B = Registers.L;
                    CpuCycles += 4;
                    break;
                case 0x46:
                    Registers.B = _ram.LoadU8Bits(Registers.GetHL());
                    CpuCycles += 8;
                    break;
            }
        }
    }
}
