namespace FFmpeg.API.DTOs
{
    public class ExtractFrameDto
    {
        public IFormFile VideoFile { get; set; }
        public TimeSpan TimeSpan { get; set; } // 
    }
}
