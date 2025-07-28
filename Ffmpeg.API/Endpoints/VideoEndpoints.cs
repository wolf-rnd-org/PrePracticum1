using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.API.DTOs;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        private const int MaxUploadSize = 104857600; // 100 MB

        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));

            app.MapPost("/api/video/greenscreen", ApplyGreenScreen)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            app.MapPost("/api/video/reverse", ReverseVideo)
               .DisableAntiforgery()
               .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            app.MapPost("/api/video/border", AddBorder)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/audio/convert", ConvertAudio)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(52428800)); // 50MB

            app.MapPost("/api/video/mergefiles", MergeTwoFiles)
               .DisableAntiforgery()
               .WithMetadata(new RequestSizeLimitAttribute(MaxUploadSize));
        }

        private static async Task<IResult> ReverseVideo(
          HttpContext context,
          [FromForm] ReverseVideoDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            try
            {
                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateReverseVideoCommand();
                    var result = await command.ExecuteAsync(new ReverseVideoModel
                    {
                        InputFile = videoFileName,
                        OutputFile = outputFileName,
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
                    logger.LogError(ex, "Error reversing video");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in reverseVideo endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AddWatermark(
            HttpContext context,
            [FromForm] WatermarkDto dto)
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

        private static async Task<IResult> ApplyGreenScreen(
            HttpContext context,
            [FromForm] GreenScreenDto dto)
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

        private static async Task<IResult> AddBorder(
            HttpContext context,
            [FromForm] BorderDto dto)
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
        
                int borderSize;
                if (!int.TryParse(dto.BorderSize, out borderSize) || borderSize <= 0)
                    borderSize = 20;
        
                string inputFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
                List<string> filesToCleanup = new() { inputFileName, outputFileName };
        
                try
                {
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
                        logger.LogError("FFmpeg BorderCommand failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to add border: " + result.ErrorMessage, statusCode: 500);
                    }
        
                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing border request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddBorder endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }
        
        private static async Task<IResult> ConvertAudio(
        HttpContext context,
        [FromForm] ConvertAudioDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        
            if (dto.AudioFile == null || string.IsNullOrWhiteSpace(dto.OutputFormat))
            {
                return Results.BadRequest("Audio file and output format are required");
            }
        
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

        private static async Task<IResult> MergeTwoFiles(
        HttpContext context,
        [FromForm] MergeTwoFilesDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.FirstInputFile == null || dto.SecondInputFile == null)
                    return Results.BadRequest("Videos files are required");

                string firstVideoFileName = await fileService.SaveUploadedFileAsync(dto.FirstInputFile);
                string secondVideoFileName = await fileService.SaveUploadedFileAsync(dto.SecondInputFile);
                string extension = Path.GetExtension(dto.FirstInputFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                var filesToCleanup = new List<string> { firstVideoFileName, secondVideoFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateMergeTwoFilesCommand();
                    var result = await command.ExecuteAsync(new MergeTwoFilesModel
                    {
                        FirstInputFile = firstVideoFileName,
                        SecondInputFile = secondVideoFileName,
                        OutputFile = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to merge videos: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.FirstInputFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing merge request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MergeTwoFiles endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

    }
}
