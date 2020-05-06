using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.input
{
    public class Joypad
    {
        private byte _directions = 0xf;
        private byte _buttons = 0xf;

        public void PressDirection(int bit)
        {
            _directions = BitUtils.ClearBit(_directions, bit);
        }
        
        public void PressButton(int bit)
        {
            _buttons = BitUtils.ClearBit(_buttons, bit);
        }
        
        public void ReleaseDirection(int bit)
        {
            _directions = BitUtils.SetBit(_directions, bit);
        }
        
        public void ReleaseButton(int bit)
        {
            _buttons = BitUtils.SetBit(_buttons, bit);
        }

        public byte Process(byte b)
        {
            bool interruptRequest = false;
            if ((b & 0x30) == 0x10)
            {
                b |= _buttons;
                interruptRequest = true;
            }
            else if ((b & 0x30) == 0x20)
            {
                b |= _directions;
                interruptRequest = true;
            }
            else
            {
                b |= 0xf;
            }

            if (interruptRequest)
            {
                // joyp irq
            }

            return b;
        }
    }
}