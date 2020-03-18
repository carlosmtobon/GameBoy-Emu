using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.Core
{
    public class CPU
    {
        public int CpuCycles { get; set; }
        public byte Opcode { get; set; }
        public ushort PC { get; set; }
        public ushort SP { get; set; }
        internal Registers Registers { get; }
        public bool IME { get; set; } 

        public MMU _ram;

        public CPU(MMU ram)
        {
            _ram = ram;
            Registers = new Registers();
            PC = 0x100;
            SP = 0xFFFE;
            IME = false;
        }

        public void UpdatePCAndCycles(ushort val, int cycles)
        {
            PC += val;
            CpuCycles += cycles;
        }

        public void Push(ushort val)
        {
            _ram.Memory[SP - 1] = (byte)(val >> 8);
            _ram.Memory[SP - 2] = (byte)(val & 0x00FF);
            SP -= 2;
        }

        public ushort Pop()
        {
            var val = (ushort)(_ram.Memory[SP] | _ram.Memory[SP + 1] << 8);
            SP += 2;
            return val;
        }

        public void JP(ushort addr)
        {
            PC = _ram.LoadU16Bits(addr);
        }

        public byte LD(byte register, ushort bytesRead, int cycles)
        {
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }

        public byte INC(byte val, ushort bytesRead, int cycles)
        {
            byte oldVal = val;
            val++;
            Registers.SetZFLag((val == 0));
            Registers.SetNFLag(false);
            Registers.SetHCYFLag((((oldVal & 0xF) + (1 & 0xF)) > 0xF));
            UpdatePCAndCycles(bytesRead, cycles);
            return val;
        }

        public byte DEC(byte val, ushort bytesRead, int cycles)
        {
            byte oldVal = val;
            val--;
            Registers.SetZFLag((val == 0));
            Registers.SetNFLag(true);
            Registers.SetHCYFLag((oldVal & 0xF) - (1 & 0xF) < 0);
            UpdatePCAndCycles(bytesRead, cycles);
            return val;
        }

        public void AddToHL(ushort registerPair)
        {
            byte h = Registers.H;
            ushort hlVal = Registers.GetHL();
            Registers.SetNFLag(false);
            Registers.SetHCYFLag((hlVal & 0xFFF) + (registerPair & 0xFFF) > 0xFFF);
            Registers.SetCYFLag((hlVal + registerPair) > 0xFFFF);
            Registers.AddToHL(registerPair);
            UpdatePCAndCycles(1, 8);
        }

        public void ADD(byte register, ushort bytesRead, int cycles)
        {
            Registers.SetZFLag((byte)(Registers.A + register) == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(((Registers.A & 0xF) + (register & 0xF) > 0xF));
            Registers.SetCYFLag((Registers.A + register) > 0xFF);
            Registers.A += register;
            UpdatePCAndCycles(bytesRead, cycles);
        }
        public void ADDC(byte register, ushort bytesRead, int cycles)
        {
            byte currentCY = Registers.GetCYFlag();
            Registers.SetZFLag((byte)(Registers.A + register + currentCY) == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(((Registers.A & 0xF) + (register & 0xF) + currentCY) > 0xF);
            Registers.SetCYFLag((Registers.A + register + currentCY) > 0xFF);
            Registers.A += (byte)(register + currentCY);
            UpdatePCAndCycles(bytesRead, cycles);
        }

        public void SUB(byte register, ushort bytesRead, int cycles)
        {
            Registers.SetZFLag((Registers.A - register) == 0);
            Registers.SetNFLag(true);
            Registers.SetHCYFLag((Registers.A & 0xF) < (register & 0xF));
            Registers.SetCYFLag(Registers.A < register);
            Registers.A -= (byte)(register);
            UpdatePCAndCycles(bytesRead, cycles);
        }
        public void SBC(byte register, ushort bytesRead, int cycles)
        {
            byte currentCY = Registers.GetCYFlag();
            Registers.SetZFLag((byte)(Registers.A - register - currentCY) == 0);
            Registers.SetNFLag(true);
            Registers.SetHCYFLag(((Registers.A & 0xF) - (register & 0xF) - currentCY) < 0);
            Registers.SetCYFLag((Registers.A - register - currentCY) < 0);
            Registers.A = (byte)(Registers.A - register - currentCY);
            UpdatePCAndCycles(bytesRead, cycles);
        }

        public void AND(byte register, ushort bytesRead, int cycles)
        {
            Registers.A &= register;
            Registers.SetZFLag(Registers.A == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(true);
            Registers.SetCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
        }

        public void XOR(byte register, ushort bytesRead, int cycles)
        {
            Registers.A ^= register;
            Registers.SetZFLag(Registers.A == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            Registers.SetCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
        }

        public void OR(byte register, ushort bytesRead, int cycles)
        {
            Registers.A |= register;
            Registers.SetZFLag(Registers.A == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            Registers.SetCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
        }

        public void CP(byte register, ushort bytesRead, int cycles)
        {
            Registers.SetZFLag((Registers.A - register) == 0);
            Registers.SetNFLag(true);
            Registers.SetHCYFLag((Registers.A & 0xF) < (register & 0xF));
            Registers.SetCYFLag(Registers.A < register);
            UpdatePCAndCycles(bytesRead, cycles);
        }

        public void DAA()
        {
            if (Registers.GetNFlag() == 0)
            {  // after an addition, adjust if (half-)carry occurred or if result is out of bounds
                if (Registers.GetCYFlag() == 1 || Registers.A > 0x99) 
                {
                    Registers.A += 0x60; 
                    Registers.SetCYFLag(true); 
                }
                if (Registers.GetHCYFlag() == 1 || (Registers.A & 0x0f) > 0x09) 
                { 
                    Registers.A += 0x6; 
                }
            }
            else
            {  // after a subtraction, only adjust if (half-)carry occurred
                if (Registers.GetCYFlag() == 1) 
                { 
                    Registers.A -= 0x60; 
                }
                if (Registers.GetHCYFlag() == 1) 
                { 
                    Registers.A -= 0x6;
                }
            }
            // these flags are always updated
            Registers.SetZFLag(Registers.A == 0); // the usual z flag
            Registers.SetHCYFLag(false); // h flag is always cleared
            UpdatePCAndCycles(1, 4);
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
                    Registers.B = INC(Registers.B, 1, 4);
                    break;
                case 0x05:
                    Registers.B = DEC(Registers.B, 1, 4);
                    break;
                case 0x06:
                    Registers.B = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x07:
                    RLCA();
                    break;
                case 0x08:
                    ushort storeAddr = _ram.LoadU16Bits(PC + 1);
                    _ram.StoreU16Bits(storeAddr, SP);
                    UpdatePCAndCycles(3, 20);
                    break;
                case 0x09:
                    AddToHL(Registers.GetBC());
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
                    Registers.C = INC(Registers.C, 1, 4);
                    break;
                case 0x0D:
                    Registers.C = DEC(Registers.C, 1, 4);
                    break;
                case 0x0E:
                    Registers.C = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x0F:
                    RRCA();
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
                    Registers.D = INC(Registers.D, 1, 4);
                    break;
                case 0x15:
                    Registers.D = DEC(Registers.D, 1, 4);
                    break;
                case 0x16:
                    Registers.D = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x17:
                    RLA();
                    break;
                case 0x18:
                    PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                    UpdatePCAndCycles(2, 12);
                    break;
                case 0x19:
                    AddToHL(Registers.GetDE());
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
                    Registers.E = INC(Registers.E, 1, 4); 
                    break;
                case 0x1D:
                    Registers.E = DEC(Registers.E, 1, 4);
                    break;
                case 0x1E:
                    Registers.E = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x1F:
                    RRA();
                    break;
                case 0x20:
                    // JR NZ,i8
                    if (Registers.GetZFlag() == 0)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        UpdatePCAndCycles(2, 12);
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
                    Registers.H = INC(Registers.H, 1, 4); 
                    break;
                case 0x25:
                    Registers.H = DEC(Registers.H, 1, 4); 
                    break;
                case 0x26:
                    Registers.H = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x27:
                    DAA();
                    break;
                case 0x28:
                    //JR Z,i8 
                    if (Registers.GetZFlag() == 1)
                    {
                        PC += (ushort)(_ram.LoadI8Bits(PC + 1));
                        UpdatePCAndCycles(2, 12);
                    }
                    else
                    {
                        UpdatePCAndCycles(2, 8);
                    }
                    break;
                case 0X29:
                    AddToHL(Registers.GetHL());
                    break;
                case 0x2A:
                    LDAHLINC();
                    break;
                case 0x2B:
                    Registers.SubToHL(1);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x2C:
                    Registers.L = INC(Registers.L, 1, 4);
                    break;
                case 0x2D:
                    Registers.L = DEC(Registers.L, 1, 4);
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
                        UpdatePCAndCycles(2, 12);
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
                    _ram.StoreU8Bits(Registers.GetHL(), INC(_ram.LoadU8Bits(Registers.GetHL()), 1, 12));
                    break;
                case 0x35:
                    _ram.StoreU8Bits(Registers.GetHL(), 
                        DEC(_ram.LoadU8Bits(Registers.GetHL()), 1, 12));
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
                        UpdatePCAndCycles(2, 12);
                    }
                    else
                    {
                        UpdatePCAndCycles(2, 8);
                    }
                    break;
                case 0x39:
                    AddToHL(SP);
                    break;
                case 0x3A:
                    LDAHLDEC();
                    break;
                case 0x3B:
                    SP--;
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0x3C:
                    Registers.A = INC(Registers.A, 1, 4);
                    break;
                case 0x3D:
                    Registers.A = DEC(Registers.A, 1, 4);
                    break;
                case 0x3E:
                    Registers.A = _ram.LoadU8Bits(PC + 1);
                    UpdatePCAndCycles(2, 8);
                    break;
                case 0x3F:
                    CCF();
                    break;
                case 0x40:
                    Registers.B = LD(Registers.B, 1, 4);
                    break;
                case 0x41:
                    Registers.B = LD(Registers.C, 1, 4);
                    break;
                case 0x42:
                    Registers.B = LD(Registers.D, 1, 4);
                    break;
                case 0x43:
                    Registers.B = LD(Registers.E, 1, 4);
                    break;
                case 0x44:
                    Registers.B = LD(Registers.H, 1, 4);
                    break;
                case 0x45:
                    Registers.B = LD(Registers.L, 1, 4);
                    break;
                case 0x46:
                    Registers.B = LD(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x47:
                    Registers.B = LD(Registers.A, 1, 4);
                    break;
                case 0x48:
                    Registers.C = LD(Registers.B, 1, 4);
                    break;
                case 0x49:
                    Registers.C = LD(Registers.C, 1, 4);
                    break;
                case 0x4A:
                    Registers.C = LD(Registers.D, 1, 4);
                    break;
                case 0x4B:
                    Registers.C = LD(Registers.E, 1, 4);
                    break;
                case 0x4C:
                    Registers.C = LD(Registers.H, 1, 4);
                    break;
                case 0x4D:
                    Registers.C = LD(Registers.L, 1, 4);
                    break;
                case 0x4E:
                    Registers.C = LD(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x4F:
                    Registers.C = LD(Registers.A, 1, 4);
                    break;
                case 0x50:
                    Registers.D = LD(Registers.B, 1, 4);
                    break;
                case 0x51:
                    Registers.D = LD(Registers.C, 1, 4);
                    break;
                case 0x52:
                    Registers.D = LD(Registers.D, 1, 4);
                    break;
                case 0x53:
                    Registers.D = LD(Registers.E, 1, 4);
                    break;
                case 0x54:
                    Registers.D = LD(Registers.H, 1, 4);
                    break;
                case 0x55:
                    Registers.D = LD(Registers.L, 1, 4);
                    break;
                case 0x56:
                    Registers.D = LD(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x57:
                    Registers.D = LD(Registers.A, 1, 4);
                    break;
                case 0x58:
                    Registers.E = LD(Registers.B, 1, 4);
                    break;
                case 0x59:
                    Registers.E = LD(Registers.C, 1, 4);
                    break;
                case 0x5A:
                    Registers.E = LD(Registers.D, 1, 4);
                    break;
                case 0x5B:
                    Registers.E = LD(Registers.E, 1, 4);
                    break;
                case 0x5C:
                    Registers.E = LD(Registers.H, 1, 4);
                    break;
                case 0x5D:
                    Registers.E = LD(Registers.L, 1, 4);
                    break;
                case 0x5E:
                    Registers.E = LD(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x5F:
                    Registers.E = LD(Registers.A, 1, 4);
                    break;
                case 0x60:
                    Registers.H = LD(Registers.B, 1, 4);
                    break;
                case 0x61:
                    Registers.H = LD(Registers.C, 1, 4);
                    break;
                case 0x62:
                    Registers.H = LD(Registers.D, 1, 4);
                    break;
                case 0x63:
                    Registers.H = LD(Registers.E, 1, 4);
                    break;
                case 0x64:
                    Registers.H = LD(Registers.H, 1, 4);
                    break;
                case 0x65:
                    Registers.H = LD(Registers.L, 1, 4);
                    break;
                case 0x66:
                    Registers.H = LD(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x67:
                    Registers.H = LD(Registers.A, 1, 4);
                    break;
                case 0x68:
                    Registers.L = LD(Registers.B, 1, 4);
                    break;
                case 0x69:
                    Registers.L = LD(Registers.C, 1, 4);
                    break;
                case 0x6A:
                    Registers.L = LD(Registers.D, 1, 4);
                    break;
                case 0x6B:
                    Registers.L = LD(Registers.E, 1, 4);
                    break;
                case 0x6C:
                    Registers.L = LD(Registers.H, 1, 4);
                    break;
                case 0x6D:
                    Registers.L = LD(Registers.L, 1, 4);
                    break;
                case 0x6E:
                    Registers.L = LD(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x6F:
                    Registers.L = LD(Registers.A, 1, 4);
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
                    ADD(Registers.B, 1, 4);
                    break;
                case 0x81:
                    ADD(Registers.C, 1, 4);
                    break;
                case 0x82:
                    ADD(Registers.D, 1, 4);
                    break;
                case 0x83:
                    ADD(Registers.E, 1, 4);
                    break;
                case 0x84:
                    ADD(Registers.H, 1, 4);
                    break;
                case 0x85:
                    ADD(Registers.L, 1, 4);
                    break;
                case 0x86:
                    ADD(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x87:
                    ADD(Registers.A, 1, 4);
                    break;
                case 0x88:
                    ADDC(Registers.B, 1, 4);
                    break;
                case 0x89:
                    ADDC(Registers.C, 1, 4);
                    break;
                case 0x8A:
                    ADDC(Registers.D, 1, 4);
                    break;
                case 0x8B:
                    ADDC(Registers.E, 1, 4);
                    break;
                case 0x8C:
                    ADDC(Registers.H, 1, 4);
                    break;
                case 0x8D:
                    ADDC(Registers.L, 1, 4);
                    break;
                case 0x8E:
                    ADDC(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x8F:
                    ADDC(Registers.A, 1, 4);
                    break;
                case 0x90:
                    SUB(Registers.B, 1, 4);
                    break;
                case 0x91:
                    SUB(Registers.C, 1, 4);
                    break;
                case 0x92:
                    SUB(Registers.D, 1, 4);
                    break;
                case 0x93:
                    SUB(Registers.E, 1, 4);
                    break;
                case 0x94:
                    SUB(Registers.H, 1, 4);
                    break;
                case 0x95:
                    SUB(Registers.L, 1, 4);
                    break;
                case 0x96:
                    SUB(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x97:
                    SUB(Registers.A, 1, 4);
                    break;
                case 0x98:
                    SBC(Registers.B, 1, 4);
                    break;
                case 0x99:
                    SBC(Registers.C, 1, 4);
                    break;
                case 0x9A:
                    SBC(Registers.D, 1, 4);
                    break;
                case 0x9B:
                    SBC(Registers.E, 1, 4);
                    break;
                case 0x9C:
                    SBC(Registers.H, 1, 4);
                    break;
                case 0x9D:
                    SBC(Registers.L, 1, 4);
                    break;
                case 0x9E:
                    SBC(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0x9F:
                    SBC(Registers.A, 1, 4);
                    break;
                case 0xA0:
                    AND(Registers.B, 1, 4);
                    break;
                case 0xA1:
                    AND(Registers.C, 1, 4);
                    break;
                case 0xA2:
                    AND(Registers.D, 1, 4);
                    break;
                case 0xA3:
                    AND(Registers.E, 1, 4);
                    break;
                case 0xA4:
                    AND(Registers.H, 1, 4);
                    break;
                case 0xA5:
                    AND(Registers.L, 1, 4);
                    break;
                case 0xA6:
                    AND(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0xA7:
                    AND(Registers.A, 1, 4);
                    break;
                case 0xA8:
                    XOR(Registers.B, 1, 4);
                    break;
                case 0xA9:
                    XOR(Registers.C, 1, 4);
                    break;
                case 0xAA:
                    XOR(Registers.D, 1, 4);
                    break;
                case 0xAB:
                    XOR(Registers.E, 1, 4);
                    break;
                case 0xAC:
                    XOR(Registers.H, 1, 4);
                    break;
                case 0xAD:
                    XOR(Registers.L, 1, 4);
                    break;
                case 0xAE:
                    XOR(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0xAF:
                    XOR(Registers.A, 1, 4);
                    break;
                case 0xB0:
                    OR(Registers.B, 1, 4);
                    break;
                case 0xB1:
                    OR(Registers.C, 1, 4);
                    break;
                case 0xB2:
                    OR(Registers.D, 1, 4);
                    break;
                case 0xB3:
                    OR(Registers.E, 1, 4);
                    break;
                case 0xB4:
                    OR(Registers.H, 1, 4);
                    break;
                case 0xB5:
                    OR(Registers.L, 1, 4);
                    break;
                case 0xB6:
                    OR(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0xB7:
                    OR(Registers.A, 1, 4);
                    break;
                case 0xB8:
                    CP(Registers.B, 1, 4);
                    break;
                case 0xB9:
                    CP(Registers.C, 1, 4);
                    break;
                case 0xBA:
                    CP(Registers.D, 1, 4);
                    break;
                case 0xBB:
                    CP(Registers.E, 1, 4);
                    break;
                case 0xBC:
                    CP(Registers.H, 1, 4);
                    break;
                case 0xBD:
                    CP(Registers.L, 1, 4);
                    break;
                case 0xBE:
                    CP(_ram.LoadU8Bits(Registers.GetHL()), 1, 8);
                    break;
                case 0xBF:
                    CP(Registers.A, 1, 4);
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
                case 0xC2:
                    if (Registers.GetZFlag() == 0)
                    {
                        JP((ushort)(PC + 1));
                        UpdatePCAndCycles(0, 16);
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }  
                    break;
                case 0xC3:
                    JP((ushort)(PC + 1));
                    UpdatePCAndCycles(0, 16);
                    break;
                case 0xC4:
                    if (Registers.GetZFlag() == 0)
                    {
                        CALL();
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }
                    break;
                case 0xC5:
                    Push(Registers.GetBC());
                    UpdatePCAndCycles(1, 16);
                    break;
                case 0xC6:
                    ADD(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xC7:
                    RST(0x0);
                    break;
                case 0xC8:
                    if (Registers.GetZFlag() == 1)
                    {
                        PC = Pop();
                        UpdatePCAndCycles(0, 20);
                    }
                    else
                    {
                        UpdatePCAndCycles(1, 8);
                    }
                    break;
                case 0xC9:
                    PC = Pop();
                    UpdatePCAndCycles(0, 16);
                    break;
                case 0xCA:
                    if (Registers.GetZFlag() == 1)
                    {
                        JP((ushort)(PC + 1));
                        UpdatePCAndCycles(0, 16);
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }
                    break;
                case 0xCB:
                    // Prefix CB
                    CBPrefix(_ram.Memory[PC + 1]);
                    break;
                case 0xCC:
                    if (Registers.GetZFlag() == 1)
                    {
                        CALL();
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }
                    break;
                case 0xCD:
                    CALL();
                    break;
                case 0xCE:
                    ADDC(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xCF:
                    RST(0x8);
                    break;
                case 0xD0:
                    if (Registers.GetCYFlag() == 0)
                    {
                        PC = Pop();
                        UpdatePCAndCycles(0, 20);
                    }
                    else
                    {
                        UpdatePCAndCycles(1, 8);
                    }
                    break;
                case 0xD1:
                    Registers.SetDE(Pop());
                    UpdatePCAndCycles(1, 12);
                    break;
                case 0xD2:
                    if (Registers.GetCYFlag() == 0)
                    {
                        JP((ushort)(PC + 1));
                        UpdatePCAndCycles(0, 16);
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }
                    break;
                case 0xD4:
                    if (Registers.GetCYFlag() == 0)
                    {
                        CALL();
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }
                    break;
                case 0xD5:
                    Push(Registers.GetDE());
                    UpdatePCAndCycles(1, 16);
                    break;
                case 0xD6:
                    SUB(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xD7:
                    RST(0x10);
                    break;
                case 0xD8:
                    if (Registers.GetCYFlag() == 1)
                    {
                        PC = Pop();
                        UpdatePCAndCycles(0, 20);
                    }
                    else
                    {
                        UpdatePCAndCycles(1, 8);
                    }
                    break;
                case 0xD9:
                    // RETI
                    PC = Pop();
                    IME = false;
                    UpdatePCAndCycles(0, 16);
                    break;
                case 0xDA:
                    if (Registers.GetCYFlag() == 1)
                    {
                        JP((ushort)(PC + 1));
                        UpdatePCAndCycles(0, 16);
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }
                    break;
                case 0xDC:
                    if (Registers.GetCYFlag() == 1)
                    {
                        CALL();
                    }
                    else
                    {
                        UpdatePCAndCycles(3, 12);
                    }
                    break;
                case 0xDE:
                    SBC(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xDF:
                    RST(0x18);
                    break;
                case 0xE0:
                    _ram.StoreU8Bits(0xFF00 + _ram.LoadU8Bits(PC + 1), Registers.A);
                    UpdatePCAndCycles(2, 12);
                    break;
                case 0xE1:
                    Registers.SetHL(Pop());
                    UpdatePCAndCycles(1, 12);
                    break;
                case 0xE2:
                    _ram.StoreU8Bits(0xFF00 + Registers.C, Registers.A);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0xE5:
                    Push(Registers.GetHL());
                    UpdatePCAndCycles(1, 16);
                    break;
                case 0xE6:
                    AND(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xE7:
                    RST(0x20);
                    break;
                case 0xE8:
                    ADDSPI8();
                    break;
                case 0xE9:
                    PC = Registers.GetHL();
                    UpdatePCAndCycles(0, 4);
                    break;
                case 0xEA:
                    _ram.StoreU8Bits(_ram.LoadU16Bits(PC + 1), Registers.A);
                    UpdatePCAndCycles(3, 16);
                    break;
                case 0xEE:
                    XOR(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xEF:
                    RST(0x28);
                    break;
                case 0xF0:
                    Registers.A = _ram.LoadU8Bits(0xFF00 + _ram.LoadU8Bits(PC + 1));
                    UpdatePCAndCycles(2, 12);
                    break;
                case 0xF1:
                    var af = Pop();
                    Registers.SetAF((ushort)(af & 0xFFF0));
                    UpdatePCAndCycles(1, 12);
                    break;
                case 0xF2:
                    Registers.A = _ram.LoadU8Bits(0xFF00 + Registers.C);
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0xF3:
                    // disable interrupt
                    DI();
                    break;
                case 0xF5:
                    Push(Registers.GetAF());
                    UpdatePCAndCycles(1, 16);
                    break;
                case 0xF6:
                    OR(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xF7:
                    RST(0x30);
                    break;
                case 0xF8:
                    LDHLSPI8();
                    break;
                case 0xF9:
                    SP = Registers.GetHL();
                    UpdatePCAndCycles(1, 8);
                    break;
                case 0xFA:
                    Registers.A = _ram.LoadU8Bits(_ram.LoadU16Bits(PC + 1));
                    UpdatePCAndCycles(3, 16);
                    break;
                case 0xFB:
                    EI();
                    break;
                case 0xFE:
                    CP(_ram.LoadU8Bits(PC + 1), 2, 8);
                    break;
                case 0xFF:
                    RST(0x38);
                    break;
            }

            CheckForInterrupts();
        }

        private void RST(ushort addr)
        {
            Push((ushort)(PC + 1));
            PC = addr;
            UpdatePCAndCycles(1, 16);
        }

        public void LDAHLINC()
        {
            byte hlContents = _ram.LoadU8Bits(Registers.GetHL());
            Registers.A = hlContents;
            Registers.AddToHL(1);
            UpdatePCAndCycles(1, 8);
        }

        public void LDAHLDEC()
        {
            byte hlContents = _ram.LoadU8Bits(Registers.GetHL());
            Registers.A = hlContents;
            Registers.SubToHL(1);
            UpdatePCAndCycles(1, 8);
        }

        public void CALL()
        {
            var currentPC = PC;
            UpdatePCAndCycles(3, 24);
            Push(PC);
            JP((ushort)(currentPC + 1));
        }

        public void ADDSPI8()
        {
            sbyte nextI8 = _ram.LoadI8Bits(PC + 1);
            
            Registers.SetZFLag(false);
            Registers.SetNFLag(false);
            ushort lSP = (ushort)(SP + nextI8);

            //Registers.SetCYFLag((((SP & 0xFF) + (nextI8 & 0xFF)) > 0xFF));
            //Registers.SetHCYFLag((((SP & 0xF) + (nextI8 & 0xF)) > 0xF));

            if (nextI8 >= 0)
            {
                Registers.SetHCYFLag((SP & 0xF) + (nextI8 & 0xF) > 0xF);
                Registers.SetCYFLag(((SP & 0xFF) + nextI8) > 0xFF);
            }
            else
            {
                Registers.SetHCYFLag((lSP & 0xF) <= (SP & 0xF));
                Registers.SetCYFLag(((lSP & 0xFF) <= (SP & 0xFF)));
            }
            SP += (ushort)nextI8;
            UpdatePCAndCycles(2, 16);
        }

        public void LDHLSPI8()
        {
            sbyte nextI8 = _ram.LoadI8Bits(PC + 1);

            Registers.SetZFLag(false);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag((SP & 0xFFF) + (nextI8 & 0xFFF) > 0xFFF);
            Registers.SetCYFLag((SP + nextI8) > 0xFFFF);
            SP += (ushort)nextI8;
            Registers.SetHL(SP);
            UpdatePCAndCycles(2, 12);
        }

        public void CCF()
        {
            Registers.SetCYFLag((!(Registers.GetCYFlag() == 1)));
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(1, 4);
        }

        public void CheckForInterrupts()
        {
            if (IME)
            {
                Console.WriteLine(_ram.GetIE());
                Console.WriteLine(_ram.GetIF());
            }
        }

        public void DI()
        {
            IME = false;
            UpdatePCAndCycles(1, 4);
        }
        public void EI()
        {
            IME = true;
            UpdatePCAndCycles(1, 4);
        }

        public byte RLC(byte register, ushort bytesRead, int cycles)
        {
            byte oldbit7 = (byte)(register >> 7);
            Registers.SetCYFLag(oldbit7 == 1);
            register = (byte)(register << 1);
            register |= (byte)(oldbit7);
            Registers.SetZFLag(register == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }
        public void RLCA()
        {
            byte oldbit7 = (byte)(Registers.A >> 7);
            Registers.SetCYFLag(oldbit7 == 1);
            Registers.A = (byte)(Registers.A << 1);
            Registers.A |= (byte)(oldbit7);
            Registers.SetZFLag(false);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(1, 4);
        }

        public byte RL(byte register, ushort bytesRead, int cycles)
        {
            var setCarry = (register >> 7) == 1;
            register = (byte)((register << 1) | Registers.GetCYFlag());
            Registers.SetCYFLag(setCarry);
            Registers.SetZFLag(register == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }
        public void RLA()
        {
            var setCarry = (Registers.A >> 7) == 1;
            Registers.A = (byte)((Registers.A << 1) | Registers.GetCYFlag());
            Registers.SetCYFLag(setCarry);
            Registers.SetZFLag(false);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(1, 4);
        }
        public byte SLA(byte register, ushort bytesRead, int cycles)
        {
            Registers.SetCYFLag((register >> 7) == 1);
            register = (byte)(register << 1);
            Registers.SetZFLag(register == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }

        public byte RRC(byte register, ushort bytesRead, int cycles)
        {
            Registers.SetCYFLag((register & 1) == 1);
            register = (byte)(register >> 1);
            register |= (byte)(Registers.GetCYFlag() << 7);
            Registers.SetZFLag(register == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }
        public void RRCA()
        {
            Registers.SetCYFLag((Registers.A & 1) == 1);
            Registers.A = (byte)(Registers.A >> 1);
            Registers.A |= (byte)(Registers.GetCYFlag() << 7);
            Registers.SetZFLag(false);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(1, 4);
        }

        public byte RR(byte register, ushort bytesRead, int cycles)
        {
            var setCarry = (register & 1) == 1;
            register = (byte)((register >> 1) | Registers.GetCYFlag() << 7);
            Registers.SetCYFLag(setCarry);
            Registers.SetZFLag(register == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }

        public void RRA()
        {
            var setCarry = (Registers.A & 1) == 1;
            Registers.A = (byte)((Registers.A >> 1) | Registers.GetCYFlag() << 7);
            Registers.SetCYFLag(setCarry);
            Registers.SetZFLag(false);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(1, 4);
        }

        public byte SRA(byte register, ushort bytesRead, int cycles)
        {
            byte bit7 = (byte)(register >> 7);
            Registers.SetCYFLag((register & 1) == 1);
            register = (byte)((register >> 1) | bit7 << 7);
            Registers.SetZFLag(register == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }

        public byte SRL(byte register, ushort bytesRead, int cycles)
        {
            Registers.SetCYFLag((register & 1) == 1);
            register = (byte)(register >> 1);
            Registers.SetZFLag(register == 0);
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return register;
        }

        public byte Swap(byte val, ushort bytesRead, int cycles)
        {
            int lowNibble = val & 0x0F;
            int highNibble = val & 0xF0;
            byte swappedByte = (byte)(lowNibble << 4 | highNibble >> 4);
            Registers.SetZFLag((swappedByte == 0));
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(false);
            Registers.SetCYFLag(false);
            UpdatePCAndCycles(bytesRead, cycles);
            return swappedByte;
        }

        public void Bit(byte val, int bitPosition, ushort bytesRead, int cycles)
        {
            val = (byte)((val >> bitPosition) & 1);
            Registers.SetZFLag((val == 0));
            Registers.SetNFLag(false);
            Registers.SetHCYFLag(true);
            UpdatePCAndCycles(bytesRead, cycles);
        }

        public byte RES(byte val, byte bitToClear, ushort bytesRead, int cycles)
        {
            val = Registers.ClearBit(val, (byte)(1 << bitToClear));
            UpdatePCAndCycles(bytesRead, cycles);
            return val;
        }

        public byte SET(byte val, byte bitToSet, ushort bytesRead, int cycles)
        {
            val = Registers.SetBit(val, (byte)(1 << bitToSet));
            UpdatePCAndCycles(bytesRead, cycles);
            return val;
        }

        public void CBPrefix(byte op)
        {
            switch (op)
            {
                case 0x00:
                    Registers.B = RLC(Registers.B, 2, 8);
                    break;
                case 0x01:
                    Registers.C = RLC(Registers.C, 2, 8);
                    break;
                case 0x02:
                    Registers.D = RLC(Registers.D, 2, 8);
                    break;
                case 0x03:
                    Registers.E = RLC(Registers.E, 2, 8);
                    break;
                case 0x04:
                    Registers.H = RLC(Registers.H, 2, 8);
                    break;
                case 0x05:
                    Registers.L = RLC(Registers.L, 2, 8);
                    break;
                case 0x06:
                    _ram.StoreU8Bits(Registers.GetHL(), RLC(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x07:
                    Registers.A = RLC(Registers.A, 2, 8);
                    break;
                case 0x08:
                    Registers.B = RRC(Registers.B, 2, 8);
                    break;
                case 0x09:
                    Registers.C = RRC(Registers.C, 2, 8);
                    break;
                case 0x0A:
                    Registers.D = RRC(Registers.D, 2, 8);
                    break;
                case 0x0B:
                    Registers.E = RRC(Registers.E, 2, 8);
                    break;
                case 0x0C:
                    Registers.H = RRC(Registers.H, 2, 8);
                    break;
                case 0x0D:
                    Registers.L = RRC(Registers.L, 2, 8);
                    break;
                case 0x0E:
                    _ram.StoreU8Bits(Registers.GetHL(), RRC(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x0F:
                    Registers.A = RRC(Registers.A, 2, 8);
                    break;
                case 0x10:
                    Registers.B = RL(Registers.B, 2, 8);
                    break;
                case 0x11:
                    Registers.C = RL(Registers.C, 2, 8);
                    break;
                case 0x12:
                    Registers.D = RL(Registers.D, 2, 8);
                    break;
                case 0x13:
                    Registers.E = RL(Registers.E, 2, 8);
                    break;
                case 0x14:
                    Registers.H = RL(Registers.H, 2, 8);
                    break;
                case 0x15:
                    Registers.L = RL(Registers.L, 2, 8);
                    break;
                case 0x16:
                    _ram.StoreU8Bits(Registers.GetHL(), RL(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x17:
                    Registers.A = RL(Registers.A, 2, 8);
                    break;
                case 0x18:
                    Registers.B = RR(Registers.B, 2, 8);
                    break;
                case 0x19:
                    Registers.C = RR(Registers.C, 2, 8);
                    break;
                case 0x1A:
                    Registers.D = RR(Registers.D, 2, 8);
                    break;
                case 0x1B:
                    Registers.E = RR(Registers.E, 2, 8);
                    break;
                case 0x1C:
                    Registers.H = RR(Registers.H, 2, 8);
                    break;
                case 0x1D:
                    Registers.L = RR(Registers.L, 2, 8);
                    break;
                case 0x1E:
                    _ram.StoreU8Bits(Registers.GetHL(), RR(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x1F:
                    Registers.A = RR(Registers.A, 2, 8);
                    break;
                case 0x20:
                    Registers.B = SLA(Registers.B, 2, 8);
                    break;
                case 0x21:
                    Registers.C = SLA(Registers.C, 2, 8);
                    break;
                case 0x22:
                    Registers.D = SLA(Registers.D, 2, 8);
                    break;
                case 0x23:
                    Registers.E = SLA(Registers.E, 2, 8);
                    break;
                case 0x24:
                    Registers.H = SLA(Registers.H, 2, 8);
                    break;
                case 0x25:
                    Registers.L = SLA(Registers.L, 2, 8);
                    break;
                case 0x26:
                    _ram.StoreU8Bits(Registers.GetHL(), SLA(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x27:
                    Registers.A = SLA(Registers.A, 2, 8);
                    break;
                case 0x28:
                    Registers.B = SRA(Registers.B, 2, 8);
                    break;
                case 0x29:
                    Registers.C = SRA(Registers.C, 2, 8);
                    break;
                case 0x2A:
                    Registers.D = SRA(Registers.D, 2, 8);
                    break;
                case 0x2B:
                    Registers.E = SRA(Registers.E, 2, 8);
                    break;
                case 0x2C:
                    Registers.H = SRA(Registers.H, 2, 8);
                    break;
                case 0x2D:
                    Registers.L = SRA(Registers.L, 2, 8);
                    break;
                case 0x2E:
                    _ram.StoreU8Bits(Registers.GetHL(), SRA(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x2F:
                    Registers.A = SRA(Registers.A, 2, 8);
                    break;
                case 0x30:
                    Registers.B = Swap(Registers.B, 2, 8);
                    break;
                case 0x31:
                    Registers.C = Swap(Registers.C, 2, 8);
                    break;
                case 0x32:
                    Registers.D = Swap(Registers.D, 2, 8);
                    break;
                case 0x33:
                    Registers.E = Swap(Registers.E, 2, 8);
                    break;
                case 0x34:
                    Registers.H = Swap(Registers.H, 2, 8);
                    break;
                case 0x35:
                    Registers.L = Swap(Registers.L, 2, 8);
                    break;
                case 0x36:
                    _ram.StoreU8Bits(Registers.GetHL(), Swap(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x37:
                    Registers.A = Swap(Registers.A, 2, 8);
                    break;
                case 0x38:
                    Registers.B = SRL(Registers.B, 2, 8);
                    break;
                case 0x39:
                    Registers.C = SRL(Registers.C, 2, 8);
                    break;
                case 0x3A:
                    Registers.D = SRL(Registers.D, 2, 8);
                    break;
                case 0x3B:
                    Registers.E = SRL(Registers.E, 2, 8);
                    break;
                case 0x3C:
                    Registers.H = SRL(Registers.H, 2, 8);
                    break;
                case 0x3D:
                    Registers.L = SRL(Registers.L, 2, 8);
                    break;
                case 0x3E:
                    _ram.StoreU8Bits(Registers.GetHL(), SRL(_ram.LoadU8Bits(Registers.GetHL()), 2, 16));
                    break;
                case 0x3F:
                    Registers.A = SRL(Registers.A, 2, 8);
                    break;
                case 0x40:
                    Bit(Registers.B, 0, 2, 8);
                    break;
                case 0x41:
                    Bit(Registers.C, 0, 2, 8);
                    break;
                case 0x42:
                    Bit(Registers.D, 0, 2, 8);
                    break;
                case 0x43:
                    Bit(Registers.E, 0, 2, 8);
                    break;
                case 0x44:
                    Bit(Registers.H, 0, 2, 8);
                    break;
                case 0x45:
                    Bit(Registers.L, 0, 2, 8);
                    break;
                case 0x46:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 0, 2, 12);
                    break;
                case 0x47:
                    Bit(Registers.A, 0, 2, 8);
                    break;
                case 0x48:
                    Bit(Registers.B, 1, 2, 8);
                    break;
                case 0x49:
                    Bit(Registers.C, 1, 2, 8);
                    break;
                case 0x4A:
                    Bit(Registers.D, 1, 2, 8);
                    break;
                case 0x4B:
                    Bit(Registers.E, 1, 2, 8);
                    break;
                case 0x4C:
                    Bit(Registers.H, 1, 2, 8);
                    break;
                case 0x4D:
                    Bit(Registers.L, 1, 2, 8);
                    break;
                case 0x4E:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 1, 2, 12);
                    break;
                case 0x4F:
                    Bit(Registers.A, 1, 2, 8);
                    break;
                case 0x50:
                    Bit(Registers.B, 2, 2, 8);
                    break;
                case 0x51:
                    Bit(Registers.C, 2, 2, 8);
                    break;
                case 0x52:
                    Bit(Registers.D, 2, 2, 8);
                    break;
                case 0x53:
                    Bit(Registers.E, 2, 2, 8);
                    break;
                case 0x54:
                    Bit(Registers.H, 2, 2, 8);
                    break;
                case 0x55:
                    Bit(Registers.L, 2, 2, 8);
                    break;
                case 0x56:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 2, 2, 12);
                    break;
                case 0x57:
                    Bit(Registers.A, 2, 2, 8);
                    break;
                case 0x58:
                    Bit(Registers.B, 3, 2, 8);
                    break;
                case 0x59:
                    Bit(Registers.C, 3, 2, 8);
                    break;
                case 0x5A:
                    Bit(Registers.D, 3, 2, 8);
                    break;
                case 0x5B:
                    Bit(Registers.E, 3, 2, 8);
                    break;
                case 0x5C:
                    Bit(Registers.H, 3, 2, 8);
                    break;
                case 0x5D:
                    Bit(Registers.L, 3, 2, 8);
                    break;
                case 0x5E:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 3, 2, 12);
                    break;
                case 0x5F:
                    Bit(Registers.A, 3, 2, 8);
                    break;
                case 0x60:
                    Bit(Registers.B, 4, 2, 8);
                    break;
                case 0x61:
                    Bit(Registers.C, 4, 2, 8);
                    break;
                case 0x62:
                    Bit(Registers.D, 4, 2, 8);
                    break;
                case 0x63:
                    Bit(Registers.E, 4, 2, 8);
                    break;
                case 0x64:
                    Bit(Registers.H, 4, 2, 8);
                    break;
                case 0x65:
                    Bit(Registers.L, 4, 2, 8);
                    break;
                case 0x66:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 4, 2, 12);
                    break;
                case 0x67:
                    Bit(Registers.A, 4, 2, 8);
                    break;
                case 0x68:
                    Bit(Registers.B, 5, 2, 8);
                    break;
                case 0x69:
                    Bit(Registers.C, 5, 2, 8);
                    break;
                case 0x6A:
                    Bit(Registers.D, 5, 2, 8);
                    break;
                case 0x6B:
                    Bit(Registers.E, 5, 2, 8);
                    break;
                case 0x6C:
                    Bit(Registers.H, 5, 2, 8);
                    break;
                case 0x6D:
                    Bit(Registers.L, 5, 2, 8);
                    break;
                case 0x6E:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 5, 2, 12);
                    break;
                case 0x6F:
                    Bit(Registers.A, 5, 2, 8);
                    break;
                case 0x70:
                    Bit(Registers.B, 6, 2, 8);
                    break;
                case 0x71:
                    Bit(Registers.C, 6, 2, 8);
                    break;
                case 0x72:
                    Bit(Registers.D, 6, 2, 8);
                    break;
                case 0x73:
                    Bit(Registers.E, 6, 2, 8);
                    break;
                case 0x74:
                    Bit(Registers.H, 6, 2, 8);
                    break;
                case 0x75:
                    Bit(Registers.L, 6, 2, 8);
                    break;
                case 0x76:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 6, 2, 12);
                    break;
                case 0x77:
                    Bit(Registers.A, 6, 2, 8);
                    break;
                case 0x78:
                    Bit(Registers.B, 7, 2, 8);
                    break;
                case 0x79:
                    Bit(Registers.C, 7, 2, 8);
                    break;
                case 0x7A:
                    Bit(Registers.D, 7, 2, 8);
                    break;
                case 0x7B:
                    Bit(Registers.E, 7, 2, 8);
                    break;
                case 0x7C:
                    Bit(Registers.H, 7, 2, 8);
                    break;
                case 0x7D:
                    Bit(Registers.L, 7, 2, 8);
                    break;
                case 0x7E:
                    Bit(_ram.LoadU8Bits(Registers.GetHL()), 7, 2, 12);
                    break;
                case 0x7F:
                    Bit(Registers.A, 7, 2, 8);
                    break;
                case 0x80:
                    Registers.B = RES(Registers.B, 0, 2, 8);
                    break;
                case 0x81:
                    Registers.C = RES(Registers.C, 0, 2, 8);
                    break;
                case 0x82:
                    Registers.D = RES(Registers.D, 0, 2, 8);
                    break;
                case 0x83:
                    Registers.E = RES(Registers.E, 0, 2, 8);
                    break;
                case 0x84:
                    Registers.H = RES(Registers.H, 0, 2, 8);
                    break;
                case 0x85:
                    Registers.L = RES(Registers.L, 0, 2, 8);
                    break;
                case 0x86:
                   _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 0, 2, 16));
                    break;
                case 0x87:
                    Registers.A = RES(Registers.A, 0, 2, 8);
                    break;
                case 0x88:
                    Registers.B = RES(Registers.B, 1, 2, 8);
                    break;
                case 0x89:
                    Registers.C = RES(Registers.C, 1, 2, 8);
                    break;
                case 0x8A:
                    Registers.D = RES(Registers.D, 1, 2, 8);
                    break;
                case 0x8B:
                    Registers.E = RES(Registers.E, 1, 2, 8);
                    break;
                case 0x8C:
                    Registers.H = RES(Registers.H, 1, 2, 8);
                    break;
                case 0x8D:
                    Registers.L = RES(Registers.L, 1, 2, 8);
                    break;
                case 0x8E:
                    _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 1, 2, 16));
                    break;
                case 0x8F:
                    Registers.A = RES(Registers.A, 1, 2, 8);
                    break;
                case 0x90:
                    Registers.B = RES(Registers.B, 2, 2, 8);
                    break;
                case 0x91:
                    Registers.C = RES(Registers.C, 2, 2, 8);
                    break;
                case 0x92:
                    Registers.D = RES(Registers.D, 2, 2, 8);
                    break;
                case 0x93:
                    Registers.E = RES(Registers.E, 2, 2, 8);
                    break;
                case 0x94:
                    Registers.H = RES(Registers.H, 2, 2, 8);
                    break;
                case 0x95:
                    Registers.L = RES(Registers.L, 2, 2, 8);
                    break;
                case 0x96:
                    _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 2, 2, 16));
                    break;
                case 0x97:
                    Registers.A = RES(Registers.A, 2, 2, 8);
                    break;
                case 0x98:
                    Registers.B = RES(Registers.B, 3, 2, 8);
                    break;
                case 0x99:
                    Registers.C = RES(Registers.C, 3, 2, 8);
                    break;
                case 0x9A:
                    Registers.D = RES(Registers.D, 3, 2, 8);
                    break;
                case 0x9B:
                    Registers.E = RES(Registers.E, 3, 2, 8);
                    break;
                case 0x9C:
                    Registers.H = RES(Registers.H, 3, 2, 8);
                    break;
                case 0x9D:
                    Registers.L = RES(Registers.L, 3, 2, 8);
                    break;
                case 0x9E:
                    _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 3, 2, 16));
                    break;
                case 0x9F:
                    Registers.A = RES(Registers.A, 3, 2, 8);
                    break;
                case 0xA0:
                    Registers.B = RES(Registers.B, 4, 2, 8);
                    break;
                case 0xA1:
                    Registers.C = RES(Registers.C, 4, 2, 8);
                    break;
                case 0xA2:
                    Registers.D = RES(Registers.D, 4, 2, 8);
                    break;
                case 0xA3:
                    Registers.E = RES(Registers.E, 4, 2, 8);
                    break;
                case 0xA4:
                    Registers.H = RES(Registers.H, 4, 2, 8);
                    break;
                case 0xA5:
                    Registers.L = RES(Registers.L, 4, 2, 8);
                    break;
                case 0xA6:
                    _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 4, 2, 16));
                    break;
                case 0xA7:
                    Registers.A = RES(Registers.A, 4, 2, 8);
                    break;
                case 0xA8:
                    Registers.B = RES(Registers.B, 5, 2, 8);
                    break;
                case 0xA9:
                    Registers.C = RES(Registers.C, 5, 2, 8);
                    break;
                case 0xAA:
                    Registers.D = RES(Registers.D, 5, 2, 8);
                    break;
                case 0xAB:
                    Registers.E = RES(Registers.E, 5, 2, 8);
                    break;
                case 0xAC:
                    Registers.H = RES(Registers.H, 5, 2, 8);
                    break;
                case 0xAD:
                    Registers.L = RES(Registers.L, 5, 2, 8);
                    break;
                case 0xAE:
                    _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 5, 2, 16));
                    break;
                case 0xAF:
                    Registers.A = RES(Registers.A, 5, 2, 8);
                    break;
                case 0xB0:
                    Registers.B = RES(Registers.B, 6, 2, 8);
                    break;
                case 0xB1:
                    Registers.C = RES(Registers.C, 6, 2, 8);
                    break;
                case 0xB2:
                    Registers.D = RES(Registers.D, 6, 2, 8);
                    break;
                case 0xB3:
                    Registers.E = RES(Registers.E, 6, 2, 8);
                    break;
                case 0xB4:
                    Registers.H = RES(Registers.H, 6, 2, 8);
                    break;
                case 0xB5:
                    Registers.L = RES(Registers.L, 6, 2, 8);
                    break;
                case 0xB6:
                    _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 6, 2, 16));
                    break;
                case 0xB7:
                    Registers.A = RES(Registers.A, 6, 2, 8);
                    break;
                case 0xB8:
                    Registers.B = RES(Registers.B, 7, 2, 8);
                    break;
                case 0xB9:
                    Registers.C = RES(Registers.C, 7, 2, 8);
                    break;
                case 0xBA:
                    Registers.D = RES(Registers.D, 7, 2, 8);
                    break;
                case 0xBB:
                    Registers.E = RES(Registers.E, 7, 2, 8);
                    break;
                case 0xBC:
                    Registers.H = RES(Registers.H, 7, 2, 8);
                    break;
                case 0xBD:
                    Registers.L = RES(Registers.L, 7, 2, 8);
                    break;
                case 0xBE:
                    _ram.StoreU8Bits(Registers.GetHL(), RES(_ram.LoadU8Bits(Registers.GetHL()), 7, 2, 16));
                    break;
                case 0xBF:
                    Registers.A = RES(Registers.A, 7, 2, 8);
                    break;
                case 0xC0:
                    Registers.B = SET(Registers.B, 0, 2, 8);
                    break;
                case 0xC1:
                    Registers.C = SET(Registers.C, 0, 2, 8);
                    break;
                case 0xC2:
                    Registers.D = SET(Registers.D, 0, 2, 8);
                    break;
                case 0xC3:
                    Registers.E = SET(Registers.E, 0, 2, 8);
                    break;
                case 0xC4:
                    Registers.H = SET(Registers.H, 0, 2, 8);
                    break;
                case 0xC5:
                    Registers.L = SET(Registers.L, 0, 2, 8);
                    break;
                case 0xC6:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 0, 2, 16));
                    break;
                case 0xC7:
                    Registers.A = SET(Registers.A, 0, 2, 8);
                    break;
                case 0xC8:
                    Registers.B = SET(Registers.B, 1, 2, 8);
                    break;
                case 0xC9:
                    Registers.C = SET(Registers.C, 1, 2, 8);
                    break;
                case 0xCA:
                    Registers.D = SET(Registers.D, 1, 2, 8);
                    break;
                case 0xCB:
                    Registers.E = SET(Registers.E, 1, 2, 8);
                    break;
                case 0xCC:
                    Registers.H = SET(Registers.H, 1, 2, 8);
                    break;
                case 0xCD:
                    Registers.L = SET(Registers.L, 1, 2, 8);
                    break;
                case 0xCE:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 1, 2, 16));
                    break;
                case 0xCF:
                    Registers.A = SET(Registers.A, 1, 2, 8);
                    break;
                case 0xD0:
                    Registers.B = SET(Registers.B, 2, 2, 8);
                    break;
                case 0xD1:
                    Registers.C = SET(Registers.C, 2, 2, 8);
                    break;
                case 0xD2:
                    Registers.D = SET(Registers.D, 2, 2, 8);
                    break;
                case 0xD3:
                    Registers.E = SET(Registers.E, 2, 2, 8);
                    break;
                case 0xD4:
                    Registers.H = SET(Registers.H, 2, 2, 8);
                    break;
                case 0xD5:
                    Registers.L = SET(Registers.L, 2, 2, 8);
                    break;
                case 0xD6:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 2, 2, 16));
                    break;
                case 0xD7:
                    Registers.A = SET(Registers.A, 2, 2, 8);
                    break;
                case 0xD8:
                    Registers.B = SET(Registers.B, 3, 2, 8);
                    break;
                case 0xD9:
                    Registers.C = SET(Registers.C, 3, 2, 8);
                    break;
                case 0xDA:
                    Registers.D = SET(Registers.D, 3, 2, 8);
                    break;
                case 0xDB:
                    Registers.E = SET(Registers.E, 3, 2, 8);
                    break;
                case 0xDC:
                    Registers.H = SET(Registers.H, 3, 2, 8);
                    break;
                case 0xDD:
                    Registers.L = SET(Registers.L, 3, 2, 8);
                    break;
                case 0xDE:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 3, 2, 16));
                    break;
                case 0xDF:
                    Registers.A = SET(Registers.A, 3, 2, 8);
                    break;
                case 0xE0:
                    Registers.B = SET(Registers.B, 4, 2, 8);
                    break;
                case 0xE1:
                    Registers.C = SET(Registers.C, 4, 2, 8);
                    break;
                case 0xE2:
                    Registers.D = SET(Registers.D, 4, 2, 8);
                    break;
                case 0xE3:
                    Registers.E = SET(Registers.E, 4, 2, 8);
                    break;
                case 0xE4:
                    Registers.H = SET(Registers.H, 4, 2, 8);
                    break;
                case 0xE5:
                    Registers.L = SET(Registers.L, 4, 2, 8);
                    break;
                case 0xE6:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 4, 2, 16));
                    break;
                case 0xE7:
                    Registers.A = SET(Registers.A, 4, 2, 8);
                    break;
                case 0xE8:
                    Registers.B = SET(Registers.B, 5, 2, 8);
                    break;
                case 0xE9:
                    Registers.C = SET(Registers.C, 5, 2, 8);
                    break;
                case 0xEA:
                    Registers.D = SET(Registers.D, 5, 2, 8);
                    break;
                case 0xEB:
                    Registers.E = SET(Registers.E, 5, 2, 8);
                    break;
                case 0xEC:
                    Registers.H = SET(Registers.H, 5, 2, 8);
                    break;
                case 0xED:
                    Registers.L = SET(Registers.L, 5, 2, 8);
                    break;
                case 0xEE:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 5, 2, 16));
                    break;
                case 0xEF:
                    Registers.A = SET(Registers.A, 5, 2, 8);
                    break;
                case 0xF0:
                    Registers.B = SET(Registers.B, 6, 2, 8);
                    break;
                case 0xF1:
                    Registers.C = SET(Registers.C, 6, 2, 8);
                    break;
                case 0xF2:
                    Registers.D = SET(Registers.D, 6, 2, 8);
                    break;
                case 0xF3:
                    Registers.E = SET(Registers.E, 6, 2, 8);
                    break;
                case 0xF4:
                    Registers.H = SET(Registers.H, 6, 2, 8);
                    break;
                case 0xF5:
                    Registers.L = SET(Registers.L, 6, 2, 8);
                    break;
                case 0xF6:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 6, 2, 16));
                    break;
                case 0xF7:
                    Registers.A = SET(Registers.A, 6, 2, 8);
                    break;
                case 0xF8:
                    Registers.B = SET(Registers.B, 7, 2, 8);
                    break;
                case 0xF9:
                    Registers.C = SET(Registers.C, 7, 2, 8);
                    break;
                case 0xFA:
                    Registers.D = SET(Registers.D, 7, 2, 8);
                    break;
                case 0xFB:
                    Registers.E = SET(Registers.E, 7, 2, 8);
                    break;
                case 0xFC:
                    Registers.H = SET(Registers.H, 7, 2, 8);
                    break;
                case 0xFD:
                    Registers.L = SET(Registers.L, 7, 2, 8);
                    break;
                case 0xFE:
                    _ram.StoreU8Bits(Registers.GetHL(), SET(_ram.LoadU8Bits(Registers.GetHL()), 7, 2, 16));
                    break;
                case 0xFF:
                    Registers.A = SET(Registers.A, 7, 2, 8);
                    break;
            }
        }
    }
}
