
using Ffmpeg.Command;
using FFmpeg.API.Endpoints;
using FFmpeg.Core.Interfaces;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("FfmpegConfig.json", optional: false, reloadOnChange: true);
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100 MB
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.AddSingleton<Ffmpeg.Command.ILogger, Logger>();
builder.Services.AddScoped<IFFmpegServiceFactory>(provider =>
{
    var logger = provider.GetRequiredService<Ffmpeg.Command.ILogger>();
    return new FFmpegServiceFactory(builder.Configuration, logger);
});
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<FFmpegExecutor>(provider =>
{
    var config = builder.Configuration;
    var ffmpegPath = config["FFmpeg:ExecutablePath"];

    if (string.IsNullOrWhiteSpace(ffmpegPath))
    {
        throw new InvalidOperationException("FFmpeg executable path is not configured.");
    }

    return new FFmpegExecutor(ffmpegPath);
});

builder.Services.AddScoped<ICommandBuilder, CommandBuilder>();
builder.Services.AddScoped<AudioReplaceCommand>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapEndpoints();
app.MapGet("/", () => { return "FFmpeg API is running"; });
app.Run();