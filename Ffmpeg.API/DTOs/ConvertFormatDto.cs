namespace FFmpeg.API.DTOs
{
    public class ConvertFormatDto
    {
        public IFormFile InputFile { get; set; }
        public string OutputFileName { get; set; }
    }
}
