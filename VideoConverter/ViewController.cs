using System;
using System.IO;
using AppKit;
using Foundation;
using System.Linq;
using System.Configuration;
using FFMpegWrapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace VideoConverter
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        public static AppDelegate App
        {
            get { return (AppDelegate)NSApplication.SharedApplication.Delegate; }
        }

        [Export("Preferences")]
        public AppPreferences Preferences => App.Preferences;

        private bool RemoveSuccessFiles => Preferences.RemoveSuccessFiles;

        private string ObserveDirectory => Preferences.ObserveDirectory;

        private string DestinationDirectory => Preferences.DestinationDirectory;

        private bool MoveFileToTrash => Preferences.MoveFileToTrash;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            bool containsFiles = GetConvertableFiles(ObserveDirectory).Any();
            if (!containsFiles)
            {
                ProcessTextField.StringValue = "Нет файлов для конвертации.";
                ProcessButton.Enabled = false;
            }
            else
            {
                ProcessTextField.StringValue = "Есть файлы для конвертации.";
                ProcessButton.Enabled = true;
            }
        }

        partial void ProcessButtonClicked(NSObject _)
        {
            string observeDirectory = ObserveDirectory;
            string destinationDirectory = DestinationDirectory;
            bool removeSuccessFiles = RemoveSuccessFiles;
            bool moveToTrash = MoveFileToTrash;

            ProcessButton.Enabled = false;
            try
            {
                Directory.CreateDirectory(DestinationDirectory);
                Task.Factory.StartNew(() => ConvertsAll(GetConvertableFiles(observeDirectory)), TaskCreationOptions.LongRunning | TaskCreationOptions.AttachedToParent)
                    .ContinueWith(t =>
                    {
                        ProcessButton.Enabled = true;
                    });
            }
            catch (Exception ex)
            {
                OnError("Ошибка при инициализации конвертера", ex);
            }

            //locals
            async Task ConvertsAll(IEnumerable<string> files)
            {
                long totalFiles = files.Count();
                foreach (var file in files)
                {
                    try
                    {
                        await HookAndConvertAsync(file);
                    }
                    catch (Exception ex)
                    {
                        OnError("Ошибка при конвертировании", ex);
                    }
                }
            }

            void OnCompleted(string file)
            {
                if (removeSuccessFiles)
                {
                    Remove(file);
                }

                this.BeginInvokeOnMainThread(() =>
                {
                    ProcessTextField.StringValue = $"Файл {Path.GetFileName(file)} завершён.";
                });
            }

            void OnProgress(string file, double progress)
            {
                this.BeginInvokeOnMainThread(() =>
                {
                    ProcessTextField.StringValue = $"Файл \"{Path.GetFileName(file)}\" сконвертирован на {progress*100:#.##}%.";
                });
            }

            void OnError(string title, Exception exception)
            {
                this.BeginInvokeOnMainThread(() =>
                        ShowError(title, exception.Message));
            }

            void Remove(string file)
            {
                if (moveToTrash)
                {
                    RemoveToTrash(file);
                }
                else
                {
                    File.Delete(file);
                }
            }

            bool RemoveToTrash(string file)
            {
                using (NSFileManager fileManager = new NSFileManager())
                {
                    return fileManager.TrashItem(new NSUrl(file, false), out NSUrl resultUrl, out NSError error);
                }
            }

            async Task HookAndConvertAsync(string sourceFilePath)
            {
                MediaConverter mediaConverter = new MediaConverter(GetFFmpegFilePath());
                var progress = new ConvertProgress(this, sourceFilePath, p => OnProgress(sourceFilePath, p));
                await mediaConverter.ConvertAsync(sourceFilePath, GetDestinationFileName(destinationDirectory, sourceFilePath), progress, CancellationToken.None);

                OnCompleted(sourceFilePath);
            }
        }

        private void ShowError(string errorText, string infromative)
        {
            var alert = new NSAlert()
            {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = infromative,
                MessageText = errorText,
            };
            alert.RunModal();
        }

        string GetDestinationFileName(string destDirectory, string sourceFilePath) => Path.Combine(destDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + ".mp4");

        string GetFFmpegFilePath() => Path.GetFullPath("ffmpeg");

        IEnumerable<string> GetConvertableFiles(string sourceDirectory) => Directory.EnumerateFiles(sourceDirectory, "*.webm");
    }

    class ConvertProgress : IProgress<MediaConverterProgressEventArgs>
    {
        private readonly ViewController parentViewController;
        private readonly string file;
        private readonly Action<double> progressHandler;

        public ConvertProgress(ViewController parentViewController, string file, Action<double> progress)
        {
            this.parentViewController = parentViewController;
            this.file = file;
            this.progressHandler = progress;
        }

        public void Report(MediaConverterProgressEventArgs e) => progressHandler(e.Duration.TotalMilliseconds / e.AboutTotal.TotalMilliseconds);
    }
}
