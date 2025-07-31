using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    internal class SpecificSoundLevel
    {

        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public double VolumeFactor { get; set; }


    }
}
