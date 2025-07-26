using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class ExtractFrameInput
    {
        public string InputFile { get; set; }
        public TimeSpan TimeSpan { get; set; }
        [Required]
        [RegularExpression(@".+\.(jpg|jpeg|png)$", ErrorMessage = "פורמט תמונה לא חוקי. יש לבחור JPG או PNG.")]
        public string OutputImagePath { get; set; }
        
    }
}
