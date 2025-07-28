
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
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            app.MapPost("/api/video/border", AddBorder)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            // app.MapPost("/api/audio/convert", ConvertAudio)
            //     .DisableAntiforgery()
            //     .WithMetadata(new RequestSizeLimitAttribute(52428800)); // 50 MB
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
                {
                    return Results.BadRequest("Video file and watermark file are required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);

                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };

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
        private static async Task<IResult> ReplaceAudio(
            [FromForm] ReplaceAudioDto dto,
            HttpContext context)
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
                {
                    await dto.VideoFile.CopyToAsync(videoStream);
                }
                await using (var audioStream = new FileStream(audioTempFile, FileMode.Create))
                {
                    await dto.AudioFile.CopyToAsync(audioStream);
                }

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
        //private static async Task<IResult> ConvertAudio(
        //    HttpContext context,
        //    [FromForm] ConvertAudioDto dto)
        //{
        //    var fileService = context.RequestServices.GetRequiredService<IFileService>();
        //   var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
         

        //    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        //    if (dto.AudioFile == null || string.IsNullOrWhiteSpace(dto.OutputFormat))
        //    {
        //        return Results.BadRequest("Audio file and output format are required");
        //    }

        //    string inputFileName = await fileService.SaveUploadedFileAsync(dto.AudioFile);
        //    string extension = "." + dto.OutputFormat.Trim().ToLower();
        //    string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
        //    List<string> filesToCleanup = new() { inputFileName, outputFileName };

            //try
            //{
            //    var command = ffmpegService.CreateConvertAudioCommand();
            //    var result = await command.ExecuteAsync(new ConvertAudioModel
            //    {
            //        InputFile = inputFileName,
            //        OutputFile = outputFileName
            //    });

            //    if (!result.IsSuccess)
            //    {
            //        logger.LogError("FFmpeg audio convert failed: {Error}", result.ErrorMessage);
            //        return Results.Problem("Conversion failed: " + result.ErrorMessage);
            //    }

            //    var fileBytes = await fileService.GetOutputFileAsync(outputFileName);
            //    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
            //    return Results.File(fileBytes, "application/octet-stream", Path.GetFileName(outputFileName));
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, "Error converting audio");
            //    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
            //    return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            //}
        //}
    }
}
