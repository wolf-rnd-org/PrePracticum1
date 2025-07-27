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
    public class SubtitleTranslationCommand: BaseCommand, ICommand<SubtitleTanslationModel>
    {

        private readonly ICommandBuilder _commandBuilder;

        public SubtitleTranslationCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(SubtitleTanslationModel model)
        {
            CommandBuilder = _commandBuilder
             .SetInput(model.InputFile)
             .AddOption($"-vf subtitles={model.SubtitleFile}") 
             .SetOutput(model.OutputFile);

           CommandBuilder.SetOutput(model.OutputFile);

            return await RunAsync();
        }

    }

}
