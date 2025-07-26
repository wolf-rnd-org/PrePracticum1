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
using FFmpeg.Infrastructure.Commands;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB
            app.MapPost("/api/video/replace-audio", ReplaceAudio)
    .DisableAntiforgery()
    .WithMetadata(new RequestSizeLimitAttribute(104857600));

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
        private static async Task<IResult> ReplaceAudio(
     [FromForm] ReplaceAudioDto dto,
     HttpContext context)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var command = context.RequestServices.GetRequiredService<AudioReplaceCommand>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            // צור נתיבים זמניים לקבצים
            string videoTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(dto.VideoFile.FileName));
            string audioTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(dto.AudioFile.FileName));
            string outputTempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");

            try
            {
                // שמירת הקבצים שהתקבלו בדיסק זמני
                await using (var videoStream = new FileStream(videoTempFile, FileMode.Create))
                {
                    await dto.VideoFile.CopyToAsync(videoStream);
                }
                await using (var audioStream = new FileStream(audioTempFile, FileMode.Create))
                {
                    await dto.AudioFile.CopyToAsync(audioStream);
                }

                // הכנת בקשת החלפת האודיו עם שמות קבצים
                var request = new AudioReplaceModel
                {
                    VideoFile = videoTempFile,
                    NewAudioFile = audioTempFile,
                    OutputFile = outputTempFile

                };

                // הרצת הפקודה
                await command.ExecuteAsync(request);

                // קריאת הקובץ המיוצר
                byte[] outputBytes = await File.ReadAllBytesAsync(outputTempFile);

                // החזרת הקובץ ללקוח
                return Results.File(outputBytes, "video/mp4", "output_with_audio.mp4");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReplaceAudio endpoint");
                return Results.Problem("Failed to replace audio: " + ex.Message, statusCode: 500);
            }
            finally
            {
                // ניקוי הקבצים הזמניים
                if (File.Exists(videoTempFile)) File.Delete(videoTempFile);
                if (File.Exists(audioTempFile)) File.Delete(audioTempFile);
                if (File.Exists(outputTempFile)) File.Delete(outputTempFile);
            }
        }

    }
}