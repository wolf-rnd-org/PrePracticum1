using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class GreenScreenModel
    {
        public string InputFile { get; set; }
        public string BackgroundFile { get; set; }
        public string OutputFile { get; set; }
        public string GreenColorHex { get; set; } = "0x00FF00"; // default green color in hex format
        public double Similarity { get; set; } = 0.1;
        public double Blend { get; set; } = 0.2;
        public string VideoCodec { get; set; } = "libx264";
    }
}
