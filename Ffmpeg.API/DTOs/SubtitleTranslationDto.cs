namespace FFmpeg.API.DTOs
{
    public class SubtitleTranslationDto
    {
        public IFormFile VideoFile { get; set; }
        public IFormFile SubtitleFile { get; set; }
        public string TargetLanguage { get; set; }
    }
}
