namespace FFmpeg.API.DTOs
{
    public class MergeTwoFilesDto
    {
        public IFormFile FirstInputFile { get; set; }
        public IFormFile SecondInputFile { get; set; }
    }
}
