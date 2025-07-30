using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;

namespace FFmpeg.Infrastructure.Services
{
    public class VideoCuttingService
    {
        private readonly CutVideoCommand _command;

        public VideoCuttingService(CutVideoCommand command)
        {
            _command = command;
        }

        public async Task<bool> CutVideoAsync(CutVideoModel model)
        {
            return await _command.ExecuteAsync(model);
        }
    }
}
