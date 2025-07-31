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
    public class ConvertFormatCommand : BaseCommand, ICommand<ConvertFormatModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ConvertFormatCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ConvertFormatModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile);

            if (!string.IsNullOrEmpty(model.VideoCodec))
            {
                CommandBuilder.SetVideoCodec(model.VideoCodec);
            }

            CommandBuilder.SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}
