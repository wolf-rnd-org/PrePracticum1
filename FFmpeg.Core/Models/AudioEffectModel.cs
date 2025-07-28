using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class AudioEffectModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string EffectType { get; set; } // "echo", "background", etc.
        public double Duration { get; set; } // Duration in seconds
        public Dictionary<string, object> EffectParameters { get; set; } = new Dictionary<string, object>();
    }
} 