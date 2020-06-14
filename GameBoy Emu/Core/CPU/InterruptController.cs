using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.cpu
{
    public class InterruptController
    {
        public const byte VBLANK_MASK = 0x1;
        public const byte LCDC_MASK = 0x2;
        public const byte TIMER_MASK = 0x4;
        public const byte SERIAL_MASK = 0x8;
        public const byte JOYPAD_MASK = 0x10;

        public bool IME { get; set; }

        private readonly Interrupt _vblank;
        private readonly Interrupt _lcdc;
        private readonly Interrupt _timer;
        private readonly Interrupt _serial;
        private readonly Interrupt _joypad;


        public InterruptController()
        {
            IME = false;
            _vblank = new Interrupt(0x40, VBLANK_MASK, Interrupt.InterruptType.VBLANK);
            _lcdc = new Interrupt(0x48, LCDC_MASK, Interrupt.InterruptType.LCDC);
            _timer = new Interrupt(0x50, TIMER_MASK, Interrupt.InterruptType.TIMER);
            _serial = new Interrupt(0x58, SERIAL_MASK, Interrupt.InterruptType.SERIAL);
            _joypad = new Interrupt(0x60, JOYPAD_MASK, Interrupt.InterruptType.JOYPAD);
        }

        public bool isVBlankSet(byte interruptFlag)
        {
            return BitUtils.IsBitSet(interruptFlag, 0);
        }

        public bool isLCDCSet(byte interruptFlag)
        {
            return BitUtils.IsBitSet(interruptFlag, 1);
        }

        public bool isTimerOverflowSet(byte interruptFlag)
        {
            return BitUtils.IsBitSet(interruptFlag, 2);
        }

        public bool isSerialCompleteSet(byte interruptFlag)
        {
            return BitUtils.IsBitSet(interruptFlag, 3);
        }

        public bool isJoypadSet(byte interruptFlag)
        {
            return BitUtils.IsBitSet(interruptFlag, 4);
        }

        public Interrupt Process(byte interruptFlag, byte interruptEnable)
        {
            return CheckForInterruptRequests(interruptFlag, interruptEnable);
        }

        private Interrupt CheckForInterruptRequests(byte interruptFlag, byte interruptEnable)
        {
            if (isVBlankSet(interruptFlag) && isVBlankSet(interruptEnable))
            {
                return _vblank;
            }

            if (isLCDCSet(interruptFlag) && isLCDCSet(interruptEnable))
            {
                return _lcdc;
            }

            if (isTimerOverflowSet(interruptFlag) && isTimerOverflowSet(interruptEnable))
            {
                return _timer;
            }

            if (isSerialCompleteSet(interruptFlag) && isSerialCompleteSet(interruptEnable))
            {
                return _serial;
            }

            if (isJoypadSet(interruptFlag) && isJoypadSet(interruptEnable))
            {
                return _joypad;
            }

            return null;
        }
    }
}
