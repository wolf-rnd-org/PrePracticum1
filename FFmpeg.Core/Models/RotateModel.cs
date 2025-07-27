namespace FFmpeg.Core.Models
{
    public class RotateModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public double RotationAngle { get; set; } 
        public bool IsVideo { get; set; }
        public string VideoCodec { get; set; } = "libx264"; 
    }
}