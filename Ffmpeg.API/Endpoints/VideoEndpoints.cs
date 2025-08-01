//using FFmpeg.API.DTOs;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Features;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using FFmpeg.Core.Interfaces;
//using FFmpeg.Core.Models;
//using FFmpeg.Infrastructure.Services;

//namespace FFmpeg.API.Endpoints
//{
//    public static class VideoEndpoints
//    {
//        private const int MaxUploadSize = 104857600; // 100 MB
//        private const int MaxUploadSize50Mb = 50 * 1024 * 1024;


//        public static void MapEndpoints(this WebApplication app)
//        {
//            app.MapPost("/api/video/replace-audio", ReplaceAudio)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/watermark", AddWatermark)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/audio/convert", ConvertAudio)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/convertVideoToGif", ConvertVideoToGif)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/greenscreen", ApplyGreenScreen)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/cut", CutVideo)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/fadein", AddFadeInEffect)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/reverse", ReverseVideo)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/border", AddBorder)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/extract-frame", ExtractFrame)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/rotate", RotateVideo)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/timestamp", AddTimestampOverlay)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/change-speed", ChangeVideoSpeed)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/audio/effect", ApplyAudioEffect)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/convertformat", ConvertVideoFormat)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/reduce-quality", ReduceQuality)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/crop", CropVideo)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/mergefiles", MergeTwoFiles)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/thumbnail", CreatePreview)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/animatedtext", AddAnimatedText)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/subtitles", AddSubtitles)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/change-resolution", ChangeResolution)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

//            app.MapPost("/api/video/remove-audio", RemoveAudio)
//                .DisableAntiforgery()
//                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));
//        }

//        private static async Task<IResult> ReplaceAudio([FromForm] ReplaceAudioDto dto, HttpContext context)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var command = context.RequestServices.GetRequiredService<AudioReplaceCommand>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            string videoTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(dto.VideoFile.FileName));
//            string audioTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(dto.AudioFile.FileName));
//            string outputTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");

//            try
//            {
//                await using (var videoStream = new FileStream(videoTempFile, FileMode.Create))
//                    await dto.VideoFile.CopyToAsync(videoStream);

//                await using (var audioStream = new FileStream(audioTempFile, FileMode.Create))
//                    await dto.AudioFile.CopyToAsync(audioStream);

//                var request = new AudioReplaceModel
//                {
//                    VideoFile = videoTempFile,
//                    NewAudioFile = audioTempFile,
//                    OutputFile = outputTempFile
//                };

//                await command.ExecuteAsync(request);

//                byte[] outputBytes = await File.ReadAllBytesAsync(outputTempFile);
//                return Results.File(outputBytes, "video/mp4", "output_with_audio.mp4");
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in ReplaceAudio endpoint");
//                return Results.Problem("Failed to replace audio: " + ex.Message, statusCode: 500);
//            }
//            finally
//            {
//                if (File.Exists(videoTempFile)) File.Delete(videoTempFile);
//                if (File.Exists(audioTempFile)) File.Delete(audioTempFile);
//                if (File.Exists(outputTempFile)) File.Delete(outputTempFile);
//            }
//        }


//        private static async Task<IResult> AddWatermark(
//            HttpContext context,
//            [FromForm] WatermarkDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//            try
//            {
//                if (dto.VideoFile == null || dto.WatermarkFile == null)
//                    return Results.BadRequest("Video file and watermark file are required");
//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                var filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };
//                try
//                {
//                    var command = ffmpegService.CreateWatermarkCommand();
//                    var result = await command.ExecuteAsync(new WatermarkModel
//                    {
//                        InputFile = videoFileName,
//                        WatermarkFile = watermarkFileName,
//                        OutputFile = outputFileName,
//                        XPosition = dto.XPosition,
//                        YPosition = dto.YPosition,
//                        IsVideo = true,
//                        VideoCodec = "libx264"
//                    });
//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to add watermark: " + result.ErrorMessage, statusCode: 500);
//                    }
//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error processing watermark request");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in AddWatermark endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ConvertAudio(HttpContext context, [FromForm] ConvertAudioDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.AudioFile == null || string.IsNullOrWhiteSpace(dto.OutputFormat))
//                return Results.BadRequest("Audio file and output format are required");

//            string inputFileName = await fileService.SaveUploadedFileAsync(dto.AudioFile);
//            string extension = "." + dto.OutputFormat.Trim().ToLower();
//            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//            List<string> filesToCleanup = new() { inputFileName, outputFileName };

