using System.Diagnostics;
using FFmpeg.API.DTOs;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using Microsoft.AspNetCore.Mvc;


namespace FFmpeg.API.Endpoints
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConvertController : ControllerBase
    {
        private readonly IConvertService _convertService;
        public ConvertController(IConvertService convertService)
        {
            _convertService = convertService;
        }
        [HttpPost("convert")]
        public async Task<IActionResult> Post([FromForm] ConvertRequest request) 
        {
            if (request.Video == null || request.Video.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                byte[]? convertedFileBytes = await _convertService.ConvertDataAsync(request);

                if (convertedFileBytes != null)
                {
                    return File(convertedFileBytes, "video/mp4", "converted_video.mp4");
                }
                else
                {
                    return StatusCode(500, "Video conversion failed for an unknown reason.");
                }
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) 
            {
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex) 
            {
                Console.Error.WriteLine($"An error occurred in API: {ex.Message}");
                return StatusCode(500, "An internal server error occurred during video conversion.");
            }
        }
    }
}
