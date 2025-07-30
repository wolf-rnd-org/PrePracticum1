using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.Core.Models;
using Ffmpeg.Command;
using FFmpeg.Core.Interfaces;
using System.Diagnostics;

namespace FFmpeg.Infrastructure.Commands
{
    public class CutVideoCommand
    {
        private readonly ILogger _logger;
        private readonly IFileService _fileService;

        public CutVideoCommand(ILogger logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<bool> ExecuteAsync(CutVideoModel model)
        {
            try
            {
                var inputPath = _fileService.GetFullInputPath(model.InputFile);
                var outputPath = _fileService.GetFullOutputPath(model.OutputFile);

                var args = $"-i \"{inputPath}\" -ss {model.StartTime} -to {model.EndTime} -c copy \"{outputPath}\"";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                    return false;

                string errorOutput = await process.StandardError.ReadToEndAsync();
                string output = await process.StandardOutput.ReadToEndAsync();


                await process.WaitForExitAsync();

                _logger.LogInformation($"FFmpeg output: {output}");
                Console.WriteLine($"FFmpeg error: {errorOutput}");

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cutting video");
                return false;
            }
        }
    }
}

