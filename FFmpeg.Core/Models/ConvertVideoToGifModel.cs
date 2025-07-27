using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class ConvertVideoToGifModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public int Fps { get; set; } = 10;            // Frames per second (ברירת מחדל 10)
        public int Width { get; set; } = 320;         // רוחב הגיף (גובה יחסית אוטומטי)
    }

}
