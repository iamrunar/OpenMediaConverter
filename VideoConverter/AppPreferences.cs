using System;
using Foundation;

namespace VideoConverter
{
    [Register("AppPreferences")]
    public class AppPreferences : NSObject
    {
        public AppPreferences()
        {
        }

        [Export("MoveFileToTrash")]
        public bool MoveFileToTrash
        {
            get => LoadBool("MoveFileToTrash", true);
            set
            {
                WillChangeValue("MoveFileToTrash");
                SaveBool("MoveFileToTrash", value, true);
                DidChangeValue("MoveFileToTrash");
            }
        }

        [Export("RemoveSuccessFiles")]
        public bool RemoveSuccessFiles
        {
            get => LoadBool("RemoveSuccessFiles", true);
            set
            {
                WillChangeValue("RemoveSuccessFiles");
                SaveBool("RemoveSuccessFiles", value, true);
                DidChangeValue("RemoveSuccessFiles");
            }
        }

        [Export("ObserveDirectory")]
        public string ObserveDirectory
        {
            get => LoadText("ObserveDirectory", string.Empty);
            set
            {
                WillChangeValue("ObserveDirectory");
                SaveText("ObserveDirectory", value, true);
                DidChangeValue("ObserveDirectory");
            }
        }

        [Export("DestinationDirectory")]
        public string DestinationDirectory
        {
            get => LoadText("DestinationDirectory", string.Empty);
            set
            {
                WillChangeValue("DestinationDirectory");
                SaveText("DestinationDirectory", value, true);
                DidChangeValue("DestinationDirectory");
            }
        }

        public string LoadText(string key, string defaultValue)
        {
            return NSUserDefaults.StandardUserDefaults.StringForKey(key) ?? defaultValue;
        }

        public void SaveText(string key, string value, bool sync)
        {
            NSUserDefaults.StandardUserDefaults.SetString(value, key);
            if (sync)
            {
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public bool LoadBool(string key, bool defaultValue)
        {
            var c = NSUserDefaults.StandardUserDefaults.BoolForKey(key); ;
            return c;
        }

        public void SaveBool(string key, bool value, bool sync)
        {
            NSUserDefaults.StandardUserDefaults.SetBool(value, key);
            if (sync)
            {
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }
    }
}
