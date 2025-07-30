using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class MergeTwoFilesCommand : BaseCommand, ICommand<MergeTwoFilesModel>
    {

        private readonly ICommandBuilder _commandBuilder;
        public MergeTwoFilesCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder) : base(executor)
        {
            _commandBuilder = commandBuilder;
        }

        public async Task<CommandResult> ExecuteAsync(MergeTwoFilesModel request)
        {
            CommandBuilder = _commandBuilder
                .SetInput(request.FirstInputFile)
                .SetInput(request.SecondInputFile)
                .AddFilterComplex($"[0:v][1:v]hstack=inputs=2[out]")
                .SetOutput(request.OutputFile);

            return await RunAsync();
        }
    }
}
