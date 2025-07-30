namespace FFmpeg.API.DTOs
{
    public class CropDto
    {
        public IFormFile VideoFile { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

    }
}
