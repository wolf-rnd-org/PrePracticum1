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
    public class PreviewCommand:BaseCommand, ICommand<PreviewModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public PreviewCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(PreviewModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-ss {model.Timestamp}")
                .AddOption("-vframes 1")
                .SetOutput(model.OutputFile, true);

            return await RunAsync();
        }
    }


}
