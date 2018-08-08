using System;
using System.Collections.Generic;
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

        private const string TAG = "BirthdayCheckJob";
        private NotificationHelper notificationHelper;
        private BirthdayService birthdayService;
        private DateTime? lastCheckTime;

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

            if (ShouldCheck(DateTime.Now, checkTime))
            {
                Task.Run(() =>
                {
                    Log.Debug($"Job wird ausgeführt (CheckTime: {checkTime.hour:00}:{checkTime.minute:00})...");
                    CheckForNextBirthdays(checkDaysInFuture);
                    lastCheckTime = DateTime.Now;

                    Log.Debug("Job erfolgreich ausgeführt.");
                    JobFinished(jobParams, false);
                });

                Log.Debug("Job gestartet.");
            }
            return true;
        }

        private bool ShouldCheck(DateTime now, (int hour, int minute) checkTime)
        {
            // Wenn seit einer Stunde nicht mehr geprüft wurde, auf jeden Fall prüfen
            if (lastCheckTime.HasValue && now < lastCheckTime.Value.AddHours(1))
                return true;
            
            // Prüfen, on die Prüfzeit erreicht ist
            var checkDateTime = new DateTime(now.Year, now.Month, now.Day, checkTime.hour, checkTime.minute, 0);

            return checkDateTime <= now && checkDateTime >= now.AddMinutes(-10);
        }

        private void CheckForNextBirthdays(int daysInFuture)
        {
            var nextBirthdays = birthdayService.GetNextBirthdays(daysInFuture);

            StringBuilder message = new StringBuilder();

            foreach (var birthday in nextBirthdays)
            {
                message.AppendLine($"{birthday.Birthday.ToString("dd.MM.")} - {birthday.Name}");
            }

            var notification = notificationHelper.GetNotification($"Bald haben {nextBirthdays.Count()} Leute Geburtstag", message.ToString());
            notificationHelper.Notify(0, notification);
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            Log.Debug("Job wurde gestoppt.");
            return true; // Reschedule
        }
    }
}