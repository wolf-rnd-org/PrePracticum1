using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class ExtractAudioCommand : BaseCommand, ICommand<ExtractAudioModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ExtractAudioCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ExtractAudioModel model)
        {
            if (string.IsNullOrWhiteSpace(model.InputFile))
                throw new ArgumentException("InputFile is required.", nameof(model.InputFile));

            if (string.IsNullOrWhiteSpace(model.OutputFile))
                throw new ArgumentException("OutputFile is required.", nameof(model.OutputFile));

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption("-q:a 0")
                .AddOption("-map a") 
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }
    }
}
