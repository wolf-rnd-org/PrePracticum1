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
    public class TimestampOverlayCommand : BaseCommand, ICommand<TimestampOverlayModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public TimestampOverlayCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(TimestampOverlayModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(model.InputFilePath)) throw new ArgumentException("Input file required");
            if (string.IsNullOrEmpty(model.OutputFilePath)) throw new ArgumentException("Output file required");

            string timestampFilter = "drawtext=text='%{pts\\:hms}':x=10:y=10:fontsize=24:fontcolor=white";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFilePath)
                .AddOption($"-vf \"{timestampFilter}\"")
                .SetOutput(model.OutputFilePath);

            return await RunAsync();
        }
    }
}
