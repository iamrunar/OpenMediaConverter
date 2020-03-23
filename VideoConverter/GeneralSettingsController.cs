using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace VideoConverter
{
    public partial class GeneralSettingsController : NSViewController
    {
        public GeneralSettingsController(IntPtr handle) : base(handle)
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

        partial void SetDestinationDirectoryAction(NSObject sender)
        {
            if (OpenSelectDirectoryDialog(Preferences.DestinationDirectory, out string dir))
            {
                Preferences.DestinationDirectory = dir;
            }
        }

        partial void SetObserveDirectoryAction(NSObject sender)
        {
            if (OpenSelectDirectoryDialog(Preferences.ObserveDirectory, out string dir))
            {
                Preferences.ObserveDirectory = dir;
            }
        }

        private static bool OpenSelectDirectoryDialog(string sourceDirectory, out string directory)
        {
            NSOpenPanel dlg = NSOpenPanel.OpenPanel;
            dlg.CanChooseFiles = false;
            dlg.CanChooseDirectories = true;
            dlg.Directory = sourceDirectory;
            bool isSuccess = dlg.RunModal() == 1;

            if (isSuccess && dlg.Urls[0] != null)
            {
                directory = dlg.Urls[0].Path;
                return true;
            }

            directory = null;
            return false;

        }
    }
}
