using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HD.Station.MediaManagement.Mvc.Services
{
    public class FileProcessor : IFileProcessor
    {
        private readonly string _ffmpegExe;
        private readonly string _ffprobeExe;
        private readonly ILogger<FileProcessor> _logger;

        public FileProcessor(IConfiguration config, ILogger<FileProcessor> logger)
        {
            _logger = logger;

            // Đọc từ appsettings.json: "FFmpegSettings": { "BinFolder": "..."}
            var bin = config["FFmpegSettings:BinFolder"]
                      ?? throw new InvalidOperationException("FFmpegSettings:BinFolder missing in configuration");

            _ffmpegExe = Path.Combine(bin, "ffmpeg.exe");
            _ffprobeExe = Path.Combine(bin, "ffprobe.exe");

            // Verify FFmpeg tools exist
            if (!File.Exists(_ffmpegExe))
                throw new FileNotFoundException($"FFmpeg not found at: {_ffmpegExe}");

            if (!File.Exists(_ffprobeExe))
                throw new FileNotFoundException($"FFprobe not found at: {_ffprobeExe}");

            _logger.LogInformation($"FFmpeg tools initialized: {bin}");
        }

        public async Task<string> GetMediaInfoJsonAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Media file not found: {filePath}");
            }

            var psi = new ProcessStartInfo
            {
                FileName = _ffprobeExe,
                Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_ffprobeExe)
            };

            _logger.LogDebug($"Running FFprobe: {psi.FileName} {psi.Arguments}");

            using var process = Process.Start(psi);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFprobe process");
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError($"FFprobe failed with exit code {process.ExitCode}: {error}");
                throw new InvalidOperationException($"FFprobe failed: {error}");
            }

            _logger.LogDebug($"FFprobe completed successfully for: {filePath}");
            return output;
        }

        public async Task<string> ConvertToMp4Async(string inputPath, string outputFolder)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input file not found: {inputPath}");
            }

            Directory.CreateDirectory(outputFolder);

            var inputFileName = Path.GetFileNameWithoutExtension(inputPath);
            var outputFileName = $"{inputFileName}_{Guid.NewGuid():N}.mp4";
            var outputPath = Path.Combine(outputFolder, outputFileName);

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            // Optimized FFmpeg arguments for web-compatible MP4
            var args = $"-y -i \"{inputPath}\" " +
                      "-c:v libx264 " +
                      "-preset medium " +
                      "-crf 23 " +
                      "-profile:v baseline " +
                      "-level 3.0 " +
                      "-pix_fmt yuv420p " +
                      "-c:a aac " +
                      "-b:a 128k " +
                      "-ac 2 " +
                      "-ar 44100 " +
                      "-movflags +faststart " +
                      $"\"{outputPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = _ffmpegExe,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_ffmpegExe)
            };

            _logger.LogInformation($"Starting video conversion: {inputPath} -> {outputPath}");
            _logger.LogDebug($"FFmpeg command: {psi.FileName} {psi.Arguments}");

            using var process = Process.Start(psi);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start FFmpeg process");
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Set timeout for conversion (10 minutes)
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(10));
            var completedTask = await Task.WhenAny(process.WaitForExitAsync(), timeoutTask);

            if (completedTask == timeoutTask)
            {
                process.Kill(true);
                throw new TimeoutException("Video conversion timed out after 10 minutes");
            }

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError($"FFmpeg conversion failed with exit code {process.ExitCode}");
                _logger.LogError($"FFmpeg stderr: {error}");

                // Clean up failed output file
                if (File.Exists(outputPath))
                {
                    try { File.Delete(outputPath); } catch { }
                }

                throw new InvalidOperationException($"Video conversion failed: {error}");
            }

            // Verify output file was created and has content
            if (!File.Exists(outputPath))
            {
                throw new InvalidOperationException("Conversion completed but output file was not created");
            }

            var outputInfo = new FileInfo(outputPath);
            if (outputInfo.Length == 0)
            {
                File.Delete(outputPath);
                throw new InvalidOperationException("Conversion completed but output file is empty");
            }

            _logger.LogInformation($"Video conversion completed successfully: {outputPath} ({outputInfo.Length} bytes)");
            return outputPath;
        }

        public bool IsVideoFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" or ".mkv" or ".m4v" or ".3gp" => true,
                _ => false
            };
        }

        public bool RequiresConversion(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return IsVideoFile(filePath) && extension != ".mp4";
        }
    }
}