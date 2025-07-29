using Ffmpeg.Command;
using Ffmpeg.Command.Commands;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
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

        
        ICommand<CropModel> CreateCropCommand();
        ICommand<ExtractFrameInput> CreateExtractFrameCommand();     


        ICommand<TimestampOverlayModel> CreateTimestampOverlayCommand();

        ICommand<ChangeSpeedModel> CreateChangeSpeedCommand();

        ICommand<ReverseVideoModel> CreateReverseVideoCommand();
        ICommand<AudioEffectModel> CreateAudioEffectCommand();
        ICommand<ConvertAudioModel> CreateConvertAudioCommand();
        ICommand<FadeEffectModel> CreateFadeEffectCommand();
        ICommand<MergeTwoFilesModel> CreateMergeTwoFilesCommand();

        ICommand<PreviewModel> CreatePreviewCommand();
    }


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

        public ICommand<ExtractFrameInput> CreateExtractFrameCommand()
        {
            return new ExtractFrameCommand(_executor, _commandBuilder);
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

        public ICommand<CropModel> CreateCropCommand()
        {
            return new CropCommand(_executor, _commandBuilder);
        }






        public ICommand<TimestampOverlayModel> CreateTimestampOverlayCommand()
        {
            return new TimestampOverlayCommand(_executor, _commandBuilder);
        }


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

        public ICommand<MergeTwoFilesModel> CreateMergeTwoFilesCommand()
        {
            return new MergeTwoFilesCommand(_executor, _commandBuilder);
        } 
    }

