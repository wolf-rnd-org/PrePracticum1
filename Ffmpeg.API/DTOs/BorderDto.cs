namespace FFmpeg.API.DTOs
{
    public class BorderDto
    {
        public IFormFile VideoFile { get; set; }
        public string? BorderSize { get; set; } = "20";
        public string? FrameColor { get; set; } = "blue";
    }
}
