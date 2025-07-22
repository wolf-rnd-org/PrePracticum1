using Ffmpeg.Command.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class FadeEffectCommand :BaseCommand, ICommand<FadeEffectModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public FadeEffectCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(FadeEffectModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(model.InputFile)) throw new ArgumentException("Input file required");
            if (string.IsNullOrEmpty(model.OutputFile)) throw new ArgumentException("Output file required");

            string fadeFilter = $"fade=t=in:st=0:d={model.DurationSeconds}";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"{fadeFilter}\"")      
                .SetVideoCodec(model.VideoCodec)
                .SetOutput(model.OutputFile);

            if (model.IsVideo)
            {
                CommandBuilder.SetVideoCodec(model.VideoCodec);
            }

            CommandBuilder.SetOutput(model.OutputFile, model.IsVideo ? false : true);


            return await RunAsync();
        }

    }
}
