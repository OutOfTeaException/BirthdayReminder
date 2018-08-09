using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BirthdayReminder.Services
{
    public class ConfigurationService
    {
        private const string FILE_LAST_CHECK_TIME = "lastCheckTime";
        private const string FILE_CHECK_TIME = "checkTime.config";

        private static readonly string lastCheckTimeFile;
        private static readonly string checkTimeFile;

        static ConfigurationService()
        {
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            Directory.CreateDirectory(documentsPath);

            lastCheckTimeFile = Path.Combine(documentsPath, FILE_LAST_CHECK_TIME);
            checkTimeFile = Path.Combine(documentsPath, FILE_CHECK_TIME);
        }

        public void SaveLastCheckTime(DateTime lastCheckTime)
        {
            string timestamp = lastCheckTime.ToString("u");
            File.WriteAllText(lastCheckTimeFile, timestamp);
        }

        public DateTime? GetLastCheckTime()
        {
            if (!File.Exists(lastCheckTimeFile))
            {
                return null;
            }

            string timeStamp = File.ReadAllText(lastCheckTimeFile);

            return DateTime.ParseExact(timeStamp, "u", CultureInfo.InvariantCulture);
        }

        public void RemoveLastCheckTime()
        {
            File.Delete(lastCheckTimeFile);
        }

        public void SaveCheckTime((int hours, int minutes) checkTime)
        {
            File.WriteAllText(checkTimeFile, $"{checkTime.hours:00}:{checkTime.minutes:00}");
        }

        public (int hours, int minutes)? GetCheckTime()
        {
            if (!File.Exists(checkTimeFile))
            {
                return null;
            }

            try
            {
                string content = File.ReadAllText(checkTimeFile);

                var split = content.Split(':');

                return (Int32.Parse(split[0]), Int32.Parse(split[1]));
            }
            catch (IOException)
            {
                return null;
            }            
            catch (IndexOutOfRangeException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}