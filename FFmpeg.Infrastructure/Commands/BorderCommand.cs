using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class BorderCommand : BaseCommand, ICommand<BorderModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public BorderCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(BorderModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddFilterComplex($"[0:v]pad=iw+{model.BorderSize * 2}:ih+{model.BorderSize * 2}:{model.BorderSize}:{model.BorderSize}:color={model.FrameColor}[out]");

            if (model.IsVideo)
            {
                CommandBuilder = CommandBuilder
                    .SetVideoCodec(model.VideoCodec)
                    .SetOutput(model.OutputFile, isFrameOutput: false);
            }
            else
            {
                CommandBuilder = CommandBuilder
                    .SetOutput(model.OutputFile, isFrameOutput: true);
            }

            return await RunAsync();
        }
    }
}
