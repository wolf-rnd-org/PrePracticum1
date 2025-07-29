using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class ChangeBrightnessAndContrastCommand : BaseCommand, ICommand<ChangeBrightnessAndContrastModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ChangeBrightnessAndContrastCommand(ICommandBuilder commandBuilder, FFmpegExecutor executor) : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ChangeBrightnessAndContrastModel model)
        {
            if (model.Brightness < -1 || model.Brightness > 1) 
                throw new ArgumentOutOfRangeException(nameof(model.Brightness), "Brightness must be between -1.0 and 1.0");// בין 1- ל1

            if (model.Contrast <= 0)// גדול מאפס 
                throw new ArgumentOutOfRangeException(nameof(model.Contrast), "Contrast must be greater than 0");

            CommandBuilder = _commandBuilder
                .SetInput(model.InputVideoPath)
                .AddOption($"-vf eq=brightness={model.Brightness}:contrast={model.Contrast}")
                .AddOption($"-map 0") // למפות את כל הזרמים (וידאו + שמע)
                .AddOption($"-c:a copy")
                .SetOutput(model.OutputVideoPath);

            return await RunAsync();
        }
    }
}