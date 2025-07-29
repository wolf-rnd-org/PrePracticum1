using Ffmpeg.Command.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.Core.Models;
namespace FFmpeg.Infrastructure.Commands
{
    public class ExtractFrameCommand : BaseCommand, ICommand<ExtractFrameInput>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ExtractFrameCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }
        public async Task<CommandResult> ExecuteAsync(ExtractFrameInput model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-ss {model.TimeSpan}")
                .AddOption("-vframes 1")
                .SetOutput(model.OutputImagePath, true); // true: no re-encode
            return await RunAsync();
        }
    }
}
