using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using System.Globalization;

namespace FFmpeg.Infrastructure.Commands
{
    public class ChangeSpeedCommand : BaseCommand, ICommand<ChangeSpeedModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ChangeSpeedCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ChangeSpeedModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(model.InputFile)) throw new ArgumentException("Input file required");
            if (string.IsNullOrEmpty(model.OutputFile)) throw new ArgumentException("Output file required");
            if (model.Speed <= 0) throw new ArgumentException("Speed must be greater than 0");

            double setptsValue = 1.0 / model.Speed;
            string videoFilter = $"setpts={setptsValue.ToString(CultureInfo.InvariantCulture)}*PTS";

            string audioFilter;
            double speed = model.Speed;
            if (speed < 0.5 || speed > 2.0)
            {
                int n = 0;
                double s = speed;
                string chain = "";
                if (s > 2.0)
                {
                    while (s > 2.0)
                    {
                        chain += "atempo=2.0,";
                        s /= 2.0;
                        n++;
                    }
                    chain += $"atempo={s.ToString(CultureInfo.InvariantCulture)}";
                }
                else if (s < 0.5)
                {
                    while (s < 0.5)
                    {
                        chain += "atempo=0.5,";
                        s /= 0.5;
                        n++;
                    }
                    chain += $"atempo={s.ToString(CultureInfo.InvariantCulture)}";
                }
                else
                {
                    chain = $"atempo={s.ToString(CultureInfo.InvariantCulture)}";
                }
                audioFilter = chain;
            }
            else
            {
                audioFilter = $"atempo={speed.ToString(CultureInfo.InvariantCulture)}";
            }

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"{videoFilter}\"")
                .AddOption($"-filter:a \"{audioFilter}\"")
                .SetOutput(model.OutputFile, isFrameOutput: false);

            return await RunAsync();
        }
    }
}