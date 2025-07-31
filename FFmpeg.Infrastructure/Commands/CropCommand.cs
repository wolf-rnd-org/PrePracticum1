using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class CropCommand : BaseCommand, ICommand<CropModel>
    {
        private readonly ICommandBuilder _commandBuilder;
        public CropCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }
        public async Task<CommandResult> ExecuteAsync(CropModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            string cropFilter = $"[0:v]crop={model.Width}:{model.Height}:{model.X}:{model.Y}[out]";
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddFilterComplex(cropFilter)
                .SetOutput(model.OutputFile);
            return await RunAsync();
        }
    }
}
