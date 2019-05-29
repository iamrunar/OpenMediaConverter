using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FFMpegWrapper;

namespace FFmpegConverter_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Task.Run(async () =>
                {
                    try
                    {
                        string d = Path.GetDirectoryName(typeof(MediaConverter).Assembly.Location);
                        var mmpegLocation = Path.Combine(d, "ffmpeg"); 
                        var mediaConverter = new MediaConverter(mmpegLocation);
                        mediaConverter.Progress += (sender, e) =>
                        {
                            Console.WriteLine(
                                $"{e.Duration} from {e.AboutTotal}. " +
                                $"Percent = {(e.Duration.TotalMilliseconds / e.AboutTotal.TotalMilliseconds) * 100:F}");
                        };
                        string sourceFilePath = Path.Combine(d, "source.webm");
                        string destinationFilePath = Path.Combine(d, "dest.mp4");
                        await mediaConverter.ConvertAsync(sourceFilePath, destinationFilePath, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Process error {ex}");
                    }
                }).GetAwaiter().GetResult();
                Console.WriteLine("Successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Process error {ex}");
            }
        }
    }
}
