namespace FFmpeg.API.DTOs
{
    public class CutVideoDto
    {
        public IFormFile VideoFile { get; set; }  // קובץ הווידאו
        public string StartTime { get; set; }     // זמן התחלה - למשל "00:00:05"
        public string EndTime { get; set; }       // זמן סיום - למשל "00:00:10"
    }
}

