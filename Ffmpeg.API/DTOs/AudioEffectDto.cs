using Microsoft.AspNetCore.Http;

{
    public class AudioEffectDto
    {
        public IFormFile VideoFile { get; set; }
        public string EffectType { get; set; } // "echo", "background", etc.
        public double Duration { get; set; } // Duration in seconds
        public string OutputFileName { get; set; }
    }
} 
