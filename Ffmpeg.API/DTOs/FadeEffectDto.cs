namespace FFmpeg.API.DTOs
{
    public class FadeEffectDto
    {
        public IFormFile VideoFile { get; set; }
        public int FadeInDurationSeconds { get; set; } = 2; 
        public string OutputFileName { get; set; }     
    }
}
