using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.Core
{
    public class InterruptController
    {
        public bool IME { get; set; }

        public static readonly int IF_REGISTER_ADDRESS = 0xFF0F;
        public static readonly int IE_REGISTER_ADDRESS = 0xFFFF;

        public InterruptController()
        {
            IME = false;
        }

        //public bool isVBlankSet()
        //{
        //    //return 
        //}

        public void CheckForInterrupts(byte interruptFlag, byte interruptEnable)
        {
            // check IF for interrupt request

            if (IME)
            {
                //Console.WriteLine(_ram.GetIE());
                // Console.WriteLine(_ram.GetIF());
            }
        }
    }
}
