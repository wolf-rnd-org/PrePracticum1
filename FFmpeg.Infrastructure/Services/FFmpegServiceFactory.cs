using Ffmpeg.Command;
using Ffmpeg.Command.Commands;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace FFmpeg.Infrastructure.Services
{
    public interface IFFmpegServiceFactory
    {
        ICommand<WatermarkModel> CreateWatermarkCommand();
        ICommand<ConvertAudioModel> CreateConvertAudioCommand();
        ICommand<ConvertVideoToGifModel> CreateConvertVideoToGifCommand();
        ICommand<BlackAndWhiteModel> CreateBlackAndWhiteCommand();
        ICommand<GreenScreenModel> CreateGreenScreenCommand();
        ICommand<BorderModel> CreateBorderCommand();
        ICommand<CropModel> CreateCropCommand();
        ICommand<ExtractFrameInput> CreateExtractFrameCommand();
        ICommand<TimestampOverlayModel> CreateTimestampOverlayCommand();
        ICommand<ChangeSpeedModel> CreateChangeSpeedCommand();
        ICommand<ReverseVideoModel> CreateReverseVideoCommand();
        ICommand<AudioEffectModel> CreateAudioEffectCommand();
        ICommand<FadeEffectModel> CreateFadeEffectCommand();
        ICommand<ReduceQualityModel> CreateReduceQualityCommand();
        ICommand<ConvertFormatModel> CreateConvertFormatCommand();
        ICommand<MergeTwoFilesModel> CreateMergeTwoFilesCommand();
        ICommand<PreviewModel> CreatePreviewCommand();
        ICommand<AnimatedTextModel> CreateAnimatedTextCommand();
        ICommand<SubtitleTranslationModel> CreateSubtitleTranslationCommand();
        ICommand<ChangeResolutionModel> CreateChangeResolutionCommand();
        ICommand<RemoveAudioModel> CreateRemoveAudioCommand();

        ICommand<RotateModel> CreateRotateCommand();
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
            => new WatermarkCommand(_executor, _commandBuilder);

        public ICommand<ConvertAudioModel> CreateConvertAudioCommand()
            => new ConvertAudioCommand(_executor, _commandBuilder);

        public ICommand<ConvertVideoToGifModel> CreateConvertVideoToGifCommand()
            => new ConvertVideoToGifCommand(_executor, _commandBuilder);

        public ICommand<BlackAndWhiteModel> CreateBlackAndWhiteCommand()
            => new BlackAndWhiteCommand(_executor, _commandBuilder);

        public ICommand<GreenScreenModel> CreateGreenScreenCommand()
            => new GreenScreenCommand(_executor, _commandBuilder);

        public ICommand<BorderModel> CreateBorderCommand()
            => new BorderCommand(_executor, _commandBuilder);

        public ICommand<CropModel> CreateCropCommand()
            => new CropCommand(_executor, _commandBuilder);

        public ICommand<ExtractFrameInput> CreateExtractFrameCommand()
            => new ExtractFrameCommand(_executor, _commandBuilder);

        public ICommand<TimestampOverlayModel> CreateTimestampOverlayCommand()
            => new TimestampOverlayCommand(_executor, _commandBuilder);

        public ICommand<ChangeSpeedModel> CreateChangeSpeedCommand()
            => new ChangeSpeedCommand(_executor, _commandBuilder);

        public ICommand<ReverseVideoModel> CreateReverseVideoCommand()
            => new ReverseVideoCommand(_executor, _commandBuilder);

        public ICommand<AudioEffectModel> CreateAudioEffectCommand()
            => new AudioEffectCommand(_executor, _commandBuilder);

        public ICommand<FadeEffectModel> CreateFadeEffectCommand()
            => new FadeEffectCommand(_executor, _commandBuilder);

        public ICommand<ReduceQualityModel> CreateReduceQualityCommand()
            => new ReduceQualityCommand(_executor, _commandBuilder);

        public ICommand<ConvertFormatModel> CreateConvertFormatCommand()
            => new ConvertFormatCommand(_executor, _commandBuilder);

        public ICommand<MergeTwoFilesModel> CreateMergeTwoFilesCommand()
            => new MergeTwoFilesCommand(_executor, _commandBuilder);

        public ICommand<PreviewModel> CreatePreviewCommand()
            => new PreviewCommand(_executor, _commandBuilder);

        public ICommand<AnimatedTextModel> CreateAnimatedTextCommand()
            => new AnimatedTextCommand(_executor, _commandBuilder);

        public ICommand<SubtitleTranslationModel> CreateSubtitleTranslationCommand()
            => new SubtitleTranslationCommand(_executor, _commandBuilder);

        public ICommand<ChangeResolutionModel> CreateChangeResolutionCommand()
            => new ChangeResolutionCommand(_executor, _commandBuilder);

        public ICommand<RemoveAudioModel> CreateRemoveAudioCommand()
            => new RemoveAudioCommand(_executor, _commandBuilder);

        public ICommand<RotateModel> CreateRotateCommand()
            => new RotateCommand(_executor, _commandBuilder);
    }
}