namespace FFmpeg.API.DTOs
{
    public class PreviewDto
    {
        public IFormFile VideoFile { get; set; }
        public string ?Timestamp { get; set; } = "00:00:05"; // ברירת מחדל

    }
}
