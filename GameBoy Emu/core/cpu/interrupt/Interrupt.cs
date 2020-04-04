namespace ChichoGB.Core.CPU.Interrupts
{
    public class Interrupt
    {
        public ushort Address { get; set; }
        public int Flag { get; set; }
        public InterruptType Type { get; set; }
        public enum InterruptType { VBLANK, LCDC, TIMER, SERIAL, JOYPAD }

        public Interrupt(ushort address, int flag, InterruptType type)
        {
            Address = address;
            Flag = flag;
            Type = type;
        }
    }
}
