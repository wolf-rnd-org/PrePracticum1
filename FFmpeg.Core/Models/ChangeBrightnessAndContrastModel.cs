using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class ChangeBrightnessAndContrastModel
    {
        public string InputVideoPath { get; set; }     // שם הקובץ המקורי
        public string OutputVideoPath { get; set; }    // שם הקובץ לאחר עיבוד
        public double Brightness { get; set; }         // -טווח: 1.0 עד 1.0
        public double Contrast { get; set; }           // טווח: > 0, לדוגמה 1.0 רגיל
    }
}
