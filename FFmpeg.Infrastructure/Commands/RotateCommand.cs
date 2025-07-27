using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;
namespace Ffmpeg.Command.Commands
{
    public class RotateCommand : BaseCommand, ICommand<RotateModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public RotateCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(RotateModel model)
        {
            double radians = model.RotationAngle * Math.PI / 180.0;
            string rotationFilter = $"rotate={radians}";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"{rotationFilter}\"");

            if (model.IsVideo)
            {
                CommandBuilder.SetVideoCodec(model.VideoCodec);
            }

            CommandBuilder.SetOutput(model.OutputFile, model.IsVideo ? false : true);

            return await RunAsync();
        }
    }
}