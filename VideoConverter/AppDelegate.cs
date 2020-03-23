using System;
using System.IO;
using AppKit;
using Foundation;

namespace VideoConverter
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public AppPreferences Preferences { get; set; } = new AppPreferences();

        public override void DidFinishLaunching(NSNotification notification)
        {
            NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;

            if (string.IsNullOrEmpty(Preferences.DestinationDirectory))
            {
                Preferences.DestinationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "VideoConverter-Export");
                Directory.CreateDirectory(Preferences.DestinationDirectory);
            }

            if (string.IsNullOrEmpty(Preferences.ObserveDirectory))
            {
                Preferences.ObserveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Downloads");
                Directory.CreateDirectory(Preferences.ObserveDirectory);
            }
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
