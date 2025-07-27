using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class AnimatedTextModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string Content { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public int FontSize { get; set; }
        public string FontColor { get; set; } = "white";
        public string VideoCodec { get; set; } = "libx264";
        public bool IsVideo { get; set; } = true;
        public bool IsAnimated { get; set; } = false;
        public int AnimationSpeed { get; set; } = 100;
    }
}
