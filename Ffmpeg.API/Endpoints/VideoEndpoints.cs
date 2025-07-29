using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.API.DTOs;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        private const int MaxUploadSize = 104857600;

        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/greenscreen", ApplyGreenScreen)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize))
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            app.MapPost("/api/video/cut", CutVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/fadein", AddFadeInEffect)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/reverse", ReverseVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            app.MapPost("/api/video/border", AddBorder)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/audio/convert", ConvertAudio)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(52428800)); // 50MB

            app.MapPost("/api/video/timestamp", AddTimestampOverlay)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/change-speed", ChangeVideoSpeed)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/audio/effect", ApplyAudioEffect)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB
        }

        private static async Task<IResult> CutVideo(HttpContext context, [FromForm] CutVideoDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var videoService = context.RequestServices.GetRequiredService<VideoCuttingService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.VideoFile == null || string.IsNullOrWhiteSpace(dto.StartTime) || string.IsNullOrWhiteSpace(dto.EndTime))
                return Results.BadRequest("Missing required fields");
            try
            {
                string inputFile = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFile = await fileService.GenerateUniqueFileNameAsync(extension);
                var model = new CutVideoModel
                {
                    InputFile = inputFile,
                    OutputFile = outputFile,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime
                };
                bool success = await videoService.CutVideoAsync(model);

                if (!success)
                {
                    logger.LogError("Cutting video failed");
                    return Results.Problem("Failed to cut video", statusCode: 500);
                }

                byte[] output = await fileService.GetOutputFileAsync(outputFile);
                await fileService.CleanupTempFilesAsync(new[] { inputFile, outputFile });

                return Results.File(output, "video/mp4", "cut_" + dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cutting video");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddFadeInEffect(HttpContext context, [FromForm] FadeEffectDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                    return Results.BadRequest("Video file is required");

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = Path.Combine(Path.GetDirectoryName(videoFileName)!, await fileService.GenerateUniqueFileNameAsync(extension));

                List<string> filesToCleanup = new() { videoFileName, outputFileName };

                var command = ffmpegService.CreateFadeEffectCommand();
                var result = await command.ExecuteAsync(new FadeEffectModel
                {
                    InputFilePath = videoFileName,
                    OutputFilePath = outputFileName,
                    FadeInDurationSeconds = dto.FadeInDurationSeconds
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to add fade effect: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.OutputFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddFadeInEffect endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ReverseVideo(HttpContext context, [FromForm] ReverseVideoDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
                List<string> filesToCleanup = new() { videoFileName, outputFileName };

                var command = ffmpegService.CreateReverseVideoCommand();
                var result = await command.ExecuteAsync(new ReverseVideoModel
                {
                    InputFile = videoFileName,
                    OutputFile = outputFileName
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to reverse file: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}", ex.Message);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddWatermark(HttpContext context, [FromForm] WatermarkDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.WatermarkFile == null)
                    return Results.BadRequest("Video file and watermark file are required");

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                var filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };

                var command = ffmpegService.CreateWatermarkCommand();
                var result = await command.ExecuteAsync(new WatermarkModel
                {
                    InputFile = videoFileName,
                    WatermarkFile = watermarkFileName,
                    OutputFile = outputFileName,
                    XPosition = dto.XPosition,
                    YPosition = dto.YPosition,
                    IsVideo = true,
                    VideoCodec = "libx264"
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to add watermark: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddWatermark endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ApplyGreenScreen(HttpContext context, [FromForm] GreenScreenDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.BackgroundFile == null)
                    return Results.BadRequest("Video file and background file are required");

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string backgroundFileName = await fileService.SaveUploadedFileAsync(dto.BackgroundFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                var filesToCleanup = new List<string> { videoFileName, backgroundFileName, outputFileName };

                var command = ffmpegService.CreateGreenScreenCommand();
                var result = await command.ExecuteAsync(new GreenScreenModel
                {
                    InputFile = videoFileName,
                    BackgroundFile = backgroundFileName,
                    OutputFile = outputFileName,
                    VideoCodec = "libx264"
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg GreenScreen failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to process green screen: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GreenScreen endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddBorder(HttpContext context, [FromForm] BorderDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                    return Results.BadRequest("Video file is required");

                if (string.IsNullOrWhiteSpace(dto.FrameColor))
                    dto.FrameColor = "blue";

                int borderSize = int.TryParse(dto.BorderSize, out var parsed) && parsed > 0 ? parsed : 20;

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new() { inputFileName, outputFileName };

                var command = ffmpegService.CreateBorderCommand();
                var result = await command.ExecuteAsync(new BorderModel
                {
                    InputFile = inputFileName,
                    OutputFile = outputFileName,
                    BorderSize = borderSize,
                    FrameColor = dto.FrameColor,
                    IsVideo = true,
                    VideoCodec = "libx264"
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg BorderCommand failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to add border: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddBorder endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ConvertAudio(HttpContext context, [FromForm] ConvertAudioDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.AudioFile == null || string.IsNullOrWhiteSpace(dto.OutputFormat))
                return Results.BadRequest("Audio file and output format are required");

            string inputFileName = await fileService.SaveUploadedFileAsync(dto.AudioFile);
            string extension = "." + dto.OutputFormat.Trim().ToLower();
            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
            List<string> filesToCleanup = new() { inputFileName, outputFileName };

            try
            {
                var command = ffmpegService.CreateConvertAudioCommand();
                var result = await command.ExecuteAsync(new ConvertAudioModel
                {
                    InputFile = inputFileName,
                    OutputFile = outputFileName
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg audio convert failed: {Error}", result.ErrorMessage);
                    return Results.Problem("Conversion failed: " + result.ErrorMessage);
                }

                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "application/octet-stream", Path.GetFileName(outputFileName));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error converting audio");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddTimestampOverlay(HttpContext context, [FromForm] TimestampOverlayDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            try
            {
                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                    return Results.BadRequest("Video file is required");

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new() { inputFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateTimestampOverlayCommand();
                    var result = await command.ExecuteAsync(new TimestampOverlayModel
                    {
                        InputFilePath = inputFileName,
                        OutputFilePath = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("TimestampOverlayCommand failed: {Error}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to add timestamp overlay: " + result.ErrorMessage);
                    }

                    var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing timestamp overlay request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in AddTimestampOverlay");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ChangeVideoSpeed(HttpContext context, [FromForm] ChangeSpeedDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.VideoFile == null || dto.Speed <= 0 || string.IsNullOrWhiteSpace(dto.OutputFileName))
                return Results.BadRequest("Video file, speed (>0), and output file name are required");

            string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
            string extension = Path.GetExtension(dto.OutputFileName);
            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
            List<string> filesToCleanup = new() { inputFileName, outputFileName };

            try
            {
                var command = ffmpegService.CreateChangeSpeedCommand();
                var result = await command.ExecuteAsync(new ChangeSpeedModel
                {
                    InputFile = inputFileName,
                    OutputFile = outputFileName,
                    Speed = dto.Speed
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg ChangeVideoSpeedCommand failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to change video speed: " + result.ErrorMessage, statusCode: 500);
                }

                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.File(fileBytes, "video/mp4", dto.OutputFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error changing video speed");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ApplyAudioEffect(HttpContext context, [FromForm] AudioEffectDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                    return Results.BadRequest("Video file is required");

                if (string.IsNullOrWhiteSpace(dto.EffectType))
                    return Results.BadRequest("Effect type is required");

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = string.IsNullOrWhiteSpace(dto.OutputFileName)
                    ? await fileService.GenerateUniqueFileNameAsync(extension)
                    : dto.OutputFileName;

                List<string> filesToCleanup = new() { inputFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateAudioEffectCommand();
                    var result = await command.ExecuteAsync(new AudioEffectModel
                    {
                        InputFile = inputFileName,
                        OutputFile = outputFileName,
                        EffectType = dto.EffectType,
                        Duration = dto.Duration
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg audio effect failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to apply audio effect: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error applying audio effect");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ApplyAudioEffect endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }
    }
}
