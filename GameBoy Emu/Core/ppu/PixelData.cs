namespace GameBoy_Emu.core.ppu
{
    public class PixelData
    {
        public PixelType Type { get; set; }
        public int ColorData { get; set; }
        public enum PixelType { BG, WINDOW, SPRITE };
    }
}