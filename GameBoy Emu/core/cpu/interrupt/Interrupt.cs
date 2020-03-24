using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChichoGB.Core.CPU.Interrupts
{
    public class Interrupt
    {
        public ushort Address { get; set; }
        public int Flag { get; set; }
        
        public Interrupt(ushort address, int flag)
        {
            Address = address;
            Flag = flag;
        }
    }
}