//            try
//            {
//                var command = ffmpegService.CreateConvertAudioCommand();
//                var result = await command.ExecuteAsync(new ConvertAudioModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName
//                });
//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg audio convert failed: {Error}", result.ErrorMessage);
//                    return Results.Problem("Conversion failed: " + result.ErrorMessage);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "application/octet-stream", Path.GetFileName(outputFileName));
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error converting audio");
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ConvertVideoToGif(
//            HttpContext context,
//            [FromForm] ConvertVideoToGifDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//            if (dto.VideoFile == null)
//            {
//                return Results.BadRequest("Video file is required");
//            }
//            string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//            string outputFileName = await fileService.GenerateUniqueFileNameAsync(".gif");
//            var filesToCleanup = new List<string> { inputFileName, outputFileName };
//            try
//            {
//                var command = ffmpegService.CreateConvertVideoToGifCommand();
//                var result = await command.ExecuteAsync(new ConvertVideoToGifModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName,
//                    Fps = dto.Fps,
//                    Width = dto.Width
//                });
//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg video-to-GIF failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to convert to GIF: " + result.ErrorMessage, statusCode: 500);
//                }
//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.File(fileBytes, "image/gif", Path.GetFileName(outputFileName));
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error converting video to GIF");
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ApplyGreenScreen(HttpContext context, [FromForm] GreenScreenDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {

//                if (dto.VideoFile == null || dto.BackgroundFile == null)
//                    return Results.BadRequest("Video file and background file are required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string backgroundFileName = await fileService.SaveUploadedFileAsync(dto.BackgroundFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

//                var filesToCleanup = new List<string> { videoFileName, backgroundFileName, outputFileName };

//                var command = ffmpegService.CreateGreenScreenCommand();
//                var result = await command.ExecuteAsync(new GreenScreenModel
//                {
//                    InputFile = videoFileName,
//                    BackgroundFile = backgroundFileName,
//                    OutputFile = outputFileName,
//                    VideoCodec = "libx264"
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg GreenScreen failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to process green screen: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in GreenScreen endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> CutVideo(HttpContext context, [FromForm] CutVideoDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var videoService = context.RequestServices.GetRequiredService<VideoCuttingService>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.VideoFile == null || string.IsNullOrWhiteSpace(dto.StartTime) || string.IsNullOrWhiteSpace(dto.EndTime))
//                return Results.BadRequest("Missing required fields");
//            try
//            {
//                string inputFile = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFile = await fileService.GenerateUniqueFileNameAsync(extension);
//                var model = new CutVideoModel
//                {
//                    InputFile = inputFile,
//                    OutputFile = outputFile,
//                    StartTime = dto.StartTime,
//                    EndTime = dto.EndTime
//                };
//                bool success = await videoService.CutVideoAsync(model);

//                if (!success)
//                {
//                    logger.LogError("Cutting video failed");
//                    return Results.Problem("Failed to cut video", statusCode: 500);
//                }

//                byte[] output = await fileService.GetOutputFileAsync(outputFile);
//                await fileService.CleanupTempFilesAsync(new[] { inputFile, outputFile });

//                return Results.File(output, "video/mp4", "cut_" + dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in CutVideo endpoint");

//                logger.LogError(ex, "Error in reverseVideo endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ExtractFrame(HttpContext context, [FromForm] ExtractFrameDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null)
//                    return Results.BadRequest("Video file is required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".png");

//                var filesToCleanup = new List<string> { videoFileName, outputFileName };

//                var command = ffmpegService.CreateExtractFrameCommand();
//                var result = await command.ExecuteAsync(new ExtractFrameInput
//                {
//                    InputFile = videoFileName,
//                    TimeSpan = dto.TimeSpan,
//                    OutputImagePath = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg extract frame failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to extract frame: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "image/png", outputFileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error processing extract frame request");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ConvertAudio(HttpContext context, [FromForm] ConvertAudioDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.AudioFile == null || string.IsNullOrWhiteSpace(dto.OutputFormat))
//            {
//                return Results.BadRequest("Audio file and output format are required");
//            }

//            string inputFileName = await fileService.SaveUploadedFileAsync(dto.AudioFile);
//            string extension = "." + dto.OutputFormat.Trim().ToLower();
//            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//            List<string> filesToCleanup = new() { inputFileName, outputFileName };

//            try
//            {
//                var command = ffmpegService.CreateConvertAudioCommand();
//                var result = await command.ExecuteAsync(new ConvertAudioModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg audio convert failed: {Error}", result.ErrorMessage);
//                    return Results.Problem("Conversion failed: " + result.ErrorMessage);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "application/octet-stream", Path.GetFileName(outputFileName));
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error converting audio");
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ApplyGreenScreen(
//            HttpContext context,
//            [FromForm] GreenScreenDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null || dto.BackgroundFile == null)
//                    return Results.BadRequest("Video file and background file are required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string backgroundFileName = await fileService.SaveUploadedFileAsync(dto.BackgroundFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                var filesToCleanup = new List<string> { videoFileName, backgroundFileName, outputFileName };

//                try
//                {
//                    var command = ffmpegService.CreateGreenScreenCommand();
//                    var result = await command.ExecuteAsync(new GreenScreenModel
//                    {
//                        InputFile = videoFileName,
//                        BackgroundFile = backgroundFileName,
//                        OutputFile = outputFileName,
//                        VideoCodec = "libx264"
//                    });

//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg GreenScreen failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to process green screen: " + result.ErrorMessage, statusCode: 500);
//                    }

//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error processing green screen");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in GreenScreen endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }

//        }
//        private static async Task<IResult> AddFadeInEffect(HttpContext context, [FromForm] FadeEffectDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null)
//                    return Results.BadRequest("Video file is required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = Path.Combine(Path.GetDirectoryName(videoFileName)!, await fileService.GenerateUniqueFileNameAsync(extension));

//                List<string> filesToCleanup = new() { videoFileName, outputFileName };

//                var command = ffmpegService.CreateFadeEffectCommand();
//                var result = await command.ExecuteAsync(new FadeEffectModel
//                {
//                    InputFilePath = videoFileName,
//                    OutputFilePath = outputFileName,
//                    FadeInDurationSeconds = dto.FadeInDurationSeconds
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to add fade effect: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.OutputFileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in AddFadeInEffect endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> AddFadeInEffect(
//            HttpContext context,
//            [FromForm] FadeEffectDto dto)

//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null)
//                    return Results.BadRequest("Video file is required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = Path.Combine(Path.GetDirectoryName(videoFileName)!, await fileService.GenerateUniqueFileNameAsync(extension));

//                List<string> filesToCleanup = new() { videoFileName, outputFileName };

//                var command = ffmpegService.CreateFadeEffectCommand();
//                var result = await command.ExecuteAsync(new FadeEffectModel
//                {
//                    InputFilePath = videoFileName,
//                    OutputFilePath = outputFileName,
//                    FadeInDurationSeconds = dto.FadeInDurationSeconds
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to add fade effect: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.OutputFileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in AddFadeInEffect endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ReverseVideo(HttpContext context, [FromForm] ReverseVideoDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                List<string> filesToCleanup = new() { videoFileName, outputFileName };

//                var command = ffmpegService.CreateReverseVideoCommand();
//                var result = await command.ExecuteAsync(new ReverseVideoModel
//                {
//                    InputFile = videoFileName,
//                    OutputFile = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to reverse file: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}", ex.Message);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> AddSubtitles(
//    HttpContext context,
//    [FromForm] SubtitleTranslationDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//            try
//            {
//                if (dto.VideoFile == null || dto.SubtitleFile == null)
//                    return Results.BadRequest("Video file and subtitle file are required");
//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string subtitleFileName = await fileService.SaveUploadedFileAsync(dto.SubtitleFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                var filesToCleanup = new List<string> { videoFileName, subtitleFileName, outputFileName };
//                try
//                {
//                    var command = ffmpegService.CreateSubtitleTranslationCommand();
//                    var result = await command.ExecuteAsync(new SubtitleTranslationModel
//                    {
//                        InputFile = videoFileName,
//                        SubtitleFile = subtitleFileName,
//                        OutputFile = outputFileName
//                    });
//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to add subtitles: " + result.ErrorMessage, statusCode: 500);
//                    }
//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error processing subtitle translation request");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in AddSubtitles endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }
//        private static async Task<IResult> AddBorder(HttpContext context, [FromForm] BorderDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
//                    return Results.BadRequest("Video file is required");

//                if (string.IsNullOrWhiteSpace(dto.FrameColor))
//                    dto.FrameColor = "blue";

//                int borderSize = int.TryParse(dto.BorderSize, out var parsed) && parsed > 0 ? parsed : 20;

//                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

//                List<string> filesToCleanup = new() { inputFileName, outputFileName };

//                var command = ffmpegService.CreateBorderCommand();
//                var result = await command.ExecuteAsync(new BorderModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName,
//                    BorderSize = borderSize,
//                    FrameColor = dto.FrameColor,
//                    IsVideo = true,
//                    VideoCodec = "libx264"
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg BorderCommand failed: {ErrorMessage}, Command: {Command}", result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to add border: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in AddBorder endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }
//        private static async Task<IResult> ExtractFrame(HttpContext context, [FromForm] ExtractFrameDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null)
//                    return Results.BadRequest("Video file is required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".png");

//                var filesToCleanup = new List<string> { videoFileName, outputFileName };

//                var command = ffmpegService.CreateExtractFrameCommand();
//                var result = await command.ExecuteAsync(new ExtractFrameInput
//                {
//                    InputFile = videoFileName,
//                    TimeSpan = dto.TimeSpan,
//                    OutputImagePath = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg extract frame failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to extract frame: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "image/png", outputFileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error processing extract frame request");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ConvertAudio(HttpContext context, [FromForm] ConvertAudioDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.AudioFile == null || string.IsNullOrWhiteSpace(dto.OutputFormat))
//            {
//                return Results.BadRequest("Audio file and output format are required");
//            }

//            string inputFileName = await fileService.SaveUploadedFileAsync(dto.AudioFile);
//            string extension = "." + dto.OutputFormat.Trim().ToLower();
//            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//            List<string> filesToCleanup = new() { inputFileName, outputFileName };

//            try
//            {
//                var command = ffmpegService.CreateConvertAudioCommand();
//                var result = await command.ExecuteAsync(new ConvertAudioModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName
//                });
//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg audio convert failed: {Error}", result.ErrorMessage);
//                    return Results.Problem("Conversion failed: " + result.ErrorMessage);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "application/octet-stream", Path.GetFileName(outputFileName));
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error converting audio");
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> RotateVideo(HttpContext context, [FromForm] RotateDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null)
//                    return Results.BadRequest("Video file is required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

//                var filesToCleanup = new List<string> { videoFileName, outputFileName };

//                try
//                {
//                    var command = ffmpegService.RotationVideoCommand();
//                    var result = await command.ExecuteAsync(new RotateModel
//                    {
//                        InputFile = videoFileName,
//                        OutputFile = outputFileName,
//                        RotationAngle = dto.RotationAngle,
//                        IsVideo = true,
//                        VideoCodec = "libx264"
//                    });

//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg rotation failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to rotate video: " + result.ErrorMessage, statusCode: 500);
//                    }

//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error during video rotation");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in RotateVideo endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }


//        private static async Task<IResult> AddTimestampOverlay(HttpContext context, [FromForm] TimestampOverlayDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
//                    return Results.BadRequest("Video file is required");

//                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

//                List<string> filesToCleanup = new() { inputFileName, outputFileName };

//                var command = ffmpegService.CreateTimestampOverlayCommand();
//                var result = await command.ExecuteAsync(new TimestampOverlayModel
//                {
//                    InputFilePath = inputFileName,
//                    OutputFilePath = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("TimestampOverlayCommand failed: {Error}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to add timestamp overlay: " + result.ErrorMessage);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Unexpected error in AddTimestampOverlay");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> RotateVideo(
//    HttpContext context,
//    [FromForm] RotateDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null)
//                    return Results.BadRequest("Video file is required");

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

//                var filesToCleanup = new List<string> { videoFileName, outputFileName };

//                try
//                {
//                    var command = ffmpegService.RotationVideoCommand();
//                    var result = await command.ExecuteAsync(new RotateModel
//                    {
//                        InputFile = videoFileName,
//                        OutputFile = outputFileName,
//                        RotationAngle = dto.RotationAngle,
//                        IsVideo = true,
//                        VideoCodec = "libx264"
//                    });

//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg rotation failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to rotate video: " + result.ErrorMessage, statusCode: 500);
//                    }

//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error during video rotation");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in RotateVideo endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ApplyAudioEffect(
//    HttpContext context,
//    [FromForm] AudioEffectDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
//                    return Results.BadRequest("Video file is required");

//                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

//                List<string> filesToCleanup = new() { inputFileName, outputFileName };

//                var command = ffmpegService.CreateAudioEffectCommand();
//                var result = await command.ExecuteAsync(new AudioEffectModel
//                {
//                    InputFilePath = inputFileName,
//                    OutputFilePath = outputFileName,
//                    EffectType = dto.EffectType // הנחה שיש שדה כזה ב-DTO
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("AudioEffectCommand failed: {Error}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to apply audio effect: " + result.ErrorMessage);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Unexpected error in ApplyAudioEffect");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> AddTimestampOverlay(
//            HttpContext context,
//            [FromForm] TimestampOverlayDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null || dto.VideoFile.Length == 0)
//                    return Results.BadRequest("Video file is required");

//                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

//                List<string> filesToCleanup = new() { inputFileName, outputFileName };

//                var command = ffmpegService.CreateTimestampOverlayCommand();
//                var result = await command.ExecuteAsync(new TimestampOverlayModel
//                {
//                    InputFilePath = inputFileName,
//                    OutputFilePath = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("TimestampOverlayCommand failed: {Error}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to add timestamp overlay: " + result.ErrorMessage);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Unexpected error in AddTimestampOverlay");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ChangeVideoSpeed(HttpContext context, [FromForm] ChangeSpeedDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.VideoFile == null || dto.Speed <= 0 || string.IsNullOrWhiteSpace(dto.OutputFileName))
//                return Results.BadRequest("Video file, speed (>0), and output file name are required");

//            string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//            string extension = Path.GetExtension(dto.VideoFile.FileName);
//            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//            List<string> filesToCleanup = new() { inputFileName, outputFileName };

//            try
//            {
//                var command = ffmpegService.CreateChangeSpeedCommand();
//                var result = await command.ExecuteAsync(new ChangeSpeedModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName,
//                    Speed = dto.Speed
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("ChangeVideoSpeedCommand failed: {Error}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to change video speed: " + result.ErrorMessage);
//                    logger.LogError("FFmpeg ChangeVideoSpeedCommand failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to change video speed: " + result.ErrorMessage, statusCode: 500);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.File(fileBytes, "video/mp4", dto.OutputFileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Unexpected error in ChangeVideoSpeed");

//                logger.LogError(ex, "Error changing video speed");
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> CropVideo(HttpContext context, [FromForm] CropDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.VideoFile == null || dto.VideoFile.Length == 0)
//                return Results.BadRequest("Video file is required");

//            try
//            {
//                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                List<string> filesToCleanup = new() { inputFileName, outputFileName };

//                var command = ffmpegService.CreateCropCommand();
//                var result = await command.ExecuteAsync(new CropModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName,
//                    Width = dto.Width,
//                    Height = dto.Height,
//                    X = dto.X,
//                    Y = dto.Y,
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg crop failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to crop video: " + result.ErrorMessage, statusCode: 500);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error cropping video");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ApplyAudioEffect(HttpContext context, [FromForm] AudioEffectDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.VideoFile == null || dto.VideoFile.Length == 0)
//                return Results.BadRequest("Video file is required");

//            if (string.IsNullOrWhiteSpace(dto.EffectType))
//                return Results.BadRequest("Effect type is required");

//            string inputFileName = string.Empty;
//            string outputFileName = string.Empty;
//            List<string> filesToCleanup = new();

//            try
//            {
//                inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);

//                outputFileName = string.IsNullOrWhiteSpace(dto.OutputFileName)
//                    ? await fileService.GenerateUniqueFileNameAsync(extension)
//                    : dto.OutputFileName;

//                filesToCleanup.Add(inputFileName);
//                filesToCleanup.Add(outputFileName);

//                var command = ffmpegService.CreateAudioEffectCommand();
//                var result = await command.ExecuteAsync(new AudioEffectModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName,
//                    EffectType = dto.EffectType,
//                    Duration = dto.Duration
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg audio effect failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to apply audio effect: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error applying audio effect");
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> CreatePreview(HttpContext context, [FromForm] PreviewDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.VideoFile == null)
//                return Results.BadRequest("Video file is required");

//            try
//            {
//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".jpg");
//                List<string> filesToCleanup = new() { videoFileName, outputFileName };
//                string timestamp = string.IsNullOrEmpty(dto.Timestamp) ? "00:00:05" : dto.Timestamp;

//                var command = ffmpegService.CreatePreviewCommand();
//                var result = await command.ExecuteAsync(new PreviewModel
//                {
//                    InputFile = videoFileName,
//                    OutputFile = outputFileName,
//                    Timestamp = timestamp
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg preview failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to create preview: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "image/jpeg", "preview.jpg");
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in CreatePreview endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }


//        private static async Task<IResult> ReduceQuality(
//    HttpContext context,
//    [FromForm] ReduceQualityDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.VideoFile == null)
//                {
//                    return Results.BadRequest("Video file is required");
//                }

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

//                try
//                {
//                    var command = ffmpegService.CreateReduceQualityCommand();
//                    var result = await command.ExecuteAsync(new ReduceQualityModel
//                    {
//                        InputFile = videoFileName,
//                        OutputFile = outputFileName
//                    });

//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to reduce video quality: " + result.ErrorMessage, statusCode: 500);
//                    }

//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error processing reduce quality request");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in ReduceQuality endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> MergeTwoFiles(
//            HttpContext context,
//            [FromForm] MergeTwoFilesDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.FirstInputFile == null || dto.SecondInputFile == null)
//                return Results.BadRequest("Both video files are required");

//            try
//            {
//                string firstVideoFileName = await fileService.SaveUploadedFileAsync(dto.FirstInputFile);
//                string secondVideoFileName = await fileService.SaveUploadedFileAsync(dto.SecondInputFile);
//                string extension = Path.GetExtension(dto.FirstInputFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                var filesToCleanup = new List<string> { firstVideoFileName, secondVideoFileName, outputFileName };

//                var command = ffmpegService.CreateMergeTwoFilesCommand();
//                var result = await command.ExecuteAsync(new MergeTwoFilesModel
//                {
//                    FirstInputFile = firstVideoFileName,
//                    SecondInputFile = secondVideoFileName,
//                    OutputFile = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg merge failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to merge videos: " + result.ErrorMessage, statusCode: 500);
//                }

//                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                return Results.File(fileBytes, "video/mp4", dto.FirstInputFile.FileName);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in MergeTwoFiles endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }


//        private static async Task<IResult> AddAnimatedText(HttpContext context, [FromForm] AnimatedTextDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//            try
//            {
//                if (dto.VideoFile == null || dto.Content == null)
//                {
//                    return Results.BadRequest("Video file and content are required");
//                }

//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                var filesToCleanup = new List<string> { videoFileName, outputFileName };
//                try
//                {
//                    var command = ffmpegService.CreateAnimatedTextCommand();
//                    var result = await command.ExecuteAsync(new AnimatedTextModel
//                    {
//                        InputFile = videoFileName,
//                        OutputFile = outputFileName,
//                        Content = dto.Content,
//                        XPosition = dto.XPosition,
//                        YPosition = dto.YPosition,
//                        FontSize = dto.FontSize,
//                        FontColor = dto.FontColor ?? "white",
//                        IsAnimated = dto.IsAnimated,
//                        AnimationSpeed = dto.AnimationSpeed,
//                    });
//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to add animatedtext: " + result.ErrorMessage, statusCode: 500);
//                    }
//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error processing animatedtext request");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in animatedtext endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ChangeResolution(
//            HttpContext context,
//            [FromForm] ChangeResolutionDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//            try
//            {
//                if (dto.VideoFile == null)
//                {
//                    return Results.BadRequest("Video file is required");
//                }
//                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//                string extension = Path.GetExtension(dto.VideoFile.FileName);
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };
//                try
//                {
//                    var command = ffmpegService.CreateChangeResolutionCommand();
//                    var result = await command.ExecuteAsync(new ChangeResolutionModel
//                    {
//                        InputFile = videoFileName,
//                        Width = dto.Width,
//                        Height = dto.Height,
//                        OutputFile = outputFileName,
//                        VideoCodec = "libx264"
//                    });
//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {CommandExecuted}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Failed to change resolution: " + result.ErrorMessage, statusCode: 500);
//                    }
//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error processing subtitle translation request");
//                    logger.LogError(ex, "Error processing change resolution request");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in AddSubtitles endpoint");

//                logger.LogError(ex, "Error in ChangeResolution endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//        private static async Task<IResult> ConvertVideoFormat(
//            HttpContext context,
//            [FromForm] ConvertFormatDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            try
//            {
//                if (dto.InputFile == null || string.IsNullOrWhiteSpace(dto.OutputFileName))
//                    return Results.BadRequest("Input file and output format are required");

//                string inputFileName = await fileService.SaveUploadedFileAsync(dto.InputFile);
//                string outputExtension = "." + dto.OutputFileName.Trim().ToLower();
//                string outputFileName = await fileService.GenerateUniqueFileNameAsync(outputExtension);

//                List<string> filesToCleanup = new() { inputFileName, outputFileName };

//                try
//                {
//                    var command = ffmpegService.CreateConvertFormatCommand();
//                    var result = await command.ExecuteAsync(new ConvertFormatModel
//                    {
//                        InputFile = inputFileName,
//                        OutputFile = outputFileName
//                    });

//                    if (!result.IsSuccess)
//                    {
//                        logger.LogError("FFmpeg format conversion failed: {ErrorMessage}, Command: {Command}",
//                            result.ErrorMessage, result.CommandExecuted);
//                        return Results.Problem("Conversion failed: " + result.ErrorMessage);
//                    }

//                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

//                    return Results.File(fileBytes, "video/" + dto.OutputFileName, "converted" + outputExtension);
//                }
//                catch (Exception ex)
//                {
//                    logger.LogError(ex, "Error converting video format");
//                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                    throw;
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in ConvertVideoFormat endpoint");
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }
//        private static async Task<IResult> RemoveAudio(
//       HttpContext context,
//        [FromForm] RemoveAudioDto dto)
//        {
//            var fileService = context.RequestServices.GetRequiredService<IFileService>();
//            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
//            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

//            if (dto.VideoFile == null || dto.VideoFile.Length == 0)
//            {
//                return Results.BadRequest("Video file is required");
//            }

//            string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
//            string extension = Path.GetExtension(dto.VideoFile.FileName);
//            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
//            List<string> filesToCleanup = new() { inputFileName, outputFileName };

//            try
//            {
//                var command = ffmpegService.CreateRemoveAudioCommand();
//                var result = await command.ExecuteAsync(new RemoveAudioModel
//                {
//                    InputFile = inputFileName,
//                    OutputFile = outputFileName
//                });

//                if (!result.IsSuccess)
//                {
//                    logger.LogError("FFmpeg RemoveAudioCommand failed: {ErrorMessage}, Command: {Command}",
//                        result.ErrorMessage, result.CommandExecuted);
//                    return Results.Problem("Failed to remove audio: " + result.ErrorMessage, statusCode: 500);
//                }

//                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.File(fileBytes, "video/mp4", Path.GetFileName(outputFileName));
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error in RemoveAudio endpoint");
//                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
//                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
//            }
//        }

//    }
//}

using FFmpeg.API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        private const int MaxUploadSize = 104857600; // 100 MB
        private const int MaxUploadSize50Mb = 50 * 1024 * 1024;

        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/replace-audio", ReplaceAudio)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/audio/convert", ConvertAudio)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/convertVideoToGif", ConvertVideoToGif)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/greenscreen", ApplyGreenScreen)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/cut", CutVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/fadein", AddFadeInEffect)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/reverse", ReverseVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/border", AddBorder)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/extract-frame", ExtractFrame)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/rotate", RotateVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/timestamp", AddTimestampOverlay)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/change-speed", ChangeVideoSpeed)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/audio/effect", ApplyAudioEffect)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/convertformat", ConvertVideoFormat)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/reduce-quality", ReduceQuality)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/crop", CropVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/mergefiles", MergeTwoFiles)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/thumbnail", CreatePreview)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/animatedtext", AddAnimatedText)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/subtitles", AddSubtitles)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/change-resolution", ChangeResolution)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/remove-audio", RemoveAudio)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));
        }

        private static async Task<IResult> ReplaceAudio([FromForm] ReplaceAudioDto dto, HttpContext context)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var command = context.RequestServices.GetRequiredService<AudioReplaceCommand>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            string videoTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(dto.VideoFile.FileName));
            string audioTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(dto.AudioFile.FileName));
            string outputTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");

            try
            {
                await using (var videoStream = new FileStream(videoTempFile, FileMode.Create))
                    await dto.VideoFile.CopyToAsync(videoStream);

                await using (var audioStream = new FileStream(audioTempFile, FileMode.Create))
                    await dto.AudioFile.CopyToAsync(audioStream);

                var request = new AudioReplaceModel
                {
                    VideoFile = videoTempFile,
                    NewAudioFile = audioTempFile,
                    OutputFile = outputTempFile
                };

                await command.ExecuteAsync(request);

                byte[] outputBytes = await File.ReadAllBytesAsync(outputTempFile);
                return Results.File(outputBytes, "video/mp4", "output_with_audio.mp4");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReplaceAudio endpoint");
                return Results.Problem("Failed to replace audio: " + ex.Message, statusCode: 500);
            }
            finally
            {
                if (File.Exists(videoTempFile)) File.Delete(videoTempFile);
                if (File.Exists(audioTempFile)) File.Delete(audioTempFile);
                if (File.Exists(outputTempFile)) File.Delete(outputTempFile);
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

                try
                {
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
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to add watermark: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing watermark request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddWatermark endpoint");
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

        private static async Task<IResult> ConvertVideoToGif(HttpContext context, [FromForm] ConvertVideoToGifDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.VideoFile == null)
            {
                return Results.BadRequest("Video file is required");
            }

            string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
            string outputFileName = await fileService.GenerateUniqueFileNameAsync(".gif");

            var filesToCleanup = new List<string> { inputFileName, outputFileName };

            try
            {
                var command = ffmpegService.CreateConvertVideoToGifCommand();
                var result = await command.ExecuteAsync(new ConvertVideoToGifModel
                {
                    InputFile = inputFileName,
                    OutputFile = outputFileName,
                    Fps = dto.Fps,
                    Width = dto.Width
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg video-to-GIF failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to convert to GIF: " + result.ErrorMessage, statusCode: 500);
                }

                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "image/gif", Path.GetFileName(outputFileName));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error converting video to GIF");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
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

                try
                {
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
                        logger.LogError("FFmpeg GreenScreen failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to process green screen: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing green screen");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GreenScreen endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
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
                logger.LogError(ex, "Error in CutVideo endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ExtractFrame(HttpContext context, [FromForm] ExtractFrameDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                    return Results.BadRequest("Video file is required");

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".png");

                var filesToCleanup = new List<string> { videoFileName, outputFileName };

                var command = ffmpegService.CreateExtractFrameCommand();
                var result = await command.ExecuteAsync(new ExtractFrameInput
                {
                    InputFile = videoFileName,
                    TimeSpan = dto.TimeSpan,
                    OutputImagePath = outputFileName
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg extract frame failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to extract frame: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "image/png", outputFileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing extract frame request");
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
                logger.LogError(ex, "Error in ReverseVideo endpoint");
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

        private static async Task<IResult> RotateVideo(HttpContext context, [FromForm] RotateDto dto)
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
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                var filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateRotateCommand();
                    var result = await command.ExecuteAsync(new RotateModel
                    {
                        InputFile = videoFileName,
                        OutputFile = outputFileName,
                        RotationAngle = dto.RotationAngle,
                        IsVideo = true,
                        VideoCodec = "libx264"
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg rotation failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to rotate video: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during video rotation");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in RotateVideo endpoint");
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
            string extension = Path.GetExtension(dto.VideoFile.FileName);
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

            if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                return Results.BadRequest("Video file is required");

            if (string.IsNullOrWhiteSpace(dto.EffectType))
                return Results.BadRequest("Effect type is required");

            string inputFileName = string.Empty;
            string outputFileName = string.Empty;
            List<string> filesToCleanup = new();

            try
            {
                inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                outputFileName = string.IsNullOrWhiteSpace(dto.OutputFileName)
                    ? await fileService.GenerateUniqueFileNameAsync(extension)
                    : dto.OutputFileName;

                filesToCleanup.Add(inputFileName);
                filesToCleanup.Add(outputFileName);

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
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ConvertVideoFormat(HttpContext context, [FromForm] ConvertFormatDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.InputFile == null || string.IsNullOrWhiteSpace(dto.OutputFileName))
                    return Results.BadRequest("Input file and output format are required");

                string inputFileName = await fileService.SaveUploadedFileAsync(dto.InputFile);
                string outputExtension = "." + dto.OutputFileName.Trim().ToLower();
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(outputExtension);

                List<string> filesToCleanup = new() { inputFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateConvertFormatCommand();
                    var result = await command.ExecuteAsync(new ConvertFormatModel
                    {
                        InputFile = inputFileName,
                        OutputFile = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg format conversion failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Conversion failed: " + result.ErrorMessage);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/" + dto.OutputFileName, "converted" + outputExtension);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error converting video format");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ConvertVideoFormat endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ReduceQuality(HttpContext context, [FromForm] ReduceQualityDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                {
                    return Results.BadRequest("Video file is required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateReduceQualityCommand();
                    var result = await command.ExecuteAsync(new ReduceQualityModel
                    {
                        InputFile = videoFileName,
                        OutputFile = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to reduce video quality: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing reduce quality request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReduceQuality endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> CropVideo(HttpContext context, [FromForm] CropDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.VideoFile == null || dto.VideoFile.Length == 0)
                return Results.BadRequest("Video file is required");

            try
            {
                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new() { inputFileName, outputFileName };

                var command = ffmpegService.CreateCropCommand();
                var result = await command.ExecuteAsync(new CropModel
                {
                    InputFile = inputFileName,
                    OutputFile = outputFileName,
                    Width = dto.Width,
                    Height = dto.Height,
                    X = dto.X,
                    Y = dto.Y,
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg crop failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to crop video: " + result.ErrorMessage, statusCode: 500);
                }

                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cropping video");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> MergeTwoFiles(HttpContext context, [FromForm] MergeTwoFilesDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.FirstInputFile == null || dto.SecondInputFile == null)
                return Results.BadRequest("Both video files are required");

            try
            {
                string firstVideoFileName = await fileService.SaveUploadedFileAsync(dto.FirstInputFile);
                string secondVideoFileName = await fileService.SaveUploadedFileAsync(dto.SecondInputFile);
                string extension = Path.GetExtension(dto.FirstInputFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                var filesToCleanup = new List<string> { firstVideoFileName, secondVideoFileName, outputFileName };

                var command = ffmpegService.CreateMergeTwoFilesCommand();
                var result = await command.ExecuteAsync(new MergeTwoFilesModel
                {
                    FirstInputFile = firstVideoFileName,
                    SecondInputFile = secondVideoFileName,
                    OutputFile = outputFileName
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg merge failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to merge videos: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", dto.FirstInputFile.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MergeTwoFiles endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> CreatePreview(HttpContext context, [FromForm] PreviewDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.VideoFile == null)
                return Results.BadRequest("Video file is required");

            try
            {
                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".jpg");

                List<string> filesToCleanup = new() { videoFileName, outputFileName };

                string timestamp = string.IsNullOrEmpty(dto.Timestamp) ? "00:00:05" : dto.Timestamp;

                var command = ffmpegService.CreatePreviewCommand();
                var result = await command.ExecuteAsync(new PreviewModel
                {
                    InputFile = videoFileName,
                    OutputFile = outputFileName,
                    Timestamp = timestamp
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg preview failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to create preview: " + result.ErrorMessage, statusCode: 500);
                }

                byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "image/jpeg", "preview.jpg");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CreatePreview endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddAnimatedText(HttpContext context, [FromForm] AnimatedTextDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.Content == null)
                {
                    return Results.BadRequest("Video file and content are required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                var filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateAnimatedTextCommand();
                    var result = await command.ExecuteAsync(new AnimatedTextModel
                    {
                        InputFile = videoFileName,
                        OutputFile = outputFileName,
                        Content = dto.Content,
                        XPosition = dto.XPosition,
                        YPosition = dto.YPosition,
                        FontSize = dto.FontSize,
                        FontColor = dto.FontColor ?? "white",
                        IsAnimated = dto.IsAnimated,
                        AnimationSpeed = dto.AnimationSpeed,
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to add animatedtext: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing animatedtext request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in animatedtext endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddSubtitles(HttpContext context, [FromForm] SubtitleTranslationDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.SubtitleFile == null)
                    return Results.BadRequest("Video file and subtitle file are required");

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string subtitleFileName = await fileService.SaveUploadedFileAsync(dto.SubtitleFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                var filesToCleanup = new List<string> { videoFileName, subtitleFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateSubtitleTranslationCommand();
                    var result = await command.ExecuteAsync(new SubtitleTranslationModel
                    {
                        InputFile = videoFileName,
                        SubtitleFile = subtitleFileName,
                        OutputFile = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to add subtitles: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing subtitle translation request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddSubtitles endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ChangeResolution(HttpContext context, [FromForm] ChangeResolutionDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                {
                    return Results.BadRequest("Video file is required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateChangeResolutionCommand();
                    var result = await command.ExecuteAsync(new ChangeResolutionModel
                    {
                        InputFile = videoFileName,
                        Width = dto.Width,
                        Height = dto.Height,
                        OutputFile = outputFileName,
                        VideoCodec = "libx264"
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {CommandExecuted}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to change resolution: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing change resolution request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChangeResolution endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> RemoveAudio(HttpContext context, [FromForm] RemoveAudioDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            if (dto.VideoFile == null || dto.VideoFile.Length == 0)
            {
                return Results.BadRequest("Video file is required");
            }

            string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
            string extension = Path.GetExtension(dto.VideoFile.FileName);
            string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

            List<string> filesToCleanup = new() { inputFileName, outputFileName };

            try
            {
                var command = ffmpegService.CreateRemoveAudioCommand();
                var result = await command.ExecuteAsync(new RemoveAudioModel
                {
                    InputFile = inputFileName,
                    OutputFile = outputFileName
                });

                if (!result.IsSuccess)
                {
                    logger.LogError("FFmpeg RemoveAudioCommand failed: {ErrorMessage}, Command: {Command}",
                        result.ErrorMessage, result.CommandExecuted);
                    return Results.Problem("Failed to remove audio: " + result.ErrorMessage, statusCode: 500);
                }

                var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                return Results.File(fileBytes, "video/mp4", Path.GetFileName(outputFileName));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in RemoveAudio endpoint");
                _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }
    }
}
