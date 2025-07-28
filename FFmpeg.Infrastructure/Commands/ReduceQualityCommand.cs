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
    public class ReduceQualityCommand : BaseCommand, ICommand<ReduceQualityModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ReduceQualityCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ReduceQualityModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .SetVideoCodec("libx264")
                .SetVideoQuality(28)
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }

}
