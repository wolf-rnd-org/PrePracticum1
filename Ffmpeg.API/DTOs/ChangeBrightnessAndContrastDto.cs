namespace FFmpeg.API.DTOs
{
    public class ChangeBrightnessAndContrastDto
    {
        public required IFormFile SourceFile { get; set; }
        public string OutputFileName { get; set; } = "output.mp4"; // שם הקובץ לאחר עיבוד
        public double Brightness { get; set; } = 1.0;         // -טווח: 1.0 עד 1.0
        public double Contrast { get; set; } = 1.0;          // טווח: > 0, לדוגמה 1.0 רגיל
    }
}
