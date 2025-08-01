using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class RotateDto
    {
        public IFormFile VideoFile { get; set; }
        public double RotationAngle { get; set; }
    }
}