namespace ChichoGB.Core
{
    public class PixelData
    {
        public PixelType Type { get; set; }
        public int ColorData { get; set; }
        public enum PixelType { BG, WINDOW, SPRITE };
    }
}