using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;

namespace FFmpeg.Infrastructure.Services
{
    public class ConvertService : IConvertService
    {
        public async Task<byte[]?> ConvertDataAsync(ConvertRequest request)
        {
            if (!Regex.IsMatch(request.Bitrate, @"^\d+[kM]?$"))
            {
                throw new ArgumentException("Invalid bitrate format. Please provide a numeric value (e.g., 1000k or 1M).");
            }

            string inputPath = string.Empty;
            string outputPath = string.Empty;
            byte[]? resultBytes = null;

            try
            {
                inputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + Path.GetExtension(request.Video!.FileName));
                outputPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");

                await using (var stream = System.IO.File.Create(inputPath))
                {
                    await request.Video.CopyToAsync(stream);
                }

                var ffmpegStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{inputPath}\" -b:v {request.Bitrate} -y \"{outputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = ffmpegStartInfo })
                {
                    process.Start();
                    string stderr = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        Console.Error.WriteLine($"FFmpeg error: {stderr}");
                        throw new InvalidOperationException($"Video conversion failed. Error: {stderr}");
                    }
                }
                resultBytes = await System.IO.File.ReadAllBytesAsync(outputPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred during conversion: {ex.Message}");
                throw;
            }
            finally
            {
                if (System.IO.File.Exists(inputPath))
                {
                    System.IO.File.Delete(inputPath);
                }
                if (System.IO.File.Exists(outputPath))
                {
                    System.IO.File.Delete(outputPath);
                }
            }
            return resultBytes;
        }
    }
}