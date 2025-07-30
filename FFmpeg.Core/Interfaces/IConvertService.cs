using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Interfaces
{
    public interface IConvertService
    {
        Task<byte[]?> ConvertDataAsync(FFmpeg.Core.Models.ConvertRequest request);
    }
}
