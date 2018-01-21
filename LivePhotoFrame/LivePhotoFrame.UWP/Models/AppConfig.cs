using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LivePhotoFrame.UWP.Models
{
    public class AppConfig
    {
        public FtpConfig FtpConfig { get; set; }
        public string FileSystemPath { get; set; }
        public string ActiveSource { get; set; }

        /// <summary>
        /// Interval in minutes
        /// </summary>
        public int Interval { get; set; }

        public AppConfig()
        {
            FtpConfig = new FtpConfig();
            FileSystemPath = @"D:\Pictures\LivePhotoFrame\Albums\Current\";
            ActiveSource = FtpPhotoProvider.TAG;
            Interval = 30;
        }

        public void Save()
        {
            AppConfigManager.GetInstance().SaveConfig(this);
        }
    }

    public class AppConfigManager
    {
        public const string SETTINGS_KEY = "AppSettings";
        private static AppConfigManager instance;

        public static AppConfigManager GetInstance()
        {
            if (instance == null)
            {
                instance = new AppConfigManager();
            }
            return instance;
        }

        public AppConfig GetConfig()
        {
            AppConfig settings;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var settingsObject = localSettings.Values[SETTINGS_KEY];
            if (settingsObject == null)
            {
                settings = new AppConfig();
            }
            else
            {
                settings = JsonConvert.DeserializeObject<AppConfig>(settingsObject.ToString());
            }
            return settings;
        }

        public void SaveConfig(AppConfig config)
        {
            string serialized = JsonConvert.SerializeObject(config);
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[SETTINGS_KEY] = serialized;
        }
    }

    public class FtpConfig
    {
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Path { get; set; }

        public FtpConfig()
        {
            Hostname = "ftp.yourserver.com";
            Path = "/path/to/your/photos/";
            Username = "yourusername";
            Password = "yourpassword";
        }
    }
}
