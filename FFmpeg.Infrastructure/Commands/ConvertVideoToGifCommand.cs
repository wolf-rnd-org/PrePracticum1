using Ffmpeg.Command.Commands;
using FFmpeg.Infrastructure.Services;
using Microsoft.Extensions.Configuration; // צריך להוסיף את זה
using System;
using System.Threading.Tasks;
using FFmpeg.Core.Models;

namespace FFmpeg.Infrastructure.Commands
{
    public class ConvertVideoToGifCommand : BaseCommand, ICommand<ConvertVideoToGifModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ConvertVideoToGifCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ConvertVideoToGifModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            string filter = $"fps={model.Fps},scale={model.Width}:-1";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"{filter}\"")
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }

    }
}
