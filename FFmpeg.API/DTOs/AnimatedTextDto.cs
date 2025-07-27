namespace FFmpeg.API.DTOs
{
    public class AnimatedTextDto
    {
        public IFormFile VideoFile { get; set; }
        public string Content { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public int FontSize { get; set; }
        public string FontColor { get; set; } = "white";
        public bool IsAnimated { get; set; } = false;
        public int AnimationSpeed { get; set; } = 100;
    }
}
