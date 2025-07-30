using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class PreviewModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string Timestamp { get; set; } = "00:00:05";

    }
}
