using Ffmpeg.Command;
using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Services
{
    public interface IFFmpegServiceFactory
    {
        ICommand<WatermarkModel> CreateWatermarkCommand();
        ICommand<GreenScreenModel> CreateGreenScreenCommand();
        ICommand<BorderModel> CreateBorderCommand();
<<<<<<< HEAD
        
        ICommand<CropModel> CreateCropCommand();
=======
        ICommand<TimestampOverlayModel> CreateTimestampOverlayCommand();
>>>>>>> 75a01a3733b08d2858b99755159877151a4be878
        ICommand<ChangeSpeedModel> CreateChangeSpeedCommand();

        ICommand<ReverseVideoModel> CreateReverseVideoCommand();
        ICommand<AudioEffectModel> CreateAudioEffectCommand();
        ICommand<ConvertAudioModel> CreateConvertAudioCommand();
        ICommand<FadeEffectModel> CreateFadeEffectCommand();
<<<<<<< HEAD

=======
        ICommand<PreviewModel> CreatePreviewCommand();
>>>>>>> 75a01a3733b08d2858b99755159877151a4be878
    }

    public class FFmpegServiceFactory : IFFmpegServiceFactory
    {
        private readonly FFmpegExecutor _executor;
        private readonly ICommandBuilder _commandBuilder;
        public FFmpegServiceFactory(IConfiguration configuration, ILogger logger = null)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDirectory, "external", "ffmpeg.exe");

            bool logOutput = bool.TryParse(configuration["FFmpeg:LogOutput"], out bool log) && log;

            _executor = new FFmpegExecutor(ffmpegPath, logOutput, logger);
            _commandBuilder = new CommandBuilder(configuration);
        }
        public ICommand<WatermarkModel> CreateWatermarkCommand()
        {
            return new WatermarkCommand(_executor, _commandBuilder);
        }
        public ICommand<FadeEffectModel> CreateFadeEffectCommand()
        {
            return new FadeEffectCommand(_executor, _commandBuilder);
        }
        public ICommand<ReverseVideoModel> CreateReverseVideoCommand()
        {
            return new ReverseVideoCommand(_executor, _commandBuilder);
        }
        public ICommand<GreenScreenModel> CreateGreenScreenCommand()
        {
            return new GreenScreenCommand(_executor, _commandBuilder);
        }
        public ICommand<BorderModel> CreateBorderCommand()
        {
            return new BorderCommand(_executor, _commandBuilder);
        }
<<<<<<< HEAD
        public ICommand<CropModel> CreateCropCommand()
        {
            return new CropCommand(_executor, _commandBuilder);
        }


=======
        public ICommand<TimestampOverlayModel> CreateTimestampOverlayCommand()
        {
            return new TimestampOverlayCommand(_executor, _commandBuilder);
        }
>>>>>>> 75a01a3733b08d2858b99755159877151a4be878
        public ICommand<ChangeSpeedModel> CreateChangeSpeedCommand()
        {
            return new ChangeSpeedCommand(_executor, _commandBuilder);
        }
        public ICommand<AudioEffectModel> CreateAudioEffectCommand()
        {
            return new AudioEffectCommand(_executor, _commandBuilder);
        } 
        public ICommand<ConvertAudioModel> CreateConvertAudioCommand()
        {
            return new ConvertAudioCommand(_executor, _commandBuilder);

        }
        public ICommand<PreviewModel> CreatePreviewCommand()
        {
            return new PreviewCommand(_executor, _commandBuilder);
        }
    }
}
