using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FFmpeg.Core.Models
{
    public class ConvertRequest
    {
        public IFormFile Video { get; set; }
        public string Bitrate { get; set; }
    }
}
