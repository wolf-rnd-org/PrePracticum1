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
    public class AnimatedTextCommand : BaseCommand, ICommand<AnimatedTextModel>
    {
        private readonly ICommandBuilder _commandBuilder;
        public AnimatedTextCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(AnimatedTextModel model)
        {
            string xExpr = model.IsAnimated
                ? $"w - mod(t*{model.AnimationSpeed}, w+text_w)" 
                : model.XPosition.ToString();

            string drawTextFilter =
                $"drawtext=text='{model.Content}':x={xExpr}:y={model.YPosition.ToString()}:fontsize={model.FontSize}:fontcolor={model.FontColor}";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption("-vf")
                .AddFilterComplex(drawTextFilter);

            if (model.IsVideo)
            {
                CommandBuilder
                    .SetVideoCodec(model.VideoCodec);
            }

            CommandBuilder.SetOutput(model.OutputFile, model.IsVideo ? false : true);

            return await RunAsync();
        }
    }
}
