namespace FFmpeg.API.DTOs
{
    public class AnimatedTextDto
    {
        public IFormFile VideoFile { get; set; }
        public string Content { get; set; }= "Hello, World!";
        public int XPosition { get; set; } = 0;
        public int YPosition { get; set; } = 0;
        public int FontSize { get; set; } = 60;
        public string FontColor { get; set; } = "white";
        public bool IsAnimated { get; set; } = true;
        public int AnimationSpeed { get; set; } = 100;
    }
}
