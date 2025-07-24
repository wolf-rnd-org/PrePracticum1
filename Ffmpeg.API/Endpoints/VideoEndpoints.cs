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
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB
            app.MapPost("/api/audio/convert", ConvertAudio)
            .DisableAntiforgery()
          .WithMetadata(new RequestSizeLimitAttribute(52428800)); // 50MB

        }

        private static async Task<IResult> AddWatermark(
            HttpContext context,
            [FromForm] WatermarkDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // or a specific logger type

            try
            {
                // Validate request
                if (dto.VideoFile == null || dto.WatermarkFile == null)
                {
                    return Results.BadRequest("Video file and watermark file are required");
                }

                // Save uploaded files
                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);

                // Generate output filename
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                // Track files to clean up
                List<string> filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };

                try
                {
                    // Create and execute the watermark command
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

                    // Read the output file
                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                    // Clean up temporary files
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    // Return the file
                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing watermark request");
                    // Clean up on error
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

    }
}