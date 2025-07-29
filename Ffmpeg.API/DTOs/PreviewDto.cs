using System.ComponentModel;

namespace FFmpeg.API.DTOs
{
    public class PreviewDto
    {
        public IFormFile VideoFile { get; set; }
        [DefaultValue("00:00:05")]
        public string ?Timestamp { get; set; } = "00:00:05";

    }
}
