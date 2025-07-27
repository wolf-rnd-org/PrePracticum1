using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class MergeTwoFilesModel
    {
        public string FirstInputFile { get; set; }
        public string SecondInputFile { get; set; }
        public string OutputFile { get; set; }
    }
}
