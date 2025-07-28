using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using FFmpeg.Infrastructure.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class AudioEffectCommand : BaseCommand, ICommand<AudioEffectModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public AudioEffectCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(AudioEffectModel model)
        {
            string audioFilter = BuildAudioFilter(model);
            
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .SetAudioFilter(audioFilter)
                .SetOutput(model.OutputFile);

            return await RunAsync();
        }

        private string BuildAudioFilter(AudioEffectModel model)
        {
            return model.EffectType.ToLower() switch
            {
                "echo" => BuildEchoFilter(model),
                "background" => BuildBackgroundFilter(model),
                _ => throw new ArgumentException($"Unsupported effect type: {model.EffectType}")
            };
        }

        private string BuildEchoFilter(AudioEffectModel model)
        {
            // Default echo parameters: aecho=0.8:0.88:60:0.4
            // Parameters: in_gain:out_gain:delays:decays
            double inGain = model.EffectParameters.ContainsKey("inGain") ? Convert.ToDouble(model.EffectParameters["inGain"]) : 0.8;
            double outGain = model.EffectParameters.ContainsKey("outGain") ? Convert.ToDouble(model.EffectParameters["outGain"]) : 0.88;
            double delays = model.EffectParameters.ContainsKey("delays") ? Convert.ToDouble(model.EffectParameters["delays"]) : 60;
            double decays = model.EffectParameters.ContainsKey("decays") ? Convert.ToDouble(model.EffectParameters["decays"]) : 0.4;
            
            return $"aecho={inGain}:{outGain}:{delays}:{decays}";
        }

        private string BuildBackgroundFilter(AudioEffectModel model)
        {
            // For background effect, we'll use a simple volume adjustment
            // This could be extended to add actual background music mixing
            double volume = model.EffectParameters.ContainsKey("volume") ? Convert.ToDouble(model.EffectParameters["volume"]) : 0.5;
            
            return $"volume={volume}";
        }
    }
} 