﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using BirthdayReminder.Services;
using BirthdayReminder.Util;

namespace BirthdayReminder.Jobs
{
    [Service(Name = "BirthdayReminder.BirthdayCheckJob", Permission = "android.permission.BIND_JOB_SERVICE")]
    public class BirthdayCheckJob : JobService
    {
        public const string JOBPARAM_DAYS_IN_FUTURE = "daysInFuture";
        public const string JOBPARAM_CHECKTIME = "checkTime";

        private const string FILE_LAST_CHECK_TIME = "lastCheckTime";
        private const string TAG = "BirthdayCheckJob";
        private static readonly string lastCheckTimeFile;

        private NotificationHelper notificationHelper;
        private BirthdayService birthdayService;
        

        static BirthdayCheckJob()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            Directory.CreateDirectory(documentsPath);

            lastCheckTimeFile = Path.Combine(documentsPath, FILE_LAST_CHECK_TIME);
        }

        public override bool OnStartJob(JobParameters jobParams)
        {
            Log.Info("Starte BirthdayCheck Job...");

            if (notificationHelper == null)
            {
                notificationHelper = new NotificationHelper(this);
                birthdayService = new BirthdayService(this);
            }

            int checkDaysInFuture = jobParams.Extras.GetInt(JOBPARAM_DAYS_IN_FUTURE, 30);
            int[] checkTimeArray = jobParams.Extras.GetIntArray(JOBPARAM_CHECKTIME);
            (int hour, int minute) checkTime = (checkTimeArray[0], checkTimeArray[1]);

            if (ShouldCheck(DateTime.Now, checkTime, GetLastCheckTime()))
            {
                Task.Run(() =>
                {
                    Log.Debug($"Job wird ausgeführt (CheckTime: {checkTime.hour:00}:{checkTime.minute:00})...");
                    CheckForNextBirthdays(checkDaysInFuture);
                    SaveLastCheckTime(DateTime.Now);

                    Log.Debug("Job erfolgreich ausgeführt.");
                    JobFinished(jobParams, false);
                });

                Log.Debug("Job gestartet.");
            }
            return true;
        }

        private bool ShouldCheck(DateTime now, (int hour, int minute) checkTime, DateTime? lastCheckTime)
        {
            var checkDateTime = new DateTime(now.Year, now.Month, now.Day, checkTime.hour, checkTime.minute, 0);

            if (now >= checkDateTime && (lastCheckTime == null || lastCheckTime.Value.Day != now.Day))
            {
                return true;
            }

            return false;
        }

        private void CheckForNextBirthdays(int daysInFuture)
        {
            var nextBirthdays = birthdayService.GetNextBirthdays(daysInFuture);

            StringBuilder message = new StringBuilder();

            foreach (var birthday in nextBirthdays)
            {
                string date = birthday.ToString();
                if (birthday.IsToday())
                {
                    date = "Heute";
                }
                else if (birthday.IsTomorrow())
                {
                    date = "Morgen";
                }

                message.AppendLine($"{date} - {birthday.Name}");
            }

            var notification = notificationHelper.GetNotification($"Bald haben {nextBirthdays.Count()} Leute Geburtstag", message.ToString());
            notificationHelper.Notify(0, notification);
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            Log.Debug("Job wurde gestoppt.");
            return true; // Reschedule
        }

        private void SaveLastCheckTime(DateTime lastCheckTime)
        {
            string timestamp = lastCheckTime.ToString("u");
            File.WriteAllText(lastCheckTimeFile, timestamp);
        }

        private DateTime? GetLastCheckTime()
        {
            if (!File.Exists(lastCheckTimeFile))
            {
                return null;
            }

            string timeStamp = File.ReadAllText(lastCheckTimeFile);

            return DateTime.ParseExact(timeStamp, "u", CultureInfo.InvariantCulture);
        }
    }
}