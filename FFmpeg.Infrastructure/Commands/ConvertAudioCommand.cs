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


    public class ConvertAudioCommand : BaseCommand, ICommand<ConvertAudioModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ConvertAudioCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ConvertAudioModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .SetOutput(model.OutputFile, true);

            return await RunAsync();
        }
    }
}