using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegWrapper
{
    public class MediaConverter
    {
        private const string ffmpeg4Progress = @"frame=\s*(?'frame'\d+).*?time=(?'time'[\d\:\.]+)\sbitrate=\s*(?'bitrate'[\d\.]+)(?'bitrateType'[\w\/]+)";
        private const string ffmpeg4VideoDuration = @"Stream.*?Video.*?Metadata:.*?DURATION\s*:\s*(?'duration'[\d:\.]+)";
        private const string ffmpeg4AudioDuration = @"Stream.*?Audio.*?Metadata:.*?DURATION\s*:\s*(?'duration'[\d:\.]+)";

        private readonly string _mmpegLocation;

        public MediaConverter(string mmpegLocation)
        {
            if (string.IsNullOrEmpty(mmpegLocation))
            {
                throw new ArgumentException("message", nameof(mmpegLocation));
            }


            if (!File.Exists(mmpegLocation))
            {
                throw new FileNotFoundException("mmpeg file not found.");
            }

            _mmpegLocation = mmpegLocation;

        }

        public event EventHandler<MediaConverterProgressEventArgs> Progress;

        public event EventHandler<MediaConverterOutputEventArgs> DataOutput;

        public event EventHandler<MediaConverterErrorEventArgs> Error;

        public Task ConvertAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellaionToken)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                throw new ArgumentException("message", nameof(sourceFilePath));
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException(sourceFilePath);
            }

            return Task.Factory.StartNew(() =>
{
    string d = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    try
    {
        var total = GetDuration(sourceFilePath);
        Process(sourceFilePath, destinationFilePath, total);
    }
    catch (Exception e)
    {
        throw;
    }

}, cancellaionToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        protected virtual void OnProgress(MediaConverterProgressEventArgs e)
        {
            Progress?.Invoke(this, e);
        }

        protected virtual void OnDataOutput(global::FFMpegWrapper.MediaConverterOutputEventArgs e)
        {
            DataOutput?.Invoke(this, e);
        }

        protected virtual void OnError(global::FFMpegWrapper.MediaConverterErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        private void Process(string sourceFilePath, string destinationFilePath, TimeSpan total)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                throw new ArgumentException("message", nameof(sourceFilePath));
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException($"File {sourceFilePath} not found.");
            }

            var ffmpegProgressDuration = new Regex(ffmpeg4VideoDuration, RegexOptions.Singleline);
            var ffmpegProgressRegex = new Regex(ffmpeg4Progress, RegexOptions.Compiled);

            using (var process = new Process())
            {
                process.StartInfo.FileName = _mmpegLocation;
                process.StartInfo.Arguments = $"-y -i \"{sourceFilePath}\" \"{destinationFilePath}\"";

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    OnDataOutput(new MediaConverterOutputEventArgs(e.Data));
                };

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data == null)
                    {
                        return;
                    }

                    try
                    {
                        bool isProgress = ExtractProgress(ffmpegProgressRegex, e.Data, out TimeSpan duration);
                        if (isProgress)
                        {
                            OnProgress(new MediaConverterProgressEventArgs(duration, total));
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(new MediaConverterErrorEventArgs(ex));
                    }

                    OnDataOutput(new MediaConverterOutputEventArgs(e.Data));
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
        }

        private TimeSpan GetDuration(string sourceFilePath)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = _mmpegLocation;
                process.StartInfo.Arguments = $"-i {sourceFilePath} -c copy -map_metadata 0 -map_metadata:s:v 0:s:v -map_metadata:s:a 0:s:a";

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                process.WaitForExit();
                string outputText = process.StandardError.ReadToEnd();

                var ffmpegVideoDuration = new Regex(ffmpeg4VideoDuration, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var videos = ffmpegVideoDuration.Matches(outputText);

                var ffmpegAudioDuration = new Regex(ffmpeg4AudioDuration, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var audios = ffmpegAudioDuration.Matches(outputText);

                if (videos.Count == 0 || audios.Count == 0)
                {
                    throw new InvalidOperationException("Duration not found.");
                }

                TimeSpan maxTimeSpan = new TimeSpan();
                foreach (Match video in videos)
                {
                    var timeSpan = TryParseTimespan(video.Groups["duration"].Value);
                    if (timeSpan.HasValue)
                    {
                        if (timeSpan > maxTimeSpan)
                        {
                            maxTimeSpan = timeSpan.Value;
                        }
                    }
                }

                foreach (Match audio in audios)
                {
                    var timeSpan = TryParseTimespan(audio.Groups["duration"].Value);
                    if (timeSpan.HasValue)
                    {
                        if (timeSpan > maxTimeSpan)
                        {
                            maxTimeSpan = timeSpan.Value;
                        }
                    }
                }

                return maxTimeSpan;
            }
        }

        private TimeSpan? TryParseTimespan(string text)
        {
            bool successed = DateTime.TryParse(text, out DateTime dateTime);
            if (successed)
            {
                return dateTime.TimeOfDay;
            }

            return null;
        }

        private bool ExtractProgress(Regex ffmpegProgressRegex, string data, out TimeSpan duration)
        {
            var match = ffmpegProgressRegex.Match(data);
            if (!match.Success)
            {
                duration = default(TimeSpan);
                return false;
            }
            var m = match.Groups["time"];
            if (!TimeSpan.TryParse(m.Value, out duration))
            {
                duration = default(TimeSpan);
                return false;
            }

            return true;
        }

    }


}
